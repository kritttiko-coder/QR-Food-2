using FoodExpiryApp.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Behaviors;
using Microsoft.Maui.Layouts;

namespace FoodExpiryApp;

public class MainPage : ContentPage
{
    private readonly ProductRepository repo;
    private readonly SearchBar search = new() { Placeholder = Loc.T("SearchPlaceholder"), FontSize = 17, HeightRequest = 58, CancelButtonColor = Color.FromArgb("#A978F5") };
    private readonly FlexLayout cards = new() { Direction = FlexDirection.Row, Wrap = FlexWrap.Wrap, JustifyContent = FlexJustify.SpaceBetween, AlignItems = FlexAlignItems.Start, Margin = new Thickness(0, 10, 0, 10) };
    private readonly Label countLabel = new() { FontSize = 15 };
    private int density;
    private List<Product> all = new();
    private Image? densityImage;

    public MainPage(ProductRepository repository)
    {
        repo = repository;
        Title = Loc.T("MyProducts");
        Padding = 0;
        try { ThemeManager.Apply(); ThemeManager.BindPage(this); BuildInterface(); }
        catch (Exception ex) { Content = new Label { Text = $"ОШИБКА:\n{ex.Message}", TextColor = Colors.Red, Padding = 20 }; }
    }

    private void BuildInterface()
    {
        var title = new Label { Text = Loc.T("MyProducts"), FontSize = 28, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };
        ThemeManager.BindText(title);
        ThemeManager.BindText(countLabel, secondary: true);
        var header = new VerticalStackLayout { Spacing = 5, Children = { title, countLabel } };

        densityImage = new Image { WidthRequest = 28, HeightRequest = 28 };
        UpdateDensityIcon();
        densityImage.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var densityButton = CreateIconButtonWithImage(densityImage);
        densityButton.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { density = (density + 1) % 3; UpdateDensityIcon(); Render(); }) });

        var statImg = new Image { Source = "icon_chart_bar.png", WidthRequest = 28, HeightRequest = 28 };
        statImg.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var statButton = CreateIconButtonWithImage(statImg);
        statButton.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => BottomNavBar.SwitchToTab(3, repo)) });

        // ИСПРАВЛЕНО: Открывает настройки, а не аккаунт
        var settingsImg = new Image { Source = "icon_gear.png", WidthRequest = 28, HeightRequest = 28 };
        settingsImg.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var settingsButton = CreateIconButtonWithImage(settingsImg);
        settingsButton.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Navigation.PushAsync(new SettingsPage(repo))) });

        var top = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8 };
        top.Add(header, 0, 0); top.Add(densityButton, 1, 0); top.Add(statButton, 2, 0); top.Add(settingsButton, 3, 0);

        ThemeManager.BindSearch(search);
        var searchFrame = new Border { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 18 }, Padding = new Thickness(8, 0), Content = search };
        searchFrame.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        search.TextChanged += (_, _) => Render();

        var scanButton = CreateActionButton("icon_qr.png", Loc.T("Scan"), ThemeManager.AccentColor, Colors.White);
        scanButton.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Navigation.PushAsync(new ScanPage(repo))) });

        var addButton = CreateActionButton("icon_plus.png", Loc.T("Add"), (Color)Application.Current!.Resources[ThemeManager.SurfaceAlt], (Color)Application.Current!.Resources[ThemeManager.Text]);
        addButton.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Navigation.PushAsync(new ProductEditPage(repo))) });

        var actions = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 12 };
        actions.Add(scanButton, 0, 0); actions.Add(addButton, 1, 0);

        var content = new VerticalStackLayout { Padding = new Thickness(18, 22, 18, 24), Spacing = 18, Children = { top, searchFrame, actions, cards } };
        var mainGrid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
        mainGrid.Add(new ScrollView { Content = content }, 0, 0);
        mainGrid.Add(new BottomNavBar(0, repo), 0, 1);

        Content = mainGrid;
        ThemeManager.Apply();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (Content == null) return;
        Content.GestureRecognizers.Clear();
        var swipeL = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        swipeL.Swiped += (_, _) => BottomNavBar.SwitchToTab(3, repo);
        Content.GestureRecognizers.Add(swipeL);
        var swipeR = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeR.Swiped += (_, _) => BottomNavBar.SwitchToTab(1, repo);
        Content.GestureRecognizers.Add(swipeR);
    }

    private Border CreateActionButton(string icon, string text, Color bg, Color textColor)
    {
        var img = new Image { Source = icon, WidthRequest = 40, HeightRequest = 40, HorizontalOptions = LayoutOptions.Center };
        img.Behaviors.Add(new IconTintColorBehavior { TintColor = textColor });
        var lbl = new Label { Text = text, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = textColor, HorizontalOptions = LayoutOptions.Center };
        return new Border { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 19 }, BackgroundColor = bg, Padding = 10, Content = new VerticalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center, Children = { img, lbl } } };
    }

    private Border CreateIconButtonWithImage(Image img)
    {
        var border = new Border { WidthRequest = 58, HeightRequest = 58, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 19 }, Content = img, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        border.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.SurfaceAlt);
        return border;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            ThemeManager.Apply();
            if (densityImage != null) { densityImage.Behaviors.Clear(); densityImage.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor }); }
            all = await repo.All(); Render();
        }
        catch (Exception ex) { cards.Children.Clear(); cards.Children.Add(new Label { Text = "Ошибка БД:\n" + ex.Message, FontSize = 12, TextColor = Color.FromArgb("#FF6878"), Margin = new Thickness(20) }); }
    }

    private void Render()
    {
        cards.Children.Clear();
        var query = search.Text?.Trim() ?? "";
        var filtered = all.Where(p => string.IsNullOrWhiteSpace(query) || (p.Name ?? "").Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        countLabel.Text = $"{Loc.T("FridgeCount")} • {all.Count} {Loc.T("ProductsCount")}";

        if (filtered.Count == 0)
        {
            var empty = new Label { Text = all.Count == 0 ? Loc.T("EmptyFridge") : Loc.T("NothingFound"), FontSize = 16, HorizontalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 28), WidthRequest = 300 };
            ThemeManager.BindText(empty, secondary: true);
            cards.Children.Add(empty); return;
        }

        foreach (var group in filtered.GroupBy(p => p.GroupKey))
        {
            var items = group.OrderBy(p => p.ExpiryDate).ToList();
            var card = CreateProductCard(items[0], items);
            if (density == 0) { card.WidthRequest = -1; FlexLayout.SetBasis(card, new FlexBasis(1f, true)); card.Margin = new Thickness(0, 0, 0, 14); }
            else if (density == 1) { card.WidthRequest = -1; FlexLayout.SetBasis(card, new FlexBasis(1f, true)); card.Margin = new Thickness(0, 0, 0, 10); }
            else { FlexLayout.SetBasis(card, new FlexBasis(0.48f, true)); card.Margin = new Thickness(0, 0, 0, 10); }
            cards.Children.Add(card);
        }
    }

    private Border CreateProductCard(Product product, List<Product> items)
    {
        var compact = density >= 1;
        var iconOnly = density == 2;
        var imageSize = iconOnly ? 60 : compact ? 80 : 100;

        var imgService = IPlatformApplication.Current!.Services.GetService<Services.ProductImageService>();
        var imgSource = imgService?.ResolveImageSource(product);
        View imageContent = imgSource != null 
            ? new Image { Source = imgSource, Aspect = Aspect.AspectFill, HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill } 
            : new Label { Text = product.Category switch { "Молочные" => "◉", "Напитки" => "◇", "Мясо и рыба" => "◈", "Овощи и фрукты" => "❋", "Заморозка" => "❄", _ => "□" }, FontSize = iconOnly ? 24 : compact ? 32 : 44, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, TextColor = Colors.White };

        var imagePlaceholder = new Border { WidthRequest = imageSize, HeightRequest = imageSize, BackgroundColor = Colors.Black, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 13 }, Padding = 0, Content = imageContent };
        var name = new Label { Text = product.Name, FontSize = iconOnly ? 13 : compact ? 15 : 19, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.TailTruncation };
        ThemeManager.BindText(name);
        var details = new Label { Text = string.Join(" • ", new[] { product.Variant, product.Volume, items.Count > 1 ? $"{items.Count} шт." : "" }.Where(s => !string.IsNullOrWhiteSpace(s))), FontSize = compact ? 11 : 14, IsVisible = !iconOnly };
        ThemeManager.BindText(details, secondary: true);

        var priceLabel = new Label
{
    Text = product.Price > 0 ? $"{product.Price:N0} ₽" : "",
    FontSize = 13,
    FontAttributes = FontAttributes.Bold,
    TextColor = ThemeManager.AccentColor,
    IsVisible = product.Price > 0
};

        var expiryLabels = new FlexLayout { Direction = FlexDirection.Row, Wrap = FlexWrap.Wrap };
        foreach (var item in items) expiryLabels.Children.Add(new Label { Text = ExpiryColor.Label(item.ExpiryDate), TextColor = ExpiryColor.For(item.ExpiryDate), FontSize = compact ? 11 : 15, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 2, 6, 0) });
        var textContent = new VerticalStackLayout { Spacing = 3, VerticalOptions = LayoutOptions.Center, Children = { name } };
        if (!iconOnly) { textContent.Children.Add(details); textContent.Children.Add(expiryLabels); } else { textContent.Children.Add(expiryLabels); }

        // КНОПКИ В КРАСИВОЙ РАМКЕ
        //  Стиль для кнопок-иконок (рамка, размер, фон)
