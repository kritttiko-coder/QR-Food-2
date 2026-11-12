using FoodExpiryApp.Models;
using FoodExpiryApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Behaviors;
using System.Text.Json;

namespace FoodExpiryApp;

public class StorePage : ContentPage
{
    private readonly ProductRepository _repo;
    private readonly StoreService _storeService;
    private VerticalStackLayout _shopItemsContainer = null!;
    private VerticalStackLayout _storesListLayout = null!;
    private List<ShoppingItem> _shoppingItems = new();
    private List<StoreModel> _nearbyStores = new();
    private Label _locText = null!;
    private Grid _rootGrid = null!;
    private Location? _currentLocation;

    public StorePage(ProductRepository repo)
    {
        _repo = repo;
        _storeService = IPlatformApplication.Current!.Services.GetService<StoreService>()!;
        Title = Loc.T("StoreTitle");
        ThemeManager.BindPage(this);
        LoadShoppingList();
        BuildUI();
    }

    private void LoadShoppingList()
    {
        var json = Preferences.Default.Get("shopping_list", "");
        if (!string.IsNullOrEmpty(json)) _shoppingItems = JsonSerializer.Deserialize<List<ShoppingItem>>(json) ?? new();
        else _shoppingItems = new List<ShoppingItem> { new() { Name = "Молоко", Quantity = "2 шт.", Emoji = "🥛" } };
    }

    private void SaveShoppingList() => Preferences.Default.Set("shopping_list", JsonSerializer.Serialize(_shoppingItems));

    private async void BuildUI()
    {
        // --- ШАПКА ---
        var headerLayout = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8 };
        var title = new Label { Text = Loc.T("StoreTitle"), FontSize = 28, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };
        ThemeManager.BindText(title);
        headerLayout.Add(title, 0, 0);

