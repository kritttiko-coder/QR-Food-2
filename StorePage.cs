using FoodExpiryApp.Models;
using FoodExpiryApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Behaviors;

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

    public StorePage(ProductRepository repo)
    {
        _repo = repo;
        _storeService = IPlatformApplication.Current!.Services.GetService<StoreService>()!;
        Title = Loc.T("StoreTitle");
        ThemeManager.BindPage(this);
        BuildUI();
    }

    private async void BuildUI()
    {
        // --- ШАПКА ---
        var headerLayout = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10 };
        var title = new Label { Text = Loc.T("StoreTitle"), FontSize = 28, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(title);

        var locStack = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
        var locImg = new Image { Source = "icon_pin.png", WidthRequest = 20, HeightRequest = 20 };
        locImg.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        _locText = new Label { Text = "Поиск GPS...", TextColor = ThemeManager.AccentColor, VerticalOptions = LayoutOptions.Center };
        locStack.Children.Add(locImg); locStack.Children.Add(_locText);

        headerLayout.Add(title, 0, 0); headerLayout.Add(locStack, 1, 0);

        // --- ПОИСК ---
        var search = new SearchBar { Placeholder = Loc.T("SearchStores"), HeightRequest = 50 };
        ThemeManager.BindSearch(search);
        var searchFrame = new Border { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = new Thickness(4, 0), Content = search };
        searchFrame.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);

        // --- ОСНОВНАЯ СЕТКА (Сплит) ---
        var mainGrid = new Grid { ColumnDefinitions = { new ColumnDefinition(0.45, GridUnitType.Star), new ColumnDefinition(0.55, GridUnitType.Star) }, ColumnSpacing = 10, HeightRequest = 400 };

        // 1. ЛЕВАЯ ПАНЕЛЬ (Список покупок)
        var shopList = new VerticalStackLayout { Spacing = 10 };
        var shopHeader = new HorizontalStackLayout { Spacing = 8 };
        var shopIcon = new Image { Source = "icon_basket.png", WidthRequest = 20, HeightRequest = 20 };
        shopIcon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var shopTitle = new Label { Text = Loc.T("ShoppingList"), FontSize = 16, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(shopTitle);
        shopHeader.Children.Add(shopIcon); shopHeader.Children.Add(shopTitle);
        shopList.Children.Add(shopHeader);

        _shopItemsContainer = new VerticalStackLayout { Spacing = 8 };
        shopList.Children.Add(_shopItemsContainer);

        var btnStack = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8 };
        
        var btnAdd = new Button { Text = "+", BackgroundColor = Colors.Transparent, BorderColor = ThemeManager.AccentColor, BorderWidth = 1, CornerRadius = 12, TextColor = ThemeManager.AccentColor, FontSize = 20 };
        