var btnStyle = new Action<Border>(b => 
{ 
    b.StrokeThickness = 1; 
    b.Stroke = ThemeManager.AccentColor; 
    b.StrokeShape = new RoundRectangle { CornerRadius = 8 }; 
    b.BackgroundColor = Color.FromRgba(0,0,0,0.2f); 
    b.Padding = 4; 
    b.WidthRequest = 40; 
    b.HeightRequest = 40; 
});

//  Кнопка смены фото (камера)
var cameraImg = new Image 
{ 
    Source = "icon_camera.png", 
    WidthRequest = 35, 
    HeightRequest = 35, 
    HorizontalOptions = LayoutOptions.Center, 
    VerticalOptions = LayoutOptions.Center 
};
cameraImg.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
var cameraButton = new Border { Content = cameraImg }; 
btnStyle(cameraButton);
cameraButton.GestureRecognizers.Add(new TapGestureRecognizer 
{ 
    Command = new Command(async () => 
    {
        var imgService = IPlatformApplication.Current!.Services.GetService<Services.ProductImageService>();
        if (imgService == null) return;
        
        var key = product.Serial ?? product.Id.ToString();
        var path = await imgService.CaptureOwnPhotoAsync(key);
        if (path != null)
        {
            product.LocalImagePath = path;
            await repo.Save(product);
            all = await repo.All();
            Render();
        }
    }) 
});

