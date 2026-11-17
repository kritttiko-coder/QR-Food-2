using FoodExpiryApp.Models;
using FoodExpiryApp.Services;
using FoodExpiryApp.Services.Maps;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Layouts;
using CommunityToolkit.Maui.Behaviors;
using System.Text.Json;
using System.Globalization;

namespace FoodExpiryApp;

public class StorePage : ContentPage
{
    private readonly ProductRepository _repo;
    private readonly StoreService _storeService;
    private readonly PriceService? _priceService;
    private readonly StoreSearchService _storeSearch;

    private VerticalStackLayout _shopItemsContainer = null!;
    private VerticalStackLayout _storesListLayout = null!;
    private Label _locText = null!;
    private Grid _rootGrid = null!;
    private Controls.MiniMapView? _miniMap;
    private Location? _currentLocation;

    private List<ShoppingItem> _shoppingItems = new();
    private IReadOnlyList<NearbyStore> _nearbyStores = Array.Empty<NearbyStore>();
    private string _searchText = "";
    private const string ShoppingListKey = "shopping_list";
    private const string CustomTemplatesKey = "store_custom_templates";

    private sealed class SavedTemplate
    {
        public string Name { get; set; } = "";
        public List<ShoppingItem> Items { get; set; } = new();
    }

    public StorePage(ProductRepository repo)
    {
        _repo = repo;
        _storeService = IPlatformApplication.Current?.Services.GetService<StoreService>()
            ?? new StoreService();
        _priceService = IPlatformApplication.Current?.Services.GetService<PriceService>();

        // StoreSearchService может быть не зарегистрирован в DI — тогда пустой список провайдеров
        _storeSearch = IPlatformApplication.Current?.Services.GetService<StoreSearchService>()
            ?? new StoreSearchService(
                Enumerable.Empty<IPlacesProvider>(),
                new PlacesCache());

        Title = Loc.T("StoreTitle");
        ThemeManager.BindPage(this);
        LoadShoppingList();
        BuildUI();
    }

    private void LoadShoppingList()
    {
        try
        {
            var json = Preferences.Default.Get(ShoppingListKey, "");
            _shoppingItems = string.IsNullOrWhiteSpace(json)
                ? new List<ShoppingItem>
                {
                    new() { Name = "Молоко", Quantity = "2 шт.", Emoji = "🥛" },
                    new() { Name = "Хлеб", Quantity = "1 шт.", Emoji = "🍞" },
                    new() { Name = "Яйца", Quantity = "1 уп.", Emoji = "🥚" },
                    new() { Name = "Яблоки", Quantity = "1 кг", Emoji = "🍎" },
                    new() { Name = "Вода", Quantity = "2 шт.", Emoji = "💧" }
                }
                : JsonSerializer.Deserialize<List<ShoppingItem>>(json) ?? new List<ShoppingItem>();
        }
        catch
        {
            _shoppingItems = new List<ShoppingItem>();
        }

        SaveShoppingList();
    }

    private void SaveShoppingList()
    {
        try
        {
            Preferences.Default.Set(ShoppingListKey, JsonSerializer.Serialize(_shoppingItems));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Shopping list save error: {ex}");
        }
    }

    private void BuildUI()
    {
        BackgroundColor = Color.FromArgb("#090811");

        // === ШАПКА ===
        var header = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12
        };

        var title = new Label
        {
            Text = Loc.T("StoreTitle"),
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            TextColor = Colors.White
        };