btnAdd.Clicked += async (_, _) =>
{
    string name = await DisplayPromptAsync("Новый товар", "Введите название:", "ОК", "Отмена");
    if (!string.IsNullOrWhiteSpace(name))
    {
        var priceSrv = IPlatformApplication.Current!.Services.GetService<PriceService>()!;
        _shoppingItems.Add(new ShoppingItem { 
            Name = name, 
            Quantity = "1 шт.", 
            Emoji = "🛒",
            Price = priceSrv.GetPrice(name) // <-- Сразу считаем цену!
        });
        RenderShoppingList();
    }
};
        var btnDel = new Button { Text = Loc.T("Delete"), BackgroundColor = Colors.Transparent, BorderColor = Color.FromArgb("#FF414D"), BorderWidth = 1, CornerRadius = 12, TextColor = Color.FromArgb("#FF414D") };
        btnDel.Clicked += (_, _) =>
        {
            var toRemove = _shoppingItems.Where(i => i.IsChecked).ToList();
            foreach (var item in toRemove) _shoppingItems.Remove(item);
            RenderShoppingList();
        };
        btnStack.Add(btnAdd, 0, 0); btnStack.Add(btnDel, 1, 0);
        shopList.Children.Add(btnStack);

        var leftPanel = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 20 }, Padding = 12, Content = shopList };
        leftPanel.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        leftPanel.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);

        // 2. ПРАВАЯ ПАНЕЛЬ (Заглушка карты + Магазины)
        var mapPlaceholder = new Border { BackgroundColor = Color.FromArgb("#111116"), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 20 } };
        var mapStack = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, Spacing = 10 };
        var mapIcon = new Image { Source = "icon_map.png", WidthRequest = 40, HeightRequest = 40 };
        mapIcon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var mapText = new Label { Text = "Карта скоро появится", TextColor = Colors.Gray, FontSize = 14 };
        mapStack.Children.Add(mapIcon); mapStack.Children.Add(mapText);
        mapPlaceholder.Content = mapStack;

        _storesListLayout = new VerticalStackLayout { Spacing = 4, Padding = 8, VerticalOptions = LayoutOptions.End, Margin = new Thickness(10) };
        var storesBorder = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = 4, Content = _storesListLayout, BackgroundColor = Color.FromRgba(0, 0, 0, 0.85) };
        storesBorder.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);

        var mapGrid = new Grid();
        mapGrid.Add(mapPlaceholder);
        mapGrid.Add(storesBorder);

        mainGrid.Add(leftPanel, 0, 0);
        mainGrid.Add(mapGrid, 1, 0);

        // --- КНОПКА ШАБЛОНОВ ---
        var templateBtn = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = 12, Margin = new Thickness(0, 10, 0, 0) };
        templateBtn.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        templateBtn.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);
        var templateGrid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        var templateIcon = new Image { Source = "icon_box.png", WidthRequest = 24, HeightRequest = 24 };
        templateIcon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var templateStack = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        var tTitle = new Label { Text = Loc.T("TemplateList"), FontSize = 14, FontAttributes = FontAttributes.Bold };
        var tDesc = new Label { Text = Loc.T("TemplateDesc"), FontSize = 11 };
        ThemeManager.BindText(tTitle); ThemeManager.BindText(tDesc, true);
        templateStack.Children.Add(tTitle); templateStack.Children.Add(tDesc);
        var chevron = new Label { Text = ">", FontSize = 18, TextColor = Colors.Gray, VerticalOptions = LayoutOptions.Center };
        templateGrid.Add(templateIcon, 0, 0); templateGrid.Add(templateStack, 1, 0); templateGrid.Add(chevron, 2, 0);
        templateBtn.Content = templateGrid;

        var templateTap = new TapGestureRecognizer();
        templateTap.Tapped += async (_, _) =>
        {
            var action = await DisplayActionSheet("Выберите шаблон", "Отмена", null, "Базовый", "Для пикника");
            if (action != "Отмена" && action != null)
            {
                var items = _storeService.GetTemplate(action);
                foreach (var item in items) _shoppingItems.Add(item);
                RenderShoppingList();
            }
        };
        templateBtn.GestureRecognizers.Add(templateTap);

        // --- СБОРКА СТРАНИЦЫ ---
        var content = new VerticalStackLayout { Padding = 16, Spacing = 12, Children = { headerLayout, searchFrame, mainGrid, templateBtn } };
        var scroll = new ScrollView { Content = content };

        var rootGrid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
        rootGrid.Add(scroll, 0, 0);
        rootGrid.Add(new BottomNavBar(1, _repo), 0, 1);

        var swipeL = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        swipeL.Swiped += (_, _) => BottomNavBar.SwitchToTab(3, _repo);
        rootGrid.GestureRecognizers.Add(swipeL);
        var swipeR = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeR.Swiped += (_, _) => BottomNavBar.SwitchToTab(0, _repo);
        rootGrid.GestureRecognizers.Add(swipeR);

        Content = rootGrid;
        ThemeManager.Apply();

        // --- ИНИЦИАЛИЗАЦИЯ ДАННЫХ ---
        var userLoc = await _storeService.GetCurrentLocationAsync();
        if (userLoc != null)
        {
            string cityName = await _storeService.GetCityNameAsync(userLoc);
            _locText.Text = cityName;
            _nearbyStores = _storeService.GetNearbyStores(userLoc);
            RenderStores();
        }
        else
        {
            _locText.Text = "Нет GPS";
            RenderStoresError();
        }

        _shoppingItems.Add(new ShoppingItem { Name = "Молоко", Quantity = "2 шт.", Emoji = "🥛" });
        _shoppingItems.Add(new ShoppingItem { Name = "Хлеб", Quantity = "1 шт.", Emoji = "🍞" });
        RenderShoppingList();
    }

    private void RenderShoppingList()
    {
        _shopItemsContainer.Children.Clear();
        foreach (var item in _shoppingItems)
        {
            var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8, Padding = new Thickness(0, 4) };
            
            var check = new CheckBox { Color = ThemeManager.AccentColor, IsChecked = item.IsChecked };
            check.CheckedChanged += (s, e) => 
            { 
                item.IsChecked = e.Value; 
                RenderShoppingList(); // Обновляем, чтобы зачеркнуть текст
            };
            
            var icon = new Label { Text = item.Emoji, FontSize = 20, VerticalOptions = LayoutOptions.Center };
            var title = new Label { Text = item.Name, FontSize = 14, VerticalOptions = LayoutOptions.Center };
            ThemeManager.BindText(title);
            
            if (item.IsChecked)
            {
                title.TextDecorations = TextDecorations.Strikethrough;
                title.TextColor = Colors.Gray;
            }

            var q = new Label { Text = item.Quantity, FontSize = 12, TextColor = Colors.Gray, VerticalOptions = LayoutOptions.Center };
            
            grid.Add(check, 0, 0); grid.Add(icon, 1, 0); grid.Add(title, 2, 0); grid.Add(q, 3, 0);
            _shopItemsContainer.Children.Add(grid);
        }
    }

    private void RenderStores()
    {
        _storesListLayout.Children.Clear();
        var storesHeader = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(0, 0, 0, 4) };
        var storesIcon = new Image { Source = "icon_store.png", WidthRequest = 20, HeightRequest = 20 };
        storesIcon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var storesTitle = new Label { Text = Loc.T("AvailableStores"), FontSize = 14, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(storesTitle);
        storesHeader.Children.Add(storesIcon); storesHeader.Children.Add(storesTitle);
        _storesListLayout.Children.Add(storesHeader);

        foreach (var store in _nearbyStores)
        {
            var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8, Padding = new Thickness(0, 8) };
            var brand = new Border { WidthRequest = 30, HeightRequest = 30, StrokeShape = new RoundRectangle { CornerRadius = 8 }, BackgroundColor = Colors.White, Content = new Label { Text = store.Logo, TextColor = Colors.Red, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center } };
            var title = new Label { Text = store.Name, FontSize = 13, VerticalOptions = LayoutOptions.Center };
            ThemeManager.BindText(title);
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

    private void RenderStoresError()
    {
        _storesListLayout.Children.Clear();
        var errorStack = new VerticalStackLayout { Spacing = 8, Padding = 10, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
        var errorText = new Label { Text = "Не удалось определить местоположение.\nВключите GPS.", FontSize = 12, TextColor = Color.FromArgb("#FF414D"), HorizontalTextAlignment = TextAlignment.Center };
        var retryBtn = new Button { Text = "Повторить", HeightRequest = 36, FontSize = 12, CornerRadius = 10, BackgroundColor = ThemeManager.AccentColor, TextColor = Colors.White };
        retryBtn.Clicked += (_, _) => Application.Current!.MainPage = new NavigationPage(new StorePage(_repo));
        errorStack.Children.Add(errorText); errorStack.Children.Add(retryBtn);
        _storesListLayout.Children.Add(errorStack);
    }
}
