using FoodExpiryApp.Models;
using FoodExpiryApp.Services;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace FoodExpiryApp;

public class ScanPage : ContentPage
{
    private enum ScanPhase { AwaitingCode, AwaitingPhoto, Processing, Confirming }
    private ScanPhase _phase = ScanPhase.AwaitingCode;

    private readonly ProductRepository _repo;
    private readonly DataMatrixCaptureService _dm = new();
    private readonly ExpiryOcrService _ocr = new();
    private readonly OpenFoodFactsClient _off = new();

    private string? _gtin, _serial;
    private byte[]? _expiryPhoto;
    private Product _current = new();

    private CameraBarcodeReaderView _scanner = null!;
    private VerticalStackLayout _photoPrompt = null!;
    private VerticalStackLayout _loadingView = null!;
    private ScrollView _confirmView = null!;
    private Label _scannerStatus = null!;
    private Button _skipBtn = null!;

    public ScanPage(ProductRepository repo)
    {
        _repo = repo;
        Title = "Сканирование";
        ThemeManager.BindPage(this);
        BuildUI();
    }

    private void BuildUI()
    {
        _scanner = new CameraBarcodeReaderView { IsDetecting = false, IsVisible = true, Options = new BarcodeReaderOptions { Formats = BarcodeFormat.DataMatrix | BarcodeFormat.QrCode | BarcodeFormat.Ean13, AutoRotate = true, Multiple = false, TryHarder = true } };
        _scanner.BarcodesDetected += OnBarcodeDetected;

        _scannerStatus = new Label { Text = "Наведите на код ЧЗ", TextColor = Colors.White, BackgroundColor = Color.FromRgba(0,0,0,0.7), Padding = 10, FontSize = 14, HorizontalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(20, 50, 20, 0) };

        // Кнопка принудительного перехода к фото
        _skipBtn = new Button { Text = " Сфоткать дату без кода", BackgroundColor = ThemeManager.AccentColor, TextColor = Colors.White, CornerRadius = 14, HeightRequest = 50, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.End, Margin = new Thickness(20, 0, 20, 30) };
        _skipBtn.Clicked += OnSkipCode;

        _photoPrompt = BuildPhotoPrompt();
        _loadingView = BuildLoadingView();
        _confirmView = BuildConfirmView();

        var overlay = new Grid();
        overlay.Add(_scanner);
        overlay.Add(_scannerStatus);
        overlay.Add(_skipBtn); // Добавляем кнопку поверх камеры
        overlay.Add(_photoPrompt);
        overlay.Add(_loadingView);
        overlay.Add(_confirmView);

        var mainGrid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
        mainGrid.Add(overlay, 0, 0);
        mainGrid.Add(new BottomNavBar(0, _repo), 0, 1);

        Content = mainGrid;
        UpdateVisibility();
    }

    private async void OnTakeExpiryPhoto(object? s, EventArgs e)
{
    try
    {
        // Запрашиваем разрешения перед съёмкой
        var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
        var storageStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();
        
        if (cameraStatus != PermissionStatus.Granted)
        {
            await DisplayAlert("Ошибка", "Нужно разрешение на камеру", "OK");
            return;
        }

        if (!MediaPicker.Default.IsCaptureSupported)
        {
            await DisplayAlert("Ошибка", "Камера не поддерживается", "OK");
            return;
        }

        var photo = await MediaPicker.Default.CapturePhotoAsync();
        if (photo == null) return;

        await using var stream = await photo.OpenReadAsync();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        _expiryPhoto = ms.ToArray();

        _phase = ScanPhase.Processing;
        UpdateVisibility();
        await ProcessAsync();
    }
    catch (Exception ex)
    {
        // Показываем ошибку вместо вылета
        await DisplayAlert("Ошибка камеры", ex.Message, "OK");
        _phase = ScanPhase.AwaitingPhoto;
        UpdateVisibility();
    }
}
    protected override void OnDisappearing() { base.OnDisappearing(); _scanner.IsDetecting = false; }