        var locationRow = new HorizontalStackLayout { Spacing = 5, VerticalOptions = LayoutOptions.Center };
        locationRow.Children.Add(CreateTintedIcon("icon_pin.png", 19));
        _locText = new Label
        {
            Text = "Поиск GPS...",
            TextColor = Color.FromArgb("#C99AFF"),
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center
        };
        locationRow.Children.Add(_locText);
        locationRow.Children.Add(CreateTintedIcon("icon_chevron_down.png", 13));
        locationRow.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await ShowCityPicker())
        });

        var mapButton = CreateTintedIcon("icon_map.png", 27);
        mapButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await OpenSelectedMap())
        });

        header.Add(title, 0, 0);
        header.Add(locationRow, 1, 0);
        header.Add(mapButton, 2, 0);

        // === ПОИСК ===
        var search = new SearchBar
        {
            Placeholder = "Поиск по магазинам или продуктам",
            HeightRequest = 54,
            FontSize = 15,
            CancelButtonColor = Color.FromArgb("#A978F5")
        };
        ThemeManager.BindSearch(search);
        var searchFrame = new Border
        {
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#29253A"),
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            BackgroundColor = Color.FromArgb("#191727"),
            Padding = new Thickness(8, 0),
            Content = search
        };
        search.TextChanged += (_, e) =>
        {
            _searchText = e.NewTextValue?.Trim() ?? "";
            RenderShoppingList();
            RenderStores();
        };

        // === ОСНОВНАЯ СЕТКА ===
        var mainGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(0.45, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(0.55, GridUnitType.Star))
            },
            ColumnSpacing = 10,
            HeightRequest = 590
        };

        // Левая панель — список покупок
        _shopItemsContainer = new VerticalStackLayout { Spacing = 17 };
        var shoppingHeader = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(0, 0, 0, 8) };
        shoppingHeader.Children.Add(CreateTintedIcon("icon_basket.png", 23));
        shoppingHeader.Children.Add(new Label
        {
            Text = Loc.T("ShoppingList"),
            FontSize = 16,
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center
        });

        var listScroll = new ScrollView
        {
            Content = _shopItemsContainer,
            VerticalOptions = LayoutOptions.Fill,
            VerticalScrollBarVisibility = ScrollBarVisibility.Never
        };

        var addButton = CreateOutlineButton("+  Добавить", Color.FromArgb("#B66AFF"));
        addButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await ShowAddItemPopup())
        });

        var deleteButton = CreateOutlineButton("▤  Удалить", Color.FromArgb("#FF354F"));
        deleteButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await DeleteCheckedItems())
        });

        var listActions = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 7,
            Margin = new Thickness(0, 12, 0, 0)
        };
        listActions.Add(addButton, 0, 0);
        listActions.Add(deleteButton, 1, 0);

        var leftContent = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };
        leftContent.Add(shoppingHeader, 0, 0);
        leftContent.Add(listScroll, 0, 1);
        leftContent.Add(listActions, 0, 2);

        var leftPanel = new Border
        {
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#29253A"),
            StrokeShape = new RoundRectangle { CornerRadius = 23 },
            BackgroundColor = Color.FromArgb("#12111E"),
            Padding = new Thickness(12),
            Content = leftContent
        };
        mainGrid.Add(leftPanel, 0, 0);

        // Правая колонка — карта + магазины
        var rightGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1.45, GridUnitType.Star)),
                new RowDefinition(new GridLength(1, GridUnitType.Star))
            },
            RowSpacing = 10
        };

        // ✅ НАСТОЯЩАЯ КАРТА (MiniMapView)
        var mapProviders = IPlatformApplication.Current?.Services
            .GetServices<IMapProvider>()?.ToList() ?? new List<IMapProvider>();

        _miniMap = new Controls.MiniMapView(mapProviders);

        var mapContainer = new Border
        {
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#29253A"),
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            Content = _miniMap
        };
        rightGrid.Add(mapContainer, 0, 0);

        // Магазины под картой
        _storesListLayout = new VerticalStackLayout
        {
            Spacing = 2,
            Padding = new Thickness(12, 10),
            VerticalOptions = LayoutOptions.Fill
        };
        var storesPanel = new Border
        {
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#29253A"),
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            BackgroundColor = Color.FromArgb("#151321"),
            Content = new ScrollView { Content = _storesListLayout, VerticalScrollBarVisibility = ScrollBarVisibility.Never }
        };
        rightGrid.Add(storesPanel, 0, 1);

        mainGrid.Add(rightGrid, 1, 0);

        // === ШАБЛОН ===
        var templateIcon = CreateTintedIcon("icon_box.png", 31);
        var templateTitle = new Label { Text = Loc.T("TemplateList"), FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Colors.White };
        var templateDescription = new Label { Text = "Быстрый выбор товаров", FontSize = 12, TextColor = Color.FromArgb("#AAA3BD") };
        var templateText = new VerticalStackLayout { Spacing = 3, VerticalOptions = LayoutOptions.Center, Children = { templateTitle, templateDescription } };
        var chevron = CreateTintedIcon("icon_chevron_right.png", 20);
        var templateGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 15,
            Padding = new Thickness(15, 16)
        };
        templateGrid.Add(templateIcon, 0, 0);
        templateGrid.Add(templateText, 1, 0);
        templateGrid.Add(chevron, 2, 0);

        var templateCard = new Border
        {
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#29253A"),
            StrokeShape = new RoundRectangle { CornerRadius = 23 },
            BackgroundColor = Color.FromArgb("#151321"),
            Content = templateGrid
        };
        templateCard.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await ShowTemplatesPopup())
        });

        var pageContent = new VerticalStackLayout
        {
            Padding = new Thickness(18, 22, 18, 20),
            Spacing = 14,
            Children = { header, searchFrame, mainGrid, templateCard }
        };

        var scroll = new ScrollView { Content = pageContent, VerticalScrollBarVisibility = ScrollBarVisibility.Never };

        _rootGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };
        _rootGrid.Add(scroll, 0, 0);
        _rootGrid.Add(new BottomNavBar(1, _repo), 0, 1);
        Content = _rootGrid;

        RenderShoppingList();
        _ = LoadLocationAndStoresAsync();
    }

    private Image CreateTintedIcon(string source, double size)
    {
        var image = new Image
        {
            Source = source,
            WidthRequest = size,
            HeightRequest = size,
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Center
        };
        try { image.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor }); }
        catch { }
        return image;
    }

    private Border CreateOutlineButton(string text, Color color)
    {
        return new Border
        {
            Stroke = color.WithAlpha(0.65f),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 13 },
            BackgroundColor = Color.FromArgb("#171522"),
            HeightRequest = 48,
            Padding = new Thickness(3, 0),
            Content = new Label
            {
                Text = text,
                TextColor = color,
                FontSize = 13,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation
            }
        };
    }

    // === ЗАГРУЗКА МЕСТОПОЛОЖЕНИЯ И МАГАЗИНОВ ===
    private async Task LoadLocationAndStoresAsync()
    {
        try
        {
            var userLocation = await _storeService.GetCurrentLocationAsync();

            if (userLocation == null)
            {
                _locText.Text = "GPS отключен";
                _currentLocation = null;
                _nearbyStores = Array.Empty<NearbyStore>();
                MainThread.BeginInvokeOnMainThread(RenderStores);
                return;
            }

            _currentLocation = userLocation;

            var city = await _storeService.GetCityNameAsync(userLocation);
            _locText.Text = string.IsNullOrWhiteSpace(city) ? "Определено по GPS" : city;

            _nearbyStores = await _storeSearch.FindAsync(
                userLocation.Latitude,
                userLocation.Longitude,
                radiusMeters: 3000);

            MainThread.BeginInvokeOnMainThread(RenderStores);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Store] load error: {ex}");
            _locText.Text = "Ошибка загрузки";
            _nearbyStores = Array.Empty<NearbyStore>();
            MainThread.BeginInvokeOnMainThread(RenderStores);
        }
    }

    private async Task ShowCityPicker()
    {
        var cities = new[] { "Москва", "Чита", "Санкт-Петербург", "Казань", "Новосибирск" };
        var selected = await DisplayActionSheet("Выберите город", "Отмена", null, cities);
        if (string.IsNullOrWhiteSpace(selected) || selected == "Отмена") return;

        _locText.Text = selected;
        _currentLocation = selected switch
        {
            "Москва" => new Location(55.7558, 37.6173),
            "Чита" => new Location(52.0340, 113.4994),
            "Санкт-Петербург" => new Location(59.9343, 30.3351),
            "Казань" => new Location(55.7963, 49.1088),
            "Новосибирск" => new Location(55.0084, 82.9357),
            _ => _currentLocation
        };

        if (_currentLocation != null)
        {
            _nearbyStores = await _storeSearch.FindAsync(
                _currentLocation.Latitude,
                _currentLocation.Longitude,
                radiusMeters: 3000);
            RenderStores();
        }
    }

    private async Task OpenSelectedMap()
    {
        try
        {
            if (_currentLocation == null)
            {
                await DisplayAlert("Карта", "Местоположение пока недоступно.", "OK");
                return;
            }

            var lat = _currentLocation.Latitude.ToString(CultureInfo.InvariantCulture);
            var lon = _currentLocation.Longitude.ToString(CultureInfo.InvariantCulture);

            string uri = MapConfig.Preferred switch
            {
                MapProviderType.Yandex => $"https://yandex.ru/maps/?pt={lon},{lat}&z=15&l=map",
                MapProviderType.TwoGis => $"https://2gis.ru/geo/{lon},{lat}",
                _ => $"https://www.google.com/maps/search/?api=1&query={lat},{lon}"
            };

            await Launcher.Default.OpenAsync(uri);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Карта", $"Не удалось открыть карту: {ex.Message}", "OK");
        }
    }

    // === СПИСОК ПОКУПОК ===
    private async Task ShowAddItemPopup()
    {
        try
        {
            var name = await DisplayPromptAsync("Добавить товар", "Введите название продукта:", "Далее", "Отмена", "Например, Молоко", maxLength: 80);
            if (string.IsNullOrWhiteSpace(name)) return;

            var quantity = await DisplayPromptAsync("Количество", "Укажите количество:", "Сохранить", "Отмена", "Например, 2 шт. или 1 кг", initialValue: "1 шт.", maxLength: 30);
            if (quantity == null) return;

            var emojiOptions = new[] { "🥛", "🍞", "🥩", "🍎", "💧", "🧀", "🥚", "🍫", "🛒" };
            var emoji = await DisplayActionSheet("Выберите значок товара", "Отмена", null, emojiOptions);
            if (string.IsNullOrWhiteSpace(emoji) || emoji == "Отмена") emoji = "🛒";

            var price = _priceService?.GetPrice(name) ?? 0;
            _shoppingItems.Add(new ShoppingItem { Name = name.Trim(), Quantity = quantity.Trim(), Emoji = emoji, Price = price });

            SaveShoppingList();
            RenderShoppingList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Не удалось добавить товар", ex.Message, "OK");
        }
    }

    private async Task DeleteCheckedItems()
    {
        var checkedCount = _shoppingItems.Count(item => item.IsChecked);
        if (checkedCount == 0)
        {
            await DisplayAlert("Список покупок", "Сначала отметьте товары, которые хотите удалить.", "OK");
            return;
        }

        var confirm = await DisplayAlert("Удаление", $"Удалить отмеченные товары ({checkedCount})?", "Удалить", "Отмена");
        if (!confirm) return;

        _shoppingItems.RemoveAll(item => item.IsChecked);
        SaveShoppingList();
        RenderShoppingList();
    }

    // === ШАБЛОНЫ ===
    private async Task ShowTemplatesPopup()
    {
        var customTemplates = LoadCustomTemplates();
        var options = new List<string> { "Классический", "Для пикника" };
        options.AddRange(customTemplates.Select(t => t.Name));
        options.Add("Создать свой шаблон");
        options.Add("Отмена");

        var selected = await DisplayActionSheet("Шаблоны покупок", "Отмена", null, options.ToArray());
        if (string.IsNullOrWhiteSpace(selected) || selected == "Отмена") return;

        if (selected == "Создать свой шаблон")
        {
            await CreateCustomTemplate();
            return;
        }

        List<ShoppingItem> templateItems;
        if (selected == "Классический") templateItems = _storeService.GetTemplate("Базовый");
        else if (selected == "Для пикника") templateItems = _storeService.GetTemplate("Для пикника");
        else templateItems = customTemplates.FirstOrDefault(t => t.Name == selected)?.Items ?? new List<ShoppingItem>();

        if (templateItems.Count == 0) return;

        foreach (var item in templateItems)
        {
            _shoppingItems.Add(new ShoppingItem
            {
                Name = item.Name,
                Quantity = item.Quantity,
                Emoji = item.Emoji,
                Price = _priceService?.GetPrice(item.Name) ?? item.Price
            });
        }

        SaveShoppingList();
        RenderShoppingList();
    }

    private async Task CreateCustomTemplate()
    {
        var templateName = await DisplayPromptAsync("Новый шаблон", "Название шаблона:", "Продолжить", "Отмена", "Например, Еженедельные покупки", maxLength: 50);
        if (string.IsNullOrWhiteSpace(templateName)) return;

        var templateItems = new List<ShoppingItem>();
        while (true)
        {
            var name = await DisplayPromptAsync("Товары шаблона", "Введите название товара. Оставьте поле пустым, чтобы закончить:", "Добавить", "Готово", "Название товара", maxLength: 80);
            if (string.IsNullOrWhiteSpace(name)) break;

            var quantity = await DisplayPromptAsync("Количество", $"Количество для «{name.Trim()}»:", "Добавить", "Завершить", "Например, 1 шт.", initialValue: "1 шт.", maxLength: 30);
            if (quantity == null) break;

            templateItems.Add(new ShoppingItem
            {
                Name = name.Trim(),
                Quantity = quantity.Trim(),
                Emoji = GuessEmoji(name),
                Price = _priceService?.GetPrice(name) ?? 0
            });

            var more = await DisplayAlert("Шаблон", "Добавить ещё один товар?", "Добавить", "Сохранить");
            if (!more) break;
        }

        if (templateItems.Count == 0)
        {
            await DisplayAlert("Шаблон", "Шаблон не сохранён: в нём нет товаров.", "OK");
            return;
        }

        var templates = LoadCustomTemplates();
        templates.RemoveAll(t => string.Equals(t.Name, templateName.Trim(), StringComparison.OrdinalIgnoreCase));
        templates.Add(new SavedTemplate { Name = templateName.Trim(), Items = templateItems });
        Preferences.Default.Set(CustomTemplatesKey, JsonSerializer.Serialize(templates));

        var addNow = await DisplayAlert("Шаблон сохранён", "Добавить его товары в текущий список?", "Добавить", "Позже");
        if (addNow)
        {
            _shoppingItems.AddRange(templateItems.Select(i => new ShoppingItem { Name = i.Name, Quantity = i.Quantity, Emoji = i.Emoji, Price = i.Price }));
            SaveShoppingList();
            RenderShoppingList();
        }
    }

    private List<SavedTemplate> LoadCustomTemplates()
    {
        try
        {
            var json = Preferences.Default.Get(CustomTemplatesKey, "");
            return string.IsNullOrWhiteSpace(json)
                ? new List<SavedTemplate>()
                : JsonSerializer.Deserialize<List<SavedTemplate>>(json) ?? new List<SavedTemplate>();
        }
        catch { return new List<SavedTemplate>(); }
    }

    private static string GuessEmoji(string name)
    {
        var n = name.ToLowerInvariant();
        if (n.Contains("мол") || n.Contains("сыр") || n.Contains("творог")) return "🥛";
        if (n.Contains("хлеб") || n.Contains("бул")) return "🍞";
        if (n.Contains("мяс") || n.Contains("кур")) return "🥩";
        if (n.Contains("яблок") || n.Contains("фрукт") || n.Contains("овощ")) return "🍎";
        if (n.Contains("вод")) return "💧";
        if (n.Contains("яйц")) return "🥚";
        return "🛒";
    }

    // === РЕНДЕР ===
    private void RenderShoppingList()
    {
        if (_shopItemsContainer == null) return;
        _shopItemsContainer.Children.Clear();

        var filtered = _shoppingItems
            .Where(item => string.IsNullOrWhiteSpace(_searchText) || item.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (filtered.Count == 0)
        {
            _shopItemsContainer.Children.Add(new Label
            {
                Text = _shoppingItems.Count == 0 ? "Список пока пуст" : "Ничего не найдено",
                TextColor = Color.FromArgb("#938DA5"),
                FontSize = 12,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 20)
            });
            return;
        }

        foreach (var item in filtered)
        {
            var check = new CheckBox { Color = Color.FromArgb("#A978F5"), IsChecked = item.IsChecked, VerticalOptions = LayoutOptions.Center, Scale = 0.9 };
            check.CheckedChanged += (_, e) => { item.IsChecked = e.Value; SaveShoppingList(); RenderShoppingList(); };

            var icon = new Label { Text = item.Emoji, FontSize = 23, VerticalOptions = LayoutOptions.Center };
            var title = new Label
            {
                Text = item.Name,
                FontSize = 13,
                TextColor = item.IsChecked ? Color.FromArgb("#858091") : Colors.White,
                TextDecorations = item.IsChecked ? TextDecorations.Strikethrough : TextDecorations.None,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation
            };
            var quantity = new Label { Text = item.Quantity, FontSize = 11, TextColor = Color.FromArgb("#B7B0C8"), VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.End };

            var row = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 5,
                Padding = new Thickness(0, 5)
            };
            row.Add(check, 0, 0);
            row.Add(icon, 1, 0);
            row.Add(title, 2, 0);
            row.Add(quantity, 3, 0);
            _shopItemsContainer.Children.Add(row);
        }
    }

    private void RenderStores()
    {
        if (_storesListLayout == null) return;
        _storesListLayout.Children.Clear();

        // ✅ Обновляем карту
        if (_miniMap != null && _currentLocation != null)
        {
            var markers = new List<MapMarker>
            {
                new(_currentLocation.Latitude, _currentLocation.Longitude, "Вы здесь", null, true)
            };
            foreach (var s in _nearbyStores)
                markers.Add(new MapMarker(s.Lat, s.Lng, s.Name, s.Address));

            _miniMap.Show(_currentLocation.Latitude, _currentLocation.Longitude, 15, markers);
        }

        // Заголовок
        var header = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(0, 0, 0, 5) };
        header.Children.Add(CreateTintedIcon("icon_store.png", 21));
        header.Children.Add(new Label
        {
            Text = Loc.T("AvailableStores"),
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center
        });
        _storesListLayout.Children.Add(header);

        // Пустые состояния
        if (_currentLocation == null)
        {
            _storesListLayout.Children.Add(new Label
            {
                Text = "Включите GPS, чтобы найти магазины рядом",
                FontSize = 11,
                TextColor = Color.FromArgb("#938DA5")
            });
            return;
        }

        if (_nearbyStores.Count == 0)
        {
            _storesListLayout.Children.Add(new Label
            {
                Text = "Магазины не найдены в радиусе 3 км",
                FontSize = 11,
                TextColor = Color.FromArgb("#938DA5")
            });
            return;
        }

        var filtered = _nearbyStores
            .Where(s => string.IsNullOrWhiteSpace(_searchText)
                || s.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
                || (s.Address ?? "").Contains(_searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var store in filtered)
        {
            var logoText = string.IsNullOrEmpty(store.Name) ? "?" : store.Name[0].ToString().ToUpper();

            var brand = new Border
            {
                WidthRequest = 31,
                HeightRequest = 31,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 9 },
                BackgroundColor = Colors.White,
                Content = new Label
                {
                    Text = logoText,
                    TextColor = Color.FromArgb("#D83A43"),
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 14,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                }
            };

            var name = new Label
            {
                Text = store.Name,
                FontSize = 12,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation
            };

            var distanceText = store.DistanceMeters is { } d
                ? (d < 1000 ? $"{d:F0} м  ›" : $"{d / 1000:F1} км  ›")
                : "›";

            var distance = new Label
            {
                Text = distanceText,
                FontSize = 11,
                TextColor = Color.FromArgb("#B8A6D6"),
                VerticalOptions = LayoutOptions.Center
            };

            var row = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 8,
                Padding = new Thickness(0, 7)
            };
            row.Add(brand, 0, 0);
            row.Add(name, 1, 0);
            row.Add(distance, 2, 0);

            var captured = store;
            row.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await OpenStoreMap(captured))
            });
            _storesListLayout.Children.Add(row);
        }
    }

    private async Task OpenStoreMap(NearbyStore store)
    {
        try
        {
            var lat = store.Lat.ToString(CultureInfo.InvariantCulture);
            var lon = store.Lng.ToString(CultureInfo.InvariantCulture);

            string uri = MapConfig.Preferred switch
            {
                MapProviderType.Yandex => $"https://yandex.ru/maps/?pt={lon},{lat}&z=18&l=map",
                MapProviderType.TwoGis => $"https://2gis.ru/geo/{lon},{lat}",
                _ => $"https://www.google.com/maps/search/?api=1&query={lat},{lon}"
            };
            await Launcher.Default.OpenAsync(uri);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Карта", $"Не удалось открыть магазин на карте: {ex.Message}", "OK");
        }
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (Content == null) return;
        Content.GestureRecognizers.Clear();

        var swipeL = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        swipeL.Swiped += (_, _) => BottomNavBar.SwitchToTab(2, _repo);
        Content.GestureRecognizers.Add(swipeL);

        var swipeR = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeR.Swiped += (_, _) => BottomNavBar.SwitchToTab(0, _repo);
        Content.GestureRecognizers.Add(swipeR);
    }
}
