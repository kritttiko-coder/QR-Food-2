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
        _locText = new Label { Text = "Чита", TextColor = ThemeManager.AccentColor, VerticalOptions = LayoutOptions.Center, FontSize = 14 };
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
        new ColumnDefinition { Width = new GridLength(0.48, GridUnitType.Star) }, 
        new ColumnDefinition { Width = new GridLength(0.52, GridUnitType.Star) } 
    }, 
    ColumnSpacing = 10, 
    HeightRequest = 300 
};
        // 1. Левая панель (Список)
        var shopList = new VerticalStackLayout { Spacing = 8 };
        var shopHeader = new HorizontalStackLayout { Spacing = 8, Children = { new Image { Source = "icon_basket.png", WidthRequest = 20, HeightRequest = 20 }.BindTint(ThemeManager.AccentColor), new Label { Text = Loc.T("ShoppingList"), FontSize = 16, FontAttributes = FontAttributes.Bold }.BindText() } };
        shopList.Children.Add(shopHeader);

        _shopItemsContainer = new VerticalStackLayout { Spacing = 8 };
        var shopScroll = new ScrollView { Content = _shopItemsContainer, VerticalOptions = LayoutOptions.FillAndExpand };
        shopList.Children.Add(shopScroll);

        // Кнопки на одной строке
        var btnStack = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8 };
        var btnAdd = new Button { Text = "+ " + Loc.T("Add"), BackgroundColor = Colors.Transparent, BorderColor = ThemeManager.AccentColor, BorderWidth = 1, CornerRadius = 12, TextColor = ThemeManager.AccentColor };
        btnAdd.Clicked += async (_, _) => await ShowAddItemPopup();
        var btnDel = new Button { Text = "🗑 " + Loc.T("Delete"), BackgroundColor = Colors.Transparent, BorderColor = Color.FromArgb("#FF414D"), BorderWidth = 1, CornerRadius = 12, TextColor = Color.FromArgb("#FF414D") };
        btnDel.Clicked += (_, _) => { _shoppingItems.RemoveAll(i => i.IsChecked); SaveShoppingList(); RenderShoppingList(); };
        btnStack.Add(btnAdd, 0, 0); btnStack.Add(btnDel, 1, 0);
        shopList.Children.Add(btnStack);

        var leftPanel = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 20 }, Padding = 10, Content = shopList };
        leftPanel.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        leftPanel.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);

        // 2. Правая панель (Карта)
        var mapPlaceholder = new Border { BackgroundColor = Color.FromArgb("#111116"), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 20 } };
        var mapStack = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, Spacing = 10 };
        var mapIcon = new Image { Source = "icon_map.png", WidthRequest = 40, HeightRequest = 40 };
        mapIcon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        mapStack.Children.Add(mapIcon);
        mapStack.Children.Add(new Label { Text = "Карта скоро", TextColor = Colors.Gray, FontSize = 14 });
        mapPlaceholder.Content = mapStack;

        mainGrid.Add(leftPanel, 0, 0);
        mainGrid.Add(mapPlaceholder, 1, 0);

        // 3. Магазины (под картой)
        _storesListLayout = new VerticalStackLayout { Spacing = 4, Padding = 12 };
        var storesBorder = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Content = _storesListLayout };
        storesBorder.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        storesBorder.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);

        // 4. Шаблон
        var templateBtn = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = 12 };
        templateBtn.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        templateBtn.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);
        var templateGrid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        templateGrid.Add(new Image { Source = "icon_box.png", WidthRequest = 24, HeightRequest = 24 }.BindTint(ThemeManager.AccentColor), 0, 0);
        var tStack = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { new Label { Text = Loc.T("TemplateList"), FontSize = 14, FontAttributes = FontAttributes.Bold }.BindText(), new Label { Text = Loc.T("TemplateDesc"), FontSize = 11 }.BindText(true) } };
        templateGrid.Add(tStack, 1, 0);
        templateGrid.Add(new Image { Source = "icon_chevron_right.png", WidthRequest = 16, HeightRequest = 16 }.BindTint(Colors.Gray), 2, 0);
        templateBtn.Content = templateGrid;
        templateBtn.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await ShowTemplatesPopup()) });

        var content = new VerticalStackLayout { Padding = 16, Spacing = 12, Children = { headerLayout, searchFrame, mainGrid, storesBorder, templateBtn } };
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

    private async Task ShowAddItemPopup()
    {
        var overlay = new Grid { BackgroundColor = Color.FromRgba(0,0,0,0.7), ZIndex = 100 };
        var popup = new Border { StrokeThickness = 2, Stroke = ThemeManager.AccentColor, StrokeShape = new RoundRectangle { CornerRadius = 20 }, BackgroundColor = (Color)Application.Current!.Resources[ThemeManager.Surface], Padding = 20, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, WidthRequest = 320 };

        var nameEntry = new Entry { Placeholder = "Название", HeightRequest = 50 };
        var qtyEntry = new Entry { Placeholder = "Кол-во", Text = "1 шт.", HeightRequest = 50 };
        ThemeManager.BindEntry(nameEntry); ThemeManager.BindEntry(qtyEntry);

        var emojis = new[] { "🥛", "🍞", "🥩", "🍎", "🥤", "🧀", "🥚", "🧻", "🍫" };
        string selectedEmoji = "🛒";
        var emojiGrid = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
        foreach (var em in emojis) { var lbl = new Label { Text = em, FontSize = 24, Padding = 4 }; lbl.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => selectedEmoji = em) }); emojiGrid.Children.Add(lbl); }

        var saveBtn = new Button { Text = "Сохранить", BackgroundColor = ThemeManager.AccentColor, TextColor = Colors.White, CornerRadius = 10 };
        var cancelBtn = new Button { Text = "Отмена", BackgroundColor = Colors.Transparent, TextColor = Colors.Gray, CornerRadius = 10 };

        popup.Content = new VerticalStackLayout { Spacing = 12, Children = { new Label { Text = "Новый товар", FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center }, nameEntry, qtyEntry, new Label { Text = "Эмодзи:", FontSize = 12, TextColor = Colors.Gray }, emojiGrid, new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8, Children = { cancelBtn, saveBtn } } } };
        var btnGrid = (Grid)((VerticalStackLayout)popup.Content).Children.Last(); btnGrid.Add(cancelBtn, 0, 0); btnGrid.Add(saveBtn, 1, 0);

        overlay.Add(popup); _rootGrid.Add(overlay);
        var tcs = new TaskCompletionSource<bool>();
        saveBtn.Clicked += (_, _) => { if (!string.IsNullOrWhiteSpace(nameEntry.Text)) { var priceSrv = IPlatformApplication.Current!.Services.GetService<PriceService>()!; _shoppingItems.Add(new ShoppingItem { Name = nameEntry.Text, Quantity = qtyEntry.Text, Emoji = selectedEmoji, Price = priceSrv.GetPrice(nameEntry.Text) }); SaveShoppingList(); RenderShoppingList(); } tcs.SetResult(true); };
        cancelBtn.Clicked += (_, _) => tcs.SetResult(false);
        await tcs.Task; _rootGrid.Remove(overlay);
    }

    private async Task ShowTemplatesPopup()
    {
        var overlay = new Grid { BackgroundColor = Color.FromRgba(0,0,0,0.7), ZIndex = 100 };
        var popup = new Border { StrokeThickness = 2, Stroke = ThemeManager.AccentColor, StrokeShape = new RoundRectangle { CornerRadius = 20 }, BackgroundColor = (Color)Application.Current!.Resources[ThemeManager.Surface], Padding = 20, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, WidthRequest = 280 };
        var stack = new VerticalStackLayout { Spacing = 12 };
        stack.Children.Add(new Label { Text = "Шаблоны", FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center });
        var btnClassic = new Button { Text = "Классический", BackgroundColor = ThemeManager.AccentColor, TextColor = Colors.White, CornerRadius = 10 };
        var btnPicnic = new Button { Text = "Пикник", BackgroundColor = ThemeManager.AccentColor, TextColor = Colors.White, CornerRadius = 10 };
        var btnCustom = new Button { Text = "Создать свой", BackgroundColor = Colors.Transparent, BorderColor = ThemeManager.AccentColor, BorderWidth = 1, TextColor = ThemeManager.AccentColor, CornerRadius = 10 };
        var btnCancel = new Button { Text = "Назад", BackgroundColor = Colors.Transparent, TextColor = Colors.Gray, CornerRadius = 10 };
        stack.Children.Add(btnClassic); stack.Children.Add(btnPicnic); stack.Children.Add(btnCustom); stack.Children.Add(btnCancel);
        popup.Content = stack; overlay.Add(popup); _rootGrid.Add(overlay);
        var tcs = new TaskCompletionSource<bool>();
        btnClassic.Clicked += (_, _) => { _shoppingItems.AddRange(_storeService.GetTemplate("Базовый")); SaveShoppingList(); RenderShoppingList(); tcs.SetResult(true); };
        btnPicnic.Clicked += (_, _) => { _shoppingItems.AddRange(_storeService.GetTemplate("Для пикника")); SaveShoppingList(); RenderShoppingList(); tcs.SetResult(true); };
        btnCustom.Clicked += async (_, _) => { _rootGrid.Remove(overlay); await ShowAddItemPopup(); tcs.SetResult(true); };
        btnCancel.Clicked += (_, _) => tcs.SetResult(false);
        await tcs.Task; _rootGrid.Remove(overlay);
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
        _storesListLayout.Children.Add(new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(0, 0, 0, 4), Children = { new Image { Source = "icon_store.png", WidthRequest = 20, HeightRequest = 20 }.BindTint(ThemeManager.AccentColor), new Label { Text = Loc.T("AvailableStores"), FontSize = 14, FontAttributes = FontAttributes.Bold }.BindText() } });
        foreach (var store in _nearbyStores)
        {
            var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8, Padding = new Thickness(0, 8) };
            grid.Add(new Border { WidthRequest = 30, HeightRequest = 30, StrokeShape = new RoundRectangle { CornerRadius = 8 }, BackgroundColor = Colors.White, Content = new Label { Text = store.Logo, TextColor = Colors.Red, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center } }, 0, 0);
            grid.Add(new Label { Text = store.Name, FontSize = 13, VerticalOptions = LayoutOptions.Center }.BindText(), 1, 0);
            grid.Add(new Label { Text = $"{store.DistanceKm:F1} км", FontSize = 12, TextColor = Colors.Gray, VerticalOptions = LayoutOptions.Center }, 2, 0);
            grid.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Map.Default.OpenAsync(new Location(store.Location.Latitude, store.Location.Longitude), new MapLaunchOptions { Name = store.Name })) });
            _storesListLayout.Children.Add(grid);
        }
    }
}

// Вспомогательный класс для быстрой привязки цветов
public static class ViewExtensions
{
    public static T BindTint<T>(this T view, Color color) where T : Image { view.Behaviors.Add(new IconTintColorBehavior { TintColor = color }); return view; }
    public static T BindText<T>(this T view, bool secondary = false) where T : Label { ThemeManager.BindText(view, secondary); return view; }
}
