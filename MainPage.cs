using FoodExpiryApp.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Behaviors;

namespace FoodExpiryApp;

public class MainPage : ContentPage
{
    private readonly ProductRepository repo;

    private readonly SearchBar search = new()
    {
        Placeholder = "Поиск продуктов...",
        FontSize = 17,
        HeightRequest = 58,
        CancelButtonColor = Color.FromArgb("#A978F5")
    };

    private readonly VerticalStackLayout cards = new()
    {
        Spacing = 14
    };

    private readonly Label countLabel = new()
    {
        FontSize = 15
    };

    private int density;
    private List<Product> all = new();
    
    // Ссылка на картинку плотности, чтобы менять её Source
    private Image? densityImage; 

    public MainPage(ProductRepository repository)
    {
        repo = repository;

        Title = "Мои продукты";
        Padding = 0;

        ThemeManager.Apply();
        ThemeManager.BindPage(this);

        BuildInterface();
    }


private void BuildInterface()
{
    var title = new Label { Text = Loc.T("MyProducts"), FontSize = 28, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };
    ThemeManager.BindText(title);

    ThemeManager.BindText(countLabel, secondary: true);

    var header = new VerticalStackLayout { Spacing = 5, Children = { title, countLabel } };

    densityImage = new Image { WidthRequest = 28, HeightRequest = 28 };
    UpdateDensityIcon();
    var densityButton = CreateIconButtonWithImage(densityImage);
    densityButton.GestureRecognizers.Add(new TapGestureRecognizer
    {
        Command = new Command(() => { density = (density + 1) % 3; UpdateDensityIcon(); Render(); })
    });

    var settingsImg = new Image { Source = "icon_gear.png", WidthRequest = 28, HeightRequest = 28 };
    // Привязываем цвет иконки настроек к акценту динамически
    settingsImg.SetDynamicResource(Image.TintColorProperty, ThemeManager.Accent); // В MAUI 8+ это работает!
    
    var settingsButton = CreateIconButtonWithImage(settingsImg);
    settingsButton.GestureRecognizers.Add(new TapGestureRecognizer
    {
        Command = new Command(async () => await Navigation.PushAsync(new SettingsPage()))
    });

    var top = new Grid
    {
        ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
        ColumnSpacing = 10
    };
    top.Add(header, 0, 0);
    top.Add(densityButton, 1, 0);
    top.Add(settingsButton, 2, 0);

    ThemeManager.BindSearch(search);
    var searchFrame = new Border { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 18 }, Padding = new Thickness(8, 0), Content = search };
    searchFrame.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
    search.TextChanged += (_, _) => Render();

    // Кнопка QR
    var scanImg = new Image { Source = "icon_qr.png", WidthRequest = 40, HeightRequest = 40, HorizontalOptions = LayoutOptions.Center };
    var scanLabel = new Label { Text = Loc.T("Scan"), FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center };
    var scanButton = new Border
    {
        StrokeThickness = 0,
        StrokeShape = new RoundRectangle { CornerRadius = 19 },
        Padding = 10,
        Content = new VerticalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center, Children = { scanImg, scanLabel } }
    };
    // Привязываем цвет фона кнопки QR к акценту!
    scanButton.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Accent);
    
    scanButton.GestureRecognizers.Add(new TapGestureRecognizer
    {
        Command = new Command(async () => await DisplayAlert(Loc.T("Scan"), "Сканер ещё не подключён.", "ОК"))
    });

    // Кнопка Добавить
    var addImg = new Image { Source = "icon_plus.png", WidthRequest = 40, HeightRequest = 40, HorizontalOptions = LayoutOptions.Center };
    addImg.SetDynamicResource(Image.TintColorProperty, ThemeManager.Text);
    var addLabel = new Label { Text = Loc.T("Add"), FontSize = 16, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center };
    ThemeManager.BindText(addLabel);

    var addButton = new Border
    {
        StrokeThickness = 0,
        StrokeShape = new RoundRectangle { CornerRadius = 19 },
        Padding = 10,
        Content = new VerticalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center, Children = { addImg, addLabel } }
    };
    addButton.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.SurfaceAlt);
    addButton.GestureRecognizers.Add(new TapGestureRecognizer
    {
        Command = new Command(async () => await Navigation.PushAsync(new ProductEditPage(repo)))
    });

    var actions = new Grid
    {
        ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
        ColumnSpacing = 12
    };
    actions.Add(scanButton, 0, 0);
    actions.Add(addButton, 1, 0);

    var content = new VerticalStackLayout
    {
        Padding = new Thickness(18, 22, 18, 24),
        Spacing = 18,
        Children = { top, searchFrame, actions, cards }
    };

    Content = new ScrollView { Content = content };
}