        var locStack = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
        var locImg = new Image { Source = "icon_pin.png", WidthRequest = 18, HeightRequest = 18 };
        locImg.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        _locText = new Label { Text = "Поиск...", TextColor = ThemeManager.AccentColor, VerticalOptions = LayoutOptions.Center, FontSize = 14 };
        var locChevron = new Image { Source = "icon_chevron_down.png", WidthRequest = 14, HeightRequest = 14 };
        locChevron.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        locStack.Children.Add(locImg); locStack.Children.Add(_locText); locStack.Children.Add(locChevron);
        locStack.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await ShowCityPicker()) });
        headerLayout.Add(locStack, 1, 0);

        var mapBtn = new Image { Source = "icon_map.png", WidthRequest = 26, HeightRequest = 26, VerticalOptions = LayoutOptions.Center };
        mapBtn.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        mapBtn.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                if (_currentLocation != null) await Map.Default.OpenAsync(_currentLocation, new MapLaunchOptions { Name = _locText.Text });
                else await DisplayAlert("Ошибка", "Нет GPS", "OK");
            })
        });
        headerLayout.Add(mapBtn, 2, 0);

        // --- ПОИСК ---
        var search = new SearchBar { Placeholder = Loc.T("SearchStores"), HeightRequest = 50 };
        ThemeManager.BindSearch(search);
        var searchFrame = new Border { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = new Thickness(4, 0), Content = search };
        searchFrame.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);

        // --- ОСНОВНАЯ СЕТКА (Слева список, Справа карта) ---
        var mainGrid = new Grid 
        { 
            ColumnDefinitions = 
            { 
                new ColumnDefinition { Width = new GridLength(0.45, GridUnitType.Star) }, 
                new ColumnDefinition { Width = new GridLength(0.55, GridUnitType.Star) } 
            }, 
            ColumnSpacing = 10, 
            HeightRequest = 300 
        };

        // 1. Левая панель (Список)
        var shopList = new VerticalStackLayout { Spacing = 8 };
        var shopHeader = new HorizontalStackLayout { Spacing = 8 };
        var shopIcon = new Image { Source = "icon_basket.png", WidthRequest = 20, HeightRequest = 20 };
        shopIcon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var shopTitle = new Label { Text = Loc.T("ShoppingList"), FontSize = 16, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(shopTitle);
        shopHeader.Children.Add(shopIcon); shopHeader.Children.Add(shopTitle);
        shopList.Children.Add(shopHeader);

        _shopItemsContainer = new VerticalStackLayout { Spacing = 8 };
        var shopScroll = new ScrollView { Content = _shopItemsContainer };
        shopList.Children.Add(shopScroll);

        // ✅ Кнопки СТРОГО как в концепте (кастомный Border вместо Button)
        var btnStack = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8, Margin = new Thickness(0, 8, 0, 0) };
        
        var btnAdd = new Border { StrokeShape = new RoundRectangle { CornerRadius = 12 }, StrokeThickness = 1, Stroke = ThemeManager.AccentColor, HeightRequest = 44, Content = new Label { Text = "+ " + Loc.T("Add"), TextColor = ThemeManager.AccentColor, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, FontSize = 13 } };
        btnAdd.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await ShowAddItemPopup()) });

        var btnDel = new Border { StrokeShape = new RoundRectangle { CornerRadius = 12 }, StrokeThickness = 1, Stroke = Color.FromArgb("#FF414D"), HeightRequest = 44, Content = new Label { Text = "🗑 " + Loc.T("Delete"), TextColor = Color.FromArgb("#FF414D"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, FontSize = 13 } };
        btnDel.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { _shoppingItems.RemoveAll(i => i.IsChecked); SaveShoppingList(); RenderShoppingList(); }) });

        btnStack.Add(btnAdd, 0, 0); btnStack.Add(btnDel, 1, 0);
        shopList.Children.Add(btnStack);

        var leftPanel = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 20 }, Padding = 10, Content = shopList };
        leftPanel.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        leftPanel.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);

        // 2. Правая панель (Карта-заглушка + Магазины)
        var mapPlaceholder = new Border { BackgroundColor = Color.FromArgb("#1A2C4A"), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 20 } };
        
        _storesListLayout = new VerticalStackLayout { Spacing = 4, Padding = 12, VerticalOptions = LayoutOptions.End };
        var storesOverlay = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Content = _storesListLayout, BackgroundColor = Color.FromRgba(0,0,0,0.85), Margin = new Thickness(10) };
        storesOverlay.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);

        var mapGrid = new Grid();
        mapGrid.Add(mapPlaceholder);
        mapGrid.Add(storesOverlay);

        mainGrid.Add(leftPanel, 0, 0);
        mainGrid.Add(mapGrid, 1, 0);

        // 3. Шаблон (в самом низу)
        var templateBtn = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = 12, Margin = new Thickness(0, 10, 0, 0) };
        templateBtn.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        templateBtn.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);
        var templateGrid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        
        var templateIcon = new Image { Source = "icon_box.png", WidthRequest = 24, HeightRequest = 24 };
        templateIcon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        templateGrid.Add(templateIcon, 0, 0);
        
        var tStack = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        var tTitle = new Label { Text = Loc.T("TemplateList"), FontSize = 14, FontAttributes = FontAttributes.Bold };
        var tDesc = new Label { Text = Loc.T("TemplateDesc"), FontSize = 11 };
        ThemeManager.BindText(tTitle); ThemeManager.BindText(tDesc, true);
        tStack.Children.Add(tTitle); tStack.Children.Add(tDesc);
        templateGrid.Add(tStack, 1, 0);
        
        var chevron = new Image { Source = "icon_chevron_right.png", WidthRequest = 16, HeightRequest = 16, VerticalOptions = LayoutOptions.Center };
        chevron.Behaviors.Add(new IconTintColorBehavior { TintColor = Colors.Gray });
        templateGrid.Add(chevron, 2, 0);
        
        templateBtn.Content = templateGrid;
        templateBtn.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await ShowTemplatesPopup()) });

        var content = new VerticalStackLayout { Padding = 16, Spacing = 12, Children = { headerLayout, searchFrame, mainGrid, templateBtn } };
        var scroll = new ScrollView { Content = content };

        _rootGrid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
        _rootGrid.Add(scroll, 0, 0);
        _rootGrid.Add(new BottomNavBar(1, _repo), 0, 1);

        Content = _rootGrid;
        ThemeManager.Apply();

        // Загрузка данных
        var userLoc = await _storeService.GetCurrentLocationAsync();
        if (userLoc != null) { _currentLocation = userLoc; _locText.Text = await _storeService.GetCityNameAsync(userLoc); _nearbyStores = _storeService.GetNearbyStores(userLoc); }
        else { _locText.Text = "GPS отключен"; _nearbyStores = _storeService.GetNearbyStores(new Location(55.7558, 37.6173)); }
        RenderStores();
        RenderShoppingList();
    }

    private async Task ShowCityPicker()
    {
        var cities = new[] { "Москва", "Чита", "Санкт-Петербург", "Казань", "Новосибирск" };
        var action = await DisplayActionSheet("Выберите город", "Отмена", null, cities);
        if (action != "Отмена" && action != null)
        {
            _locText.Text = action;
            _currentLocation = action switch { "Москва" => new Location(55.7558, 37.6173), "Чита" => new Location(52.0340, 113.4994), "Санкт-Петербург" => new Location(59.9343, 30.3351), "Казань" => new Location(55.7963, 49.1088), "Новосибирск" => new Location(55.0084, 82.9357), _ => _currentLocation };
            if (_currentLocation != null) { _nearbyStores = _storeService.GetNearbyStores(_currentLocation); RenderStores(); }
        }
    }

    // ✅ БЕЗОПАСНЫЙ ПОП-АП (без вложенности, без кастомных ресурсов)
    private async Task ShowAddItemPopup()
    {
        var overlay = new Grid { BackgroundColor = Color.FromRgba(0,0,0,0.7), ZIndex = 100 };
        
        var nameEntry = new Entry { Placeholder = "Название", HeightRequest = 50 };
        var qtyEntry = new Entry { Placeholder = "Кол-во", Text = "1 шт.", HeightRequest = 50 };
        ThemeManager.BindEntry(nameEntry); ThemeManager.BindEntry(qtyEntry);

        var emojis = new[] { "🥛", "🍞", "🥩", "🍎", "🥤", "🧀", "🥚", "🧻", "🍫" };
        string selectedEmoji = "🛒";
        var emojiGrid = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
        foreach (var em in emojis) 
        { 
            var lbl = new Label { Text = em, FontSize = 24, Padding = 4 }; 
            lbl.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => selectedEmoji = em) }); 
            emojiGrid.Children.Add(lbl); 
        }

        var saveBtn = new Border { BackgroundColor = ThemeManager.AccentColor, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Padding = new Thickness(0, 10), Content = new Label { Text = "Сохранить", TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center } };
        var cancelBtn = new Border { BackgroundColor = Colors.Transparent, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Padding = new Thickness(0, 10), Content = new Label { Text = "Отмена", TextColor = Colors.Gray, HorizontalOptions = LayoutOptions.Center } };

        var popup = new Border 
        { 
            StrokeThickness = 2, Stroke = ThemeManager.AccentColor, 
            StrokeShape = new RoundRectangle { CornerRadius = 20 }, 
            BackgroundColor = Color.FromArgb("#201D28"), // ✅ ЖЕСТКИЙ ЦВЕТ, ЧТОБЫ НЕ БЫЛО ВЫЛЕТА
            Padding = 20, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
            WidthRequest = 320
        };

        var btnGrid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8, Children = { cancelBtn, saveBtn } };
        btnGrid.Add(cancelBtn, 0, 0); btnGrid.Add(saveBtn, 1, 0);

        popup.Content = new VerticalStackLayout
        {
            Spacing = 12,
            Children = 
            { 
                new Label { Text = "Новый товар", FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, TextColor = Colors.White }, 
                nameEntry, 
                qtyEntry, 
                new Label { Text = "Эмодзи:", FontSize = 12, TextColor = Colors.Gray }, 
                emojiGrid, 
                btnGrid 
            }
        };

        overlay.Add(popup);
        _rootGrid.Add(overlay);

        var tcs = new TaskCompletionSource<bool>();
        saveBtn.GestureRecognizers.Add(new TapGestureRecognizer 
        { 
            Command = new Command(() => 
            { 
                if (!string.IsNullOrWhiteSpace(nameEntry.Text)) 
                { 
                    var priceSrv = IPlatformApplication.Current!.Services.GetService<PriceService>()!; 
                    _shoppingItems.Add(new ShoppingItem { Name = nameEntry.Text, Quantity = qtyEntry.Text, Emoji = selectedEmoji, Price = priceSrv.GetPrice(nameEntry.Text) }); 
                    SaveShoppingList(); RenderShoppingList(); 
                } 
                tcs.SetResult(true); 
            }) 
        });
        
        cancelBtn.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => tcs.SetResult(false)) });

        await tcs.Task;
        _rootGrid.Remove(overlay);
    }

    // ✅ БЕЗОПАСНЫЙ ПОП-АП ШАБЛОНОВ
    private async Task ShowTemplatesPopup()
    {
        var overlay = new Grid { BackgroundColor = Color.FromRgba(0,0,0,0.7), ZIndex = 100 };
        var popup = new Border 
        { 
            StrokeThickness = 2, Stroke = ThemeManager.AccentColor, 
            StrokeShape = new RoundRectangle { CornerRadius = 20 }, 
            BackgroundColor = Color.FromArgb("#201D28"), // ✅ ЖЕСТКИЙ ЦВЕТ
            Padding = 20, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
            WidthRequest = 280
        };

        var btnClassic = new Border { BackgroundColor = ThemeManager.AccentColor, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Padding = new Thickness(0, 10), Content = new Label { Text = "Классический", TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center } };
        var btnPicnic = new Border { BackgroundColor = ThemeManager.AccentColor, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Padding = new Thickness(0, 10), Content = new Label { Text = "Пикник", TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center } };
        var btnCustom = new Border { BackgroundColor = Colors.Transparent, Stroke = ThemeManager.AccentColor, StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Padding = new Thickness(0, 10), Content = new Label { Text = "Создать свой", TextColor = ThemeManager.AccentColor, HorizontalOptions = LayoutOptions.Center } };
        var btnCancel = new Border { BackgroundColor = Colors.Transparent, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Padding = new Thickness(0, 10), Content = new Label { Text = "Назад", TextColor = Colors.Gray, HorizontalOptions = LayoutOptions.Center } };

        var stack = new VerticalStackLayout { Spacing = 12 };
        stack.Children.Add(new Label { Text = "Шаблоны", FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, TextColor = Colors.White });
        stack.Children.Add(btnClassic); stack.Children.Add(btnPicnic); stack.Children.Add(btnCustom); stack.Children.Add(btnCancel);
        
        popup.Content = stack; overlay.Add(popup); _rootGrid.Add(overlay);
        var tcs = new TaskCompletionSource<bool>();
        
        btnClassic.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { _shoppingItems.AddRange(_storeService.GetTemplate("Базовый")); SaveShoppingList(); RenderShoppingList(); tcs.SetResult(true); }) });
        btnPicnic.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { _shoppingItems.AddRange(_storeService.GetTemplate("Для пикника")); SaveShoppingList(); RenderShoppingList(); tcs.SetResult(true); }) });
        
        // ✅ ИСПРАВЛЕННЫЙ ВЫЛЕТ: Сначала убираем этот поп-ап, потом открываем новый
        btnCustom.GestureRecognizers.Add(new TapGestureRecognizer 
        { 
            Command = new Command(async () => 
            { 
                tcs.SetResult(true); 
                await Task.Delay(200); // Даем UI время закрыть текущее окно
                await ShowAddItemPopup(); 
            }) 
        });
        btnCancel.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => tcs.SetResult(false)) });

        await tcs.Task; 
        _rootGrid.Remove(overlay);
    }

    private void RenderShoppingList()
    {
        _shopItemsContainer.Children.Clear();
        foreach (var item in _shoppingItems)
        {
            var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8, Padding = new Thickness(0, 4) };
            var check = new CheckBox { Color = ThemeManager.AccentColor, IsChecked = item.IsChecked };
            check.CheckedChanged += (s, e) => { item.IsChecked = e.Value; SaveShoppingList(); RenderShoppingList(); };
            var icon = new Label { Text = item.Emoji, FontSize = 20, VerticalOptions = LayoutOptions.Center };
            var title = new Label { Text = item.Name, FontSize = 14, VerticalOptions = LayoutOptions.Center };
            ThemeManager.BindText(title);
            if (item.IsChecked) { title.TextDecorations = TextDecorations.Strikethrough; title.TextColor = Colors.Gray; }
            var q = new Label { Text = item.Quantity, FontSize = 12, TextColor = Colors.Gray, VerticalOptions = LayoutOptions.Center };
            grid.Add(check, 0, 0); grid.Add(icon, 1, 0); grid.Add(title, 2, 0); grid.Add(q, 3, 0);
            _shopItemsContainer.Children.Add(grid);
        }
    }

    private void RenderStores()
    {
        _storesListLayout.Children.Clear();
        
        var hHeader = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(0, 0, 0, 4) };
        var img = new Image { Source = "icon_store.png", WidthRequest = 20, HeightRequest = 20 };
        img.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var lbl = new Label { Text = Loc.T("AvailableStores"), FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Colors.White };
        hHeader.Children.Add(img); hHeader.Children.Add(lbl);
        _storesListLayout.Children.Add(hHeader);

        foreach (var store in _nearbyStores)
        {
            var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8, Padding = new Thickness(0, 8) };
            var brand = new Border { WidthRequest = 30, HeightRequest = 30, StrokeShape = new RoundRectangle { CornerRadius = 8 }, BackgroundColor = Colors.White, Content = new Label { Text = store.Logo, TextColor = Colors.Red, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center } };
            var title = new Label { Text = store.Name, FontSize = 13, VerticalOptions = LayoutOptions.Center, TextColor = Colors.White };
            var d = new Label { Text = $"{store.DistanceKm:F1} км", FontSize = 12, TextColor = Colors.Gray, VerticalOptions = LayoutOptions.Center };
            grid.Add(brand, 0, 0); grid.Add(title, 1, 0); grid.Add(d, 2, 0);

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) =>
            {
                var location = new Location(store.Location.Latitude, store.Location.Longitude);
                var options = new MapLaunchOptions { Name = store.Name };
                await Map.Default.OpenAsync(location, options);
            };
            grid.GestureRecognizers.Add(tap);
            _storesListLayout.Children.Add(grid);
        }
    }
}
