using BarcodeScanning;
using FoodExpiryApp.Models;
using FoodExpiryApp.Services;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

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

    // === Основные вью ===
    private CameraView _cameraView = null!;
    private Grid _photoPromptOverlay = null!;
    private VerticalStackLayout _loadingView = null!;
    private Grid _confirmViewOverlay = null!;
    private Label _scannerStatus = null!;
    private Button _skipBtn = null!;
    private Border _scanFrame = null!;

    // === Элементы формы ===
    private Image _productImage = null!;
    private Label _noPhotoLabel = null!;
    private Label _gtinLabel = null!;
    private Entry _nameEntry = null!;
    private Entry _brandEntry = null!;
    private Entry _volumeEntry = null!;
    private Entry _priceEntry = null!;
    private Picker _categoryPicker = null!;
    private DatePicker _datePicker = null!;

    public ScanPage(ProductRepository repo)
    {
        _repo = repo;
        Title = "Сканирование";
        ThemeManager.BindPage(this);
        BuildUI();
    }

    private void BuildUI()
    {
        // ✅ CAMERA VIEW (ML Kit) вместо ZXing
        _cameraView = new CameraView
        {
            CameraEnabled = false,
            VibrationOnDetected = true,
            CaptureQuality = CaptureQuality.High,
            BarcodeSymbologies =
                BarcodeSymbologies.FormatDataMatrix |
                BarcodeSymbologies.FormatQrCode |
                BarcodeSymbologies.FormatEan13
        };
        _cameraView.OnDetectionFinished += OnBarcodeDetected;

        _scannerStatus = new Label
        {
            Text = "Наведите на код ЧЗ",
            TextColor = Colors.White,
            BackgroundColor = Color.FromRgba(0, 0, 0, 0.6),
            Padding = 10,
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(20, 50, 20, 0)
        };

        _scanFrame = new Border
        {
            WidthRequest = 260,
            HeightRequest = 260,
            Stroke = ThemeManager.AccentColor,
            StrokeThickness = 4,
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        var scanHint = new Label
        {
            Text = "Поместите код в рамку",
            TextColor = Colors.White,
            BackgroundColor = Color.FromRgba(0, 0, 0, 0.6),
            Padding = 8,
            FontSize = 13,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 300, 0, 0)
        };

        _skipBtn = new Button
        {
            Text = "Сфоткать дату без кода",
            BackgroundColor = ThemeManager.AccentColor,
            TextColor = Colors.White,
            CornerRadius = 14,
            HeightRequest = 50,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(20, 0, 20, 30)
        };
        _skipBtn.Clicked += OnSkipCode;

        _photoPromptOverlay = BuildPhotoPrompt();
        _loadingView = BuildLoadingView();
        _confirmViewOverlay = BuildConfirmView();

        var overlay = new Grid();
        overlay.Add(_cameraView);
        overlay.Add(_scanFrame);
        overlay.Add(scanHint);
        overlay.Add(_scannerStatus);
        overlay.Add(_skipBtn);
        overlay.Add(_photoPromptOverlay);
        overlay.Add(_loadingView);
        overlay.Add(_confirmViewOverlay);

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
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

        if (_phase == ScanPhase.AwaitingCode)
        {
            _cameraView.CameraEnabled = true;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _cameraView.CameraEnabled = false;
    }

    // ✅ Обработчик ML Kit
    private void OnBarcodeDetected(object? sender, OnDetectionFinishedEventArg e)
    {
        if (e.BarcodeResults == null || e.BarcodeResults.Length == 0) return;

        var result = e.BarcodeResults.FirstOrDefault();
        if (result == null) return;

        var raw = result.RawValue ?? "";

        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Фиксируем камеру (она стопорится, картинка замирает)
            _cameraView.CameraEnabled = false;

            var parsed = _dm.TryParse(raw);

            if (parsed != null)
            {
                _gtin = parsed.Value.Gtin;
                _serial = parsed.Value.Serial;
            }
            else if (raw.Length > 0 && raw.All(char.IsDigit))
            {
                _gtin = raw; // EAN13
            }
            else
            {
                // Не ЧЗ и не EAN13 — продолжаем сканировать
                _cameraView.CameraEnabled = true;
                return;
            }

            _phase = ScanPhase.AwaitingPhoto;
            UpdateVisibility();
        });
    }

    private void OnSkipCode(object? s, EventArgs e)
    {
        _cameraView.CameraEnabled = false;
        _phase = ScanPhase.AwaitingPhoto;
        UpdateVisibility();
    }

    private async void OnTakeExpiryPhoto(object? s, EventArgs e)
    {
        try
        {
            var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
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
            await DisplayAlert("Ошибка камеры", ex.Message, "OK");
            _phase = ScanPhase.AwaitingPhoto;
            UpdateVisibility();
        }
    }

    private async Task ProcessAsync()
    {
        DateTime? expiry = null;
        try
        {
            if (_expiryPhoto != null) expiry = await _ocr.ReadFromPhotoAsync(_expiryPhoto);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OCR Error: {ex.Message}");
        }

        ProductInfo? info = null;
        if (!string.IsNullOrEmpty(_gtin)) info = await _off.TryGetByGtinAsync(_gtin);

        string? localPath = null;
        if (info?.ImageUrl != null && !string.IsNullOrEmpty(_serial))
        {
            var imgService = IPlatformApplication.Current!.Services.GetService<ProductImageService>();
            if (imgService != null) localPath = await imgService.DownloadToCacheAsync(info.ImageUrl, _serial);
        }

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

        if (expiry == null)
            _gtinLabel.Text += "\n⚠️ Дата не распознана, проверьте вручную.";

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

        if (double.TryParse(_priceEntry.Text, out double price))
            _current.Price = price;
        else
            _current.Price = 0;

        await _repo.Save(_current);
        await Navigation.PopAsync();
    }

    private void UpdateVisibility()
    {
        bool isScanning = _phase == ScanPhase.AwaitingCode;

        _cameraView.IsVisible = isScanning;
        _scanFrame.IsVisible = isScanning;
        _scannerStatus.IsVisible = isScanning;
        _skipBtn.IsVisible = isScanning;

        _photoPromptOverlay.IsVisible = _phase == ScanPhase.AwaitingPhoto;
        _loadingView.IsVisible = _phase == ScanPhase.Processing;
        _confirmViewOverlay.IsVisible = _phase == ScanPhase.Confirming;

        if (isScanning)
        {
            _scannerStatus.Text = "Наведите на код ЧЗ";
            _cameraView.CameraEnabled = true;
        }
    }

    private Grid BuildPhotoPrompt()
    {
        var title = new Label
        {
            Text = "Код распознан",
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalTextAlignment = TextAlignment.Center
        };
        var subtitle = new Label
        {
            Text = "Сфотографируйте срок годности",
            FontSize = 15,
            TextColor = Color.FromArgb("#B7B2C1"),
            HorizontalTextAlignment = TextAlignment.Center
        };
        var btn = new Button
        {
            Text = "📷 Сфотографировать",
            CornerRadius = 14,
            HeightRequest = 52,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 16, 0, 0)
        };
        btn.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Accent);
        btn.SetDynamicResource(Button.TextColorProperty, ThemeManager.AccentText);
        btn.Clicked += OnTakeExpiryPhoto;

        var card = new Border
        {
            StrokeThickness = 2,
            Stroke = ThemeManager.AccentColor,
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            BackgroundColor = Color.FromArgb("#201D28"),
            Padding = 24,
            WidthRequest = 320,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                Children = { title, subtitle, btn }
            }
        };

        return new Grid
        {
            BackgroundColor = Color.FromRgba(0, 0, 0, 0.85),
            Children = { card }
        };
    }

    private VerticalStackLayout BuildLoadingView()
    {
        return new VerticalStackLayout
        {
            Spacing = 16,
            Padding = 24,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = Color.FromRgba(0, 0, 0, 0.85),
            Children =
            {
                new ActivityIndicator
                {
                    IsRunning = true,
                    Color = ThemeManager.AccentColor,
                    HeightRequest = 48,
                    WidthRequest = 48
                },
                new Label
                {
                    Text = "Ищем дату...",
                    FontSize = 15,
                    TextColor = Colors.White
                }
            }
        };
    }

    private Grid BuildConfirmView()
    {
        _productImage = new Image { HeightRequest = 140, Aspect = Aspect.AspectFit, IsVisible = false };
        _noPhotoLabel = new Label
        {
            Text = "Нет фото",
            FontSize = 13,
            TextColor = Colors.Gray,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };

        var photoGrid = new Grid { HeightRequest = 140 };
        photoGrid.Add(_noPhotoLabel);
        photoGrid.Add(_productImage);

        var photoBorder = new Border
        {
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = 6,
            Content = photoGrid
        };
        photoBorder.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);
        photoBorder.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);

        var captureBtn = new Button
        {
            Text = "📷 Сфоткать упаковку",
            BackgroundColor = Colors.Transparent,
            TextColor = ThemeManager.AccentColor,
            BorderColor = ThemeManager.AccentColor,
            BorderWidth = 1,
            CornerRadius = 10,
            HeightRequest = 38,
            FontSize = 13
        };
        captureBtn.Clicked += async (_, _) =>
        {
            var imgService = IPlatformApplication.Current!.Services.GetService<ProductImageService>();
            if (imgService == null) return;
            var path = await imgService.CaptureOwnPhotoAsync(_current.Serial ?? "unknown");
            if (path != null)
            {
                _current.LocalImagePath = path;
                _productImage.Source = ImageSource.FromFile(path);
                _productImage.IsVisible = true;
                _noPhotoLabel.IsVisible = false;
            }
        };

        _gtinLabel = new Label { FontSize = 11 };
        ThemeManager.BindText(_gtinLabel, secondary: true);

        _nameEntry = new Entry { Placeholder = "Название", HeightRequest = 46 };
        ThemeManager.BindEntry(_nameEntry);

        _brandEntry = new Entry { Placeholder = "Бренд", HeightRequest = 46 };
        ThemeManager.BindEntry(_brandEntry);

        _volumeEntry = new Entry { Placeholder = "Объём (0.5 л)", HeightRequest = 46 };
        ThemeManager.BindEntry(_volumeEntry);

        _priceEntry = new Entry { Placeholder = "Цена (руб)", HeightRequest = 46, Keyboard = Keyboard.Numeric };
        ThemeManager.BindEntry(_priceEntry);

        _categoryPicker = new Picker { Title = "Категория" };
        _categoryPicker.ItemsSource = new List<string> { "Молочные", "Напитки", "Мясо и рыба", "Овощи и фрукты", "Заморозка", "Другое" };
        _categoryPicker.SelectedIndex = 5;

        _datePicker = new DatePicker { Date = DateTime.Today.AddDays(7) };

        var saveBtn = new Button { Text = "Сохранить", CornerRadius = 14, HeightRequest = 52, Margin = new Thickness(0, 8, 0, 0) };
        saveBtn.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Accent);
        saveBtn.SetDynamicResource(Button.TextColorProperty, ThemeManager.AccentText);
        saveBtn.Clicked += OnSave;

        var formStack = new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                photoBorder, captureBtn, _gtinLabel,
                _nameEntry, _brandEntry, _volumeEntry, _priceEntry,
                _categoryPicker, _datePicker, saveBtn
            }
        };

        var card = new Border
        {
            StrokeThickness = 2,
            Stroke = ThemeManager.AccentColor,
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            BackgroundColor = Color.FromArgb("#201D28"),
            Padding = 16,
            Margin = new Thickness(16),
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            Content = new ScrollView { Content = formStack }
        };

        return new Grid
        {
            BackgroundColor = Color.FromRgba(0, 0, 0, 0.85),
            Children = { card }
        };
    }
}
