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
        var headerLayout = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10 };
        var title = new Label { Text = Loc.T("StoreTitle"), FontSize = 28, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(title);

        var locStack = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
        var locImg = new Image { Source = "icon_pin.png", WidthRequest = 20, HeightRequest = 20 };
        locImg.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        _locText = new Label { Text = "Поиск GPS...", TextColor = ThemeManager.AccentColor, VerticalOptions = LayoutOptions.Center };
        locStack.Children.Add(locImg); locStack.Children.Add(_locText);
        headerLayout.Add(title, 0, 0); headerLayout.Add(locStack, 1, 0);

        var search = new SearchBar { Placeholder = Loc.T("SearchStores"), HeightRequest = 50 };
        ThemeManager.BindSearch(search);
        var searchFrame = new Border { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = new Thickness(4, 0), Content = search };
        searchFrame.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);

        //  КАРТА (не менее 25% экрана)
        var mapPlaceholder = new Border { BackgroundColor = Color.FromArgb("#111116"), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 20 }, HeightRequest = 250 };
        var mapStack = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, Spacing = 10 };
        var mapIcon = new Image { Source = "icon_map.png", WidthRequest = 40, HeightRequest = 40 };
        mapIcon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var mapText = new Label { Text = "Карта скоро появится", TextColor = Colors.Gray, FontSize = 14 };
        mapStack.Children.Add(mapIcon); mapStack.Children.Add(mapText);
        mapPlaceholder.Content = mapStack;

        //  МАГАЗИНЫ (под картой)
        _storesListLayout = new VerticalStackLayout { Spacing = 4, Padding = 12 };
        var storesBorder = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Content = _storesListLayout };
        storesBorder.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        storesBorder.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);

        // СПИСОК ПОКУПОК (со скроллом)
        var shopList = new VerticalStackLayout { Spacing = 10 };
        var shopHeader = new HorizontalStackLayout { Spacing = 8 };
        var shopIcon = new Image { Source = "icon_basket.png", WidthRequest = 20, HeightRequest = 20 };
        shopIcon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var shopTitle = new Label { Text = Loc.T("ShoppingList"), FontSize = 16, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(shopTitle);
        shopHeader.Children.Add(shopIcon); shopHeader.Children.Add(shopTitle);
        shopList.Children.Add(shopHeader);

        _shopItemsContainer = new VerticalStackLayout { Spacing = 8 };
        var shopScroll = new ScrollView { Content = _shopItemsContainer, HeightRequest = 250 }; // ✅ Скролл
        shopList.Children.Add(shopScroll);

        var btnStack = new HorizontalStackLayout { Spacing = 8 };
        var btnAdd = new Button { Text = "+", BackgroundColor = Colors.Transparent, BorderColor = ThemeManager.AccentColor, BorderWidth = 1, CornerRadius = 12, TextColor = ThemeManager.AccentColor, FontSize = 20, WidthRequest = 100 };
        btnAdd.Clicked += async (_, _) => await ShowAddItemPopup();
        
        var btnDel = new Button { Text = Loc.T("Delete"), BackgroundColor = Colors.Transparent, BorderColor = Color.FromArgb("#FF414D"), BorderWidth = 1, CornerRadius = 12, TextColor = Color.FromArgb("#FF414D") };
        btnDel.Clicked += (_, _) => { _shoppingItems.RemoveAll(i => i.IsChecked); SaveShoppingList(); RenderShoppingList(); };
        btnStack.Children.Add(btnAdd); btnStack.Children.Add(btnDel);
        shopList.Children.Add(btnStack);

        var shopBorder = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 20 }, Padding = 12, Content = shopList };
        shopBorder.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        shopBorder.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);

        // ШАБЛОН (в самом низу)
        var templateBtn = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = 12 };
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
        templateTap.Tapped += async (_, _) => await ShowTemplatesPopup();
        templateBtn.GestureRecognizers.Add(templateTap);

        var content = new VerticalStackLayout { Padding = 16, Spacing = 12, Children = { headerLayout, searchFrame, mapPlaceholder, storesBorder, shopBorder, templateBtn } };
        var scroll = new ScrollView { Content = content };

        _rootGrid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
        _rootGrid.Add(scroll, 0, 0);
        _rootGrid.Add(new BottomNavBar(1, _repo), 0, 1);

        Content = _rootGrid;
        ThemeManager.Apply();

        var userLoc = await _storeService.GetCurrentLocationAsync();
        if (userLoc != null)
        {
            _locText.Text = await _storeService.GetCityNameAsync(userLoc);
            _nearbyStores = _storeService.GetNearbyStores(userLoc);
        }
        else
        {
            _locText.Text = "GPS отключен";
            _nearbyStores = _storeService.GetNearbyStores(new Location(55.7558, 37.6173)); 
        }
        RenderStores();
        RenderShoppingList();
    }

    //  КАСТОМНОЕ ОКНО ДОБАВЛЕНИЯ
    private async Task ShowAddItemPopup()
    {
        var overlay = new Grid { BackgroundColor = Color.FromRgba(0,0,0,0.7), ZIndex = 100 };
        
        var nameEntry = new Entry { Placeholder = "Название (напр. Хлеб)", HeightRequest = 50 };
        var qtyEntry = new Entry { Placeholder = "Кол-во (напр. 1 шт.)", Text = "1 шт.", HeightRequest = 50 };
        ThemeManager.BindEntry(nameEntry); ThemeManager.BindEntry(qtyEntry);

        var emojiGrid = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, Direction = Microsoft.Maui.Layouts.FlexDirection.Row };
        string[] emojis = { "🛒", "🥛", "🍞", "🥩", "🍎", "🥤", "🧀", "🥚", "🧻", "🍫" };
        string selectedEmoji = "🛒";
        
        foreach (var em in emojis)
        {
            var lbl = new Label { Text = em, FontSize = 24, Padding = 4 };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => selectedEmoji = em;
            lbl.GestureRecognizers.Add(tap);
            emojiGrid.Children.Add(lbl);
        }

        var saveBtn = new Button { Text = "Сохранить", BackgroundColor = ThemeManager.AccentColor, TextColor = Colors.White, CornerRadius = 10 };
        var cancelBtn = new Button { Text = "Отмена", BackgroundColor = Colors.Transparent, TextColor = Colors.Gray, CornerRadius = 10 };

        var popup = new Border 
        { 
            StrokeThickness = 2, Stroke = ThemeManager.AccentColor, 
            StrokeShape = new RoundRectangle { CornerRadius = 20 }, 
            BackgroundColor = (Color)Application.Current!.Resources[ThemeManager.Surface], 
            Padding = 20, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
            WidthRequest = 320
        };

        popup.Content = new VerticalStackLayout
        {
            Spacing = 12,
            Children = { new Label { Text = "Новый товар", FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center }, nameEntry, qtyEntry, emojiGrid, new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8, Children = { cancelBtn, saveBtn } } }
        };

        // Add cancel to col 0, save to col 1
        var btnGrid = (Grid)((VerticalStackLayout)popup.Content).Children.Last();
        btnGrid.Add(cancelBtn, 0, 0); btnGrid.Add(saveBtn, 1, 0);

        overlay.Add(popup);
        _rootGrid.Add(overlay);

        var tcs = new TaskCompletionSource<bool>();
        saveBtn.Clicked += (_, _) => 
        { 
            if (!string.IsNullOrWhiteSpace(nameEntry.Text))
            {
                var priceSrv = IPlatformApplication.Current!.Services.GetService<PriceService>()!;
                _shoppingItems.Add(new ShoppingItem { Name = nameEntry.Text, Quantity = qtyEntry.Text, Emoji = selectedEmoji, Price = priceSrv.GetPrice(nameEntry.Text) });
                SaveShoppingList(); RenderShoppingList();
            }
            tcs.SetResult(true); 
        };
        cancelBtn.Clicked += (_, _) => tcs.SetResult(false);

        await tcs.Task;
        _rootGrid.Remove(overlay);
    }

    // ✅ КАСТОМНОЕ ОКНО ШАБЛОНОВ
    private async Task ShowTemplatesPopup()
    {
        var overlay = new Grid { BackgroundColor = Color.FromRgba(0,0,0,0.7), ZIndex = 100 };
        var popup = new Border 
        { 
            StrokeThickness = 2, Stroke = ThemeManager.AccentColor, 
            StrokeShape = new RoundRectangle { CornerRadius = 20 }, 
            BackgroundColor = (Color)Application.Current!.Resources[ThemeManager.Surface], 
            Padding = 20, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
            WidthRequest = 280
        };

        var stack = new VerticalStackLayout { Spacing = 12 };
        stack.Children.Add(new Label { Text = "Выберите шаблон", FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center });
        
        var btnClassic = new Button { Text = "Классический", BackgroundColor = ThemeManager.AccentColor, TextColor = Colors.White, CornerRadius = 10 };
        var btnPicnic = new Button { Text = "Пикник", BackgroundColor = ThemeManager.AccentColor, TextColor = Colors.White, CornerRadius = 10 };
        var btnCancel = new Button { Text = "Назад", BackgroundColor = Colors.Transparent, TextColor = Colors.Gray, CornerRadius = 10 };

        stack.Children.Add(btnClassic); stack.Children.Add(btnPicnic); stack.Children.Add(btnCancel);
        popup.Content = stack;
        overlay.Add(popup);
        _rootGrid.Add(overlay);

        var tcs = new TaskCompletionSource<bool>();
        btnClassic.Clicked += (_, _) => { _shoppingItems.AddRange(_storeService.GetTemplate("Базовый")); SaveShoppingList(); RenderShoppingList(); tcs.SetResult(true); };
        btnPicnic.Clicked += (_, _) => { _shoppingItems.AddRange(_storeService.GetTemplate("Для пикника")); SaveShoppingList(); RenderShoppingList(); tcs.SetResult(true); };
        btnCancel.Clicked += (_, _) => tcs.SetResult(false);

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
}
