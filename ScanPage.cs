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

    private readonly ProductRepository _repo;
    private readonly DataMatrixCaptureService _dm = new();
    private readonly ExpiryOcrService _ocr = new();
    private readonly OpenFoodFactsClient _off = new();

    private ScanPhase _phase = ScanPhase.AwaitingCode;
    private string? _gtin, _serial, _fullCode;
    private byte[]? _expiryPhoto;
    private Product _current = new();

    private CameraBarcodeReaderView _scanner = null!;
    private VerticalStackLayout _photoPrompt = null!;
    private VerticalStackLayout _loadingView = null!;
    private ScrollView _confirmView = null!;
    private Label _scannerStatus = null!;

    private Image _productImage = null!;
    private Label _noPhotoLabel = null!;
    private Entry _nameEntry = null!, _brandEntry = null!, _volumeEntry = null!;
    private Picker _categoryPicker = null!;
    private DatePicker _datePicker = null!;
    private Label _gtinLabel = null!;

    public ScanPage(ProductRepository repo)
    {
        _repo = repo;
        Title = "Сканирование";
        ThemeManager.BindPage(this);
        BuildUI();
    }

    private void BuildUI()
    {
        // ВКЛЮЧАЕМ РЕЖИМ TryHarder И ПОДДЕРЖКУ ВСЕХ КОДОВ
        _scanner = new CameraBarcodeReaderView 
        { 
            IsDetecting = false,
            IsVisible = true,
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormat.DataMatrix | BarcodeFormat.QrCode | BarcodeFormat.Ean13,
                AutoRotate = true,
                Multiple = false,
                TryHarder = true // Важно для DataMatrix!
            }
        };
        _scanner.BarcodesDetected += OnBarcodeDetected;

        _scannerStatus = new Label 
        { 
            Text = "Наведите на код Честного ЗНАКа (DataMatrix)", 
            TextColor = Colors.White, 
            BackgroundColor = Color.FromRgba(0,0,0,0.7),
            Padding = 10,
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(20, 50, 20, 0)
        };

        _photoPrompt = BuildPhotoPrompt();
        _loadingView = BuildLoadingView();
        _confirmView = BuildConfirmView();

        var overlay = new Grid();
        overlay.Add(_scanner);
        overlay.Add(_scannerStatus);
        overlay.Add(_photoPrompt);
        overlay.Add(_loadingView);
        overlay.Add(_confirmView);

        var mainGrid = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) }
        };
        mainGrid.Add(overlay, 0, 0);
        mainGrid.Add(new BottomNavBar(0, _repo), 0, 1);

        Content = mainGrid;
        UpdateVisibility();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Ошибка", "Нужно разрешение на камеру", "OK");
            await Navigation.PopAsync();
            return;
        }
        _phase = ScanPhase.AwaitingCode;
        UpdateVisibility();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _scanner.IsDetecting = false; 
    }

    private void OnBarcodeDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_phase != ScanPhase.AwaitingCode) return;

        // Берем первый попавшийся код, чтобы показать пользователю, что камера ВООБЩЕ что-то видит
        var hit = e.Results.FirstOrDefault();
        if (hit == null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Показываем, что нашли (для отладки)
            var text = hit.Value.Length > 20 ? hit.Value.Substring(0, 20) + "..." : hit.Value;
            _scannerStatus.Text = $"Найден {hit.Format}:\n{text}";

            // Пытаемся распарсить как ЧЗ
            var parsed = _dm.TryParse(hit.Value);
            if (parsed != null)
            {
                _gtin = parsed.Value.Gtin;
                _serial = parsed.Value.Serial;
                _fullCode = parsed.Value.FullCode;

                _scannerStatus.Text = $"Код ЧЗ распознан!\nGTIN: {_gtin}";
                _scanner.IsDetecting = false;
                
                // Задержка 1 сек, чтобы пользователь увидел успех
                Task.Delay(1000).ContinueWith(_ => 
                {
                    MainThread.BeginInvokeOnMainThread(() => 
                    {
                        _phase = ScanPhase.AwaitingPhoto;
                        UpdateVisibility();
                    });
                });
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
        try
        {
            if (_expiryPhoto != null)
                expiry = await _ocr.ReadFromPhotoAsync(_expiryPhoto);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"OCR Error: {ex.Message}"); }

        ProductInfo? info = null;
        if (!string.IsNullOrEmpty(_gtin))
            info = await _off.TryGetByGtinAsync(_gtin);

        string? localPath = null;
        if (info?.ImageUrl != null && !string.IsNullOrEmpty(_serial))
            localPath = await ProductImageService.DownloadToCacheAsync(info.ImageUrl, _serial);

        _current = new Product
        {
            Name = info?.Name ?? "",
            Brand = info?.Brand ?? "",
            Volume = info?.Volume ?? "",
            Barcode = _gtin ?? "",
            Gtin = _gtin,
            Serial = _serial,
            ExpiryDate = expiry ?? DateTime.Today.AddDays(7),
            RemoteImageUrl = info?.ImageUrl,
            LocalImagePath = localPath
        };

        _nameEntry.Text = _current.Name;
        _brandEntry.Text = _current.Brand;
        _volumeEntry.Text = _current.Volume;
        _datePicker.Date = _current.ExpiryDate;
        _gtinLabel.Text = $"GTIN: {_gtin}\nSerial: {_serial}";

        if (_current.HasLocalImage)
        {
            _productImage.Source = ImageSource.FromFile(_current.LocalImagePath);
            _productImage.IsVisible = true;
            _noPhotoLabel.IsVisible = false;
        }
        else
        {
            _productImage.IsVisible = false;
            _noPhotoLabel.IsVisible = true;
        }

        _phase = ScanPhase.Confirming;
        UpdateVisibility();
    }

    private async void OnSave(object? s, EventArgs e)
    {
        _current.Name = _nameEntry.Text ?? "";
        _current.Brand = _brandEntry.Text ?? "";
        _current.Volume = _volumeEntry.Text ?? "";
        _current.Category = _categoryPicker.SelectedItem as string ?? "Другое";
        _current.ExpiryDate = _datePicker.Date;

        await _repo.Save(_current);
        await Navigation.PopAsync();
    }

    private void UpdateVisibility()
    {
        _scanner.IsVisible = _phase == ScanPhase.AwaitingCode;
        _scannerStatus.IsVisible = _phase == ScanPhase.AwaitingCode;
        _photoPrompt.IsVisible = _phase == ScanPhase.AwaitingPhoto;
        _loadingView.IsVisible = _phase == ScanPhase.Processing;
        _confirmView.IsVisible = _phase == ScanPhase.Confirming;
        
        if (_phase == ScanPhase.AwaitingCode) 
        {
            _scannerStatus.Text = "Наведите на код Честного ЗНАКа (DataMatrix)";
            _scanner.IsDetecting = true;
        }
    }

    private VerticalStackLayout BuildPhotoPrompt()
    {
        var title = new Label { Text = "Код распознан (или пропущен)", FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
        var subtitle = new Label { Text = "Сфотографируйте срок годности (желательно крупно)", FontSize = 16, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };

        var btn = new Button { Text = "📷 Сфотографировать дату", CornerRadius = 14, HeightRequest = 56, HorizontalOptions = LayoutOptions.Fill, Margin = new Thickness(0, 16, 0, 0) };
        btn.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Accent);
        btn.SetDynamicResource(Button.TextColorProperty, ThemeManager.AccentText);
        btn.Clicked += OnTakeExpiryPhoto;

        return new VerticalStackLayout
        {
            Spacing = 12, Padding = 24,
            VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = Color.FromRgba(0, 0, 0, 0.8),
            Children = { title, subtitle, btn }
        };
    }

    private VerticalStackLayout BuildLoadingView()
    {
        return new VerticalStackLayout
        {
            Spacing = 16, Padding = 24,
            VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = Color.FromRgba(0, 0, 0, 0.85),
            Children =
            {
                new ActivityIndicator { IsRunning = true, Color = ThemeManager.AccentColor, HeightRequest = 48, WidthRequest = 48 },
                new Label { Text = "Ищем дату на фото...", FontSize = 15, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center }
            }
        };
    }

    private ScrollView BuildConfirmView()
    {
        _productImage = new Image { HeightRequest = 180, Aspect = Aspect.AspectFit, IsVisible = false };
        _noPhotoLabel = new Label { Text = "Нет фото товара", FontSize = 14, TextColor = Colors.Gray, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };

        var photoGrid = new Grid { HeightRequest = 180 };
        photoGrid.Add(_noPhotoLabel);
        photoGrid.Add(_productImage);

        var photoBorder = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = 8, Content = photoGrid };
        photoBorder.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);
        photoBorder.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);

        _gtinLabel = new Label { FontSize = 12 };
        ThemeManager.BindText(_gtinLabel, secondary: true);

        _nameEntry = CreateEntry("Название");
        _brandEntry = CreateEntry("Бренд");
        _volumeEntry = CreateEntry("Объём (0.5 л)");

        _categoryPicker = new Picker { Title = "Категория" };
        _categoryPicker.ItemsSource = new List<string> { "Молочные", "Напитки", "Мясо и рыба", "Овощи и фрукты", "Заморозка", "Другое" };
        _categoryPicker.SelectedIndex = 5;

        _datePicker = new DatePicker { Date = DateTime.Today.AddDays(7) };

        var saveBtn = new Button { Text = "Сохранить", CornerRadius = 14, HeightRequest = 56, Margin = new Thickness(0, 8, 0, 0) };
        saveBtn.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Accent);
        saveBtn.SetDynamicResource(Button.TextColorProperty, ThemeManager.AccentText);
        saveBtn.Clicked += OnSave;

        return new ScrollView
        {
            Content = new VerticalStackLayout { Padding = 18, Spacing = 10, Children = { photoBorder, _gtinLabel, _nameEntry, _brandEntry, _volumeEntry, _categoryPicker, _datePicker, saveBtn } }
        };
    }

    private Entry CreateEntry(string placeholder)
    {
        var e = new Entry { Placeholder = placeholder, HeightRequest = 50 };
        ThemeManager.BindEntry(e);
        return e;
    }
}