// Кнопка меню
var menuImg = new Image { Source = "icon_dots.png", WidthRequest = 35, HeightRequest = 35, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
menuImg.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
var menuButton = new Border { Content = menuImg }; 
btnStyle(menuButton);
menuButton.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => {
    var options = new List<string> { Loc.T("Edit"), Loc.T("DeleteOne") };
    if (items.Count > 1) options.Add(Loc.T("DeleteAll"));
    var choice = await DisplayActionSheet(product.Name, Loc.T("Cancel"), null, options.ToArray());
    if (choice == Loc.T("Edit")) await Navigation.PushAsync(new ProductEditPage(repo, items[0]));
    else if (choice == Loc.T("DeleteOne")) { await repo.Delete(items[0]); all = await repo.All(); Render(); }
    else if (choice == Loc.T("DeleteAll")) { foreach (var item in items) await repo.Delete(item); all = await repo.All(); Render(); }
}) });

// Кнопка удаления
var deleteImg = new Image { Source = "icon_trash.png", WidthRequest = 35, HeightRequest = 35, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
deleteImg.Behaviors.Add(new IconTintColorBehavior { TintColor = Color.FromArgb("#FF647D") });
var deleteButton = new Border { Content = deleteImg }; 
btnStyle(deleteButton);
deleteButton.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => {
    var options = items.Count > 1 ? new[] { Loc.T("DeleteOne"), Loc.T("DeleteAll") } : new[] { Loc.T("DeleteProduct") };
    var choice = await DisplayActionSheet(Loc.T("Delete"), Loc.T("Cancel"), null, options);
    if (choice == Loc.T("DeleteOne") || choice == Loc.T("DeleteProduct")) await repo.Delete(items[0]);
    else if (choice == Loc.T("DeleteAll")) foreach (var item in items) await repo.Delete(item);
    else return;
    all = await repo.All(); Render();
}) });

// ВСЕ ТРИ КНОПКИ ГОРИЗОНТАЛЬНО
var controls = new HorizontalStackLayout 
{ 
    Spacing = 6, 
    VerticalOptions = LayoutOptions.Center, 
    HorizontalOptions = LayoutOptions.End, 
    Children = { cameraButton, menuButton, deleteButton } 
};
        
        var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10, VerticalOptions = LayoutOptions.Center };
        row.Add(imagePlaceholder, 0, 0); row.Add(textContent, 1, 0); row.Add(controls, 2, 0);

        var originalTint = CategoryStyle.Tint(product.Category);
        var border = new Border { BackgroundColor = Color.FromRgba(originalTint.Red, originalTint.Green, originalTint.Blue, 0.45f), StrokeThickness = 1.5, StrokeShape = new RoundRectangle { CornerRadius = 18 }, Padding = new Thickness(compact ? 10 : 14), Content = row };
        border.SetDynamicResource(Border.StrokeProperty, ThemeManager.Accent);
        return border;
    }

    private void UpdateDensityIcon()
    {
        if (densityImage == null) return;
        densityImage.Source = density switch { 0 => "icon_layout_2.png", 1 => "icon_layout_3.png", 2 => "icon_layout_4.png", _ => "icon_layout_2.png" };
    }
}