// И метод создания карточки (исправлена верстка и цвет картинки):
private Border CreateProductCard(Product product, List<Product> items)
{
    var compact = density == 1;
    var iconOnly = density == 2;
    var imageSize = iconOnly ? 62 : compact ? 88 : 114;

    var imageSymbol = product.Category switch
    {
        "Молочные" => "◉", "Напитки" => "◇", "Мясо и рыба" => "◈",
        "Овощи и фрукты" => "❋", "Заморозка" => "❄", _ => "□"
    };

    var imageLabel = new Label
    {
        Text = imageSymbol,
        FontSize = iconOnly ? 30 : compact ? 40 : 54,           
        HorizontalTextAlignment = TextAlignment.Center,
        VerticalTextAlignment = TextAlignment.Center,
        HorizontalOptions = LayoutOptions.Fill,
        VerticalOptions = LayoutOptions.Fill,
        TextColor = Colors.White // Белая иконка на черном фоне
    };
    
    
    var imagePlaceholder = new Border
    {
        WidthRequest = imageSize,
        HeightRequest = imageSize,
        BackgroundColor = Colors.Black, // Черный фон
        StrokeThickness = 0,
        StrokeShape = new RoundRectangle { CornerRadius = 13 },
        Padding = 7,
        Content = imageLabel
    };

    var name = new Label { Text = product.Name, FontSize = iconOnly ? 14 : compact ? 17 : 21, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.TailTruncation };
    ThemeManager.BindText(name);

    var details = new Label
    {
        Text = string.Join(" • ", new[] { product.Variant, product.Volume, items.Count > 1 ? $"{items.Count} шт." : "" }.Where(s => !string.IsNullOrWhiteSpace(s))),
        FontSize = compact ? 12 : 15,
        IsVisible = !iconOnly
    };
    ThemeManager.BindText(details, secondary: true);

    var expiryLabels = new FlexLayout { Direction = Microsoft.Maui.Layouts.FlexDirection.Row, Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };

    foreach (var item in items)
    {
        expiryLabels.Children.Add(new Label
        {
            Text = ExpiryColor.Label(item.ExpiryDate),
            TextColor = ExpiryColor.For(item.ExpiryDate),
            FontSize = compact ? 12 : 17,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 3, 8, 0)
        });
    }

    var textContent = new VerticalStackLayout { Spacing = 5, VerticalOptions = LayoutOptions.Center, Children = { name } };
    if (!iconOnly) {
        textContent.Children.Add(details);
        textContent.Children.Add(expiryLabels);
    } else {
        textContent.Children.Add(expiryLabels);
    }
    var menuImg = new Image { Source = "icon_dots.png", WidthRequest = 24, HeightRequest = 24 };
    menuImg.SetDynamicResource(Image.TintColorProperty, ThemeManager.Accent);
    var menuButton = new Border { Content = menuImg, Padding = 4, BackgroundColor = Colors.Transparent, VerticalOptions = LayoutOptions.Center };
    menuButton.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await ShowMenu(product, items)) });

    var deleteImg = new Image { Source = "icon_trash.png", WidthRequest = 24, HeightRequest = 24 };
    deleteImg.Behaviors.Add(new CommunityToolkit.Maui.Behaviors.IconTintColorBehavior { TintColor = Color.FromArgb("#FF647D") }); // Красная всегда
    var deleteButton = new Border { Content = deleteImg, Padding = 4, BackgroundColor = Colors.Transparent, VerticalOptions = LayoutOptions.Center };
    deleteButton.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await DeleteProduct(product, items)) });

    // Ставим кнопки в горизонтальный стек, чтобы они были сбоку аккуратно
    var controls = new HorizontalStackLayout
    {
        Spacing = 8,
        VerticalOptions = LayoutOptions.Center,
        Children = { menuButton, deleteButton }
    };

    var row = new Grid
    {
        ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
        ColumnSpacing = iconOnly ? 8 : 14,
        VerticalOptions = LayoutOptions.Center
    };

    row.Add(imagePlaceholder, 0, 0);
    row.Add(textContent, 1, 0);
    row.Add(controls, 2, 0); // Кнопки справа

    var originalTint = CategoryStyle.Tint(product.Category);
    var translucentTint = Color.FromRgba(originalTint.Red, originalTint.Green, originalTint.Blue, 0.45f);

    var border = new Border
    {
        BackgroundColor = translucentTint,
        StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = 22 },
        Padding = new Thickness(iconOnly ? 10 : 16),
        Content = row
    };
    border.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);

    return border;
}
    
        
            
            
        