    private void OnBarcodeDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_phase != ScanPhase.AwaitingCode) return;
        var hit = e.Results.FirstOrDefault();
        if (hit == null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var parsed = _dm.TryParse(hit.Value);
            if (parsed != null)
            {
                _gtin = parsed.Value.Gtin;
                _serial = parsed.Value.Serial;
                _scannerStatus.Text = $"Код ЧЗ распознан!\nGTIN: {_gtin}";
                _scanner.IsDetecting = false;
                Task.Delay(800).ContinueWith(_ => MainThread.BeginInvokeOnMainThread(() => { _phase = ScanPhase.AwaitingPhoto; UpdateVisibility(); }));
            }
            else
            {
                _scannerStatus.Text = $"Найден {hit.Format}, но не ЧЗ";
            }
        });
    }

    private void OnSkipCode(object? s, EventArgs e)
    {
        _scanner.IsDetecting = false;
        _phase = ScanPhase.AwaitingPhoto;
        UpdateVisibility();
    }

    private async void OnTakeExpiryPhoto(object? s, EventArgs e)
    {
        if (!MediaPicker.Default.IsCaptureSupported) return;
        var photo = await MediaPicker.Default.CapturePhotoAsync();
        if (photo == null) return;

        await using var stream = await photo.OpenReadAsync();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        _expiryPhoto = ms.ToArray();

        _phase = ScanPhase.Processing;
        UpdateVisibility();
        await ProcessAsync();
    }

    private async Task ProcessAsync()
    {
        DateTime? expiry = null;
        try { if (_expiryPhoto != null) expiry = await _ocr.ReadFromPhotoAsync(_expiryPhoto); } catch { }

        ProductInfo? info = null;
        if (!string.IsNullOrEmpty(_gtin)) info = await _off.TryGetByGtinAsync(_gtin);

        string? localPath = null;
        if (info?.ImageUrl != null && !string.IsNullOrEmpty(_serial))
        {
            var imgService = IPlatformApplication.Current!.Services.GetService<ProductImageService>();
            if (imgService != null) localPath = await imgService.DownloadToCacheAsync(info.ImageUrl, _serial);
        }

        _current = new Product { Name = info?.Name ?? "", Brand = info?.Brand ?? "", Volume = info?.Volume ?? "", Barcode = _gtin ?? "", Gtin = _gtin, Serial = _serial, ExpiryDate = expiry ?? DateTime.Today.AddDays(7), RemoteImageUrl = info?.ImageUrl, LocalImagePath = localPath };
        
        // Заполнение UI
        ((Entry)_confirmView.Content.FindByName("_nameEntry")).Text = _current.Name;
        ((Entry)_confirmView.Content.FindByName("_brandEntry")).Text = _current.Brand;
        ((Entry)_confirmView.Content.FindByName("_volumeEntry")).Text = _current.Volume;
        ((DatePicker)_confirmView.Content.FindByName("_datePicker")).Date = _current.ExpiryDate;
        ((Label)_confirmView.Content.FindByName("_gtinLabel")).Text = $"GTIN: {_gtin}\nSerial: {_serial}";
        var img = (Image)_confirmView.Content.FindByName("_productImage");
        var noImg = (Label)_confirmView.Content.FindByName("_noPhotoLabel");
        if (_current.HasLocalImage) { img.Source = ImageSource.FromFile(_current.LocalImagePath); img.IsVisible = true; noImg.IsVisible = false; }
        else { img.IsVisible = false; noImg.IsVisible = true; }

        _phase = ScanPhase.Confirming;
        UpdateVisibility();
    }

    private async void OnSave(object? s, EventArgs e)
    {
        _current.Name = ((Entry)_confirmView.Content.FindByName("_nameEntry")).Text ?? "";
        _current.Brand = ((Entry)_confirmView.Content.FindByName("_brandEntry")).Text ?? "";
        _current.Volume = ((Entry)_confirmView.Content.FindByName("_volumeEntry")).Text ?? "";
        _current.Category = ((Picker)_confirmView.Content.FindByName("_categoryPicker")).SelectedItem as string ?? "Другое";
        _current.ExpiryDate = ((DatePicker)_confirmView.Content.FindByName("_datePicker")).Date;

        await _repo.Save(_current);
        await Navigation.PopAsync();
    }

    private void UpdateVisibility()
    {
        _scanner.IsVisible = _phase == ScanPhase.AwaitingCode;
        _scannerStatus.IsVisible = _phase == ScanPhase.AwaitingCode;
        _skipBtn.IsVisible = _phase == ScanPhase.AwaitingCode;
        _photoPrompt.IsVisible = _phase == ScanPhase.AwaitingPhoto;
        _loadingView.IsVisible = _phase == ScanPhase.Processing;
        _confirmView.IsVisible = _phase == ScanPhase.Confirming;
        if (_phase == ScanPhase.AwaitingCode) { _scannerStatus.Text = "Наведите на код ЧЗ"; _scanner.IsDetecting = true; }
    }

    private VerticalStackLayout BuildPhotoPrompt()
    {
        var title = new Label { Text = "Код распознан", FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
        var subtitle = new Label { Text = "Сфотографируйте срок годности", FontSize = 16, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
        var btn = new Button { Text = "Сфотографировать", CornerRadius = 14, HeightRequest = 56, HorizontalOptions = LayoutOptions.Fill, Margin = new Thickness(0, 16, 0, 0) };
        btn.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Accent);
        btn.SetDynamicResource(Button.TextColorProperty, ThemeManager.AccentText);
        btn.Clicked += OnTakeExpiryPhoto;
        return new VerticalStackLayout { Spacing = 12, Padding = 24, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, BackgroundColor = Color.FromRgba(0, 0, 0, 0.8), Children = { title, subtitle, btn } };
    }

    private VerticalStackLayout BuildLoadingView() { return new VerticalStackLayout { Spacing = 16, Padding = 24, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, BackgroundColor = Color.FromRgba(0, 0, 0, 0.85), Children = { new ActivityIndicator { IsRunning = true, Color = ThemeManager.AccentColor, HeightRequest = 48, WidthRequest = 48 }, new Label { Text = "Ищем дату...", FontSize = 15, TextColor = Colors.White } } }; }

    private ScrollView BuildConfirmView()
    {
        var productImage = new Image { HeightRequest = 180, Aspect = Aspect.AspectFit, IsVisible = false, AutomationId = "_productImage" };
        var noPhotoLabel = new Label { Text = "Нет фото", FontSize = 14, TextColor = Colors.Gray, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, AutomationId = "_noPhotoLabel" };
        var photoGrid = new Grid { HeightRequest = 180 }; photoGrid.Add(noPhotoLabel); photoGrid.Add(productImage);
        var photoBorder = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = 8, Content = photoGrid };
        photoBorder.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border); photoBorder.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);

        var captureBtn = new Button { Text = "Сфоткать упаковку", BackgroundColor = Colors.Transparent, TextColor = ThemeManager.AccentColor, BorderColor = ThemeManager.AccentColor, BorderWidth = 1, CornerRadius = 10, HeightRequest = 40 };
        captureBtn.Clicked += async (_, _) => { var imgService = IPlatformApplication.Current!.Services.GetService<ProductImageService>(); if (imgService == null) return; var path = await imgService.CaptureOwnPhotoAsync(_current.Serial ?? "unknown"); if (path != null) { _current.LocalImagePath = path; productImage.Source = ImageSource.FromFile(path); productImage.IsVisible = true; noPhotoLabel.IsVisible = false; } };

        var gtinLabel = new Label { FontSize = 12, AutomationId = "_gtinLabel" }; ThemeManager.BindText(gtinLabel, secondary: true);
        var nameEntry = new Entry { Placeholder = "Название", HeightRequest = 50, AutomationId = "_nameEntry" }; ThemeManager.BindEntry(nameEntry);
        var brandEntry = new Entry { Placeholder = "Бренд", HeightRequest = 50, AutomationId = "_brandEntry" }; ThemeManager.BindEntry(brandEntry);
        var volumeEntry = new Entry { Placeholder = "Объём (0.5 л)", HeightRequest = 50, AutomationId = "_volumeEntry" }; ThemeManager.BindEntry(volumeEntry);

        var categoryPicker = new Picker { Title = "Категория", AutomationId = "_categoryPicker" };
        categoryPicker.ItemsSource = new List<string> { "Молочные", "Напитки", "Мясо и рыба", "Овощи и фрукты", "Заморозка", "Другое" };
        categoryPicker.SelectedIndex = 5;

        var datePicker = new DatePicker { Date = DateTime.Today.AddDays(7), AutomationId = "_datePicker" };
        var saveBtn = new Button { Text = "Сохранить", CornerRadius = 14, HeightRequest = 56, Margin = new Thickness(0, 8, 0, 0) };
        saveBtn.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Accent);
        saveBtn.SetDynamicResource(Button.TextColorProperty, ThemeManager.AccentText);
        saveBtn.Clicked += OnSave;

        return new ScrollView { Content = new VerticalStackLayout { Padding = 18, Spacing = 10, Children = { photoBorder, captureBtn, gtinLabel, nameEntry, brandEntry, volumeEntry, categoryPicker, datePicker, saveBtn } } };
    }
}
