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
    private Image densityImage; 

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
        var title = new Label
        {
            Text = "Мои продукты",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };
        ThemeManager.BindText(title);

        ThemeManager.BindText(countLabel, secondary: true);

        var header = new VerticalStackLayout
        {
            Spacing = 5,
            Children = { title, countLabel }
        };

        // --- Кнопка плотности (Смена иконок) ---
        densityImage = new Image { WidthRequest = 28, HeightRequest = 28 };
        UpdateDensityIcon(); // Ставим начальную иконку
        densityImage.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });

        var densityButton = CreateIconButtonWithImage(densityImage);
        densityButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() =>
            {
                density = (density + 1) % 3; // 0 -> 1 -> 2 -> 0
                UpdateDensityIcon();
                Render();
            })
        });

        // --- Кнопка настроек ---
        var settingsImg = new Image { Source = "icon_gear.png", WidthRequest = 28, HeightRequest = 28 };
        settingsImg.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var settingsButton = CreateIconButtonWithImage(settingsImg);
        settingsButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Navigation.PushAsync(new SettingsPage()))
        });

        var top = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10
        };

        top.Add(header, 0, 0);
        top.Add(densityButton, 1, 0);
        top.Add(settingsButton, 2, 0);

        ThemeManager.BindSearch(search);

        var searchFrame = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = new Thickness(8, 0),
            Content = search
        };
        searchFrame.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        search.TextChanged += (_, _) => Render();

        // --- Кнопка QR (Оставили без тонирования, как просил) ---
        var scanImg = new Image { Source = "icon_qr.png", WidthRequest = 40, HeightRequest = 40, HorizontalOptions = LayoutOptions.Center };
        var scanLabel = new Label { Text = "Сканировать", FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center };
        
        var scanButton = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 19 },
            BackgroundColor = ThemeManager.AccentColor,
            Padding = 10,
            Content = new VerticalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center, Children = { scanImg, scanLabel } }
        };
        scanButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await DisplayAlert("Сканирование QR", "Сканер ещё не подключён.", "Понятно"))
        });

        // --- Кнопка Добавить (Плюс оставили) ---
        var addImg = new Image { Source = "icon_plus.png", WidthRequest = 40, HeightRequest = 40, HorizontalOptions = LayoutOptions.Center };
        addImg.Behaviors.Add(new IconTintColorBehavior { TintColor = (Color)Application.Current.Resources[ThemeManager.Text] });
        var addLabel = new Label { Text = "Добавить", FontSize = 16, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center };
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

    private void UpdateDensityIcon()
    {
        densityImage.Source = density switch
        {
            0 => "icon_layout_2.png", // 2 строки
            1 => "icon_layout_3.png", // 3 строки
            2 => "icon_layout_4.png", // 4 квадрата
            _ => "icon_layout_2.png"
        };
    }

    private Border CreateIconButtonWithImage(Image img)
    {
        var border = new Border
        {
            WidthRequest = 58,
            HeightRequest = 58,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 19 },
            Content = img,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        border.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.SurfaceAlt);
        return border;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ThemeManager.Apply();

        // Обновляем цвет иконок при заходе на страницу (на случай смены акцента)
        if (densityImage != null) 
            densityImage.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });

        try
        {
            all = await repo.All();
            Render();
        }
        catch (Exception ex)
        {
            cards.Children.Clear();
            var error = new Label { Text = "Не удалось загрузить продукты:\n" + ex.Message, FontSize = 14, TextColor = Color.FromArgb("#FF6878") };
            cards.Children.Add(error);
        }
    }

    private void Render()
    {
        cards.Children.Clear();
        var query = search.Text?.Trim() ?? "";

        var filtered = all
            .Where(p =>
                string.IsNullOrWhiteSpace(query) ||
                (p.Name ?? "").Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (p.Brand ?? "").Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (p.Barcode ?? "").Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        countLabel.Text = $"Ваш холодильник • {all.Count} продуктов";

        if (filtered.Count == 0)
        {
            var empty = new Label
            {
                Text = all.Count == 0 ? "Здесь пока нет продуктов.\nНажми «Добавить», чтобы начать." : "Ничего не найдено.",
                FontSize = 16,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 28)
            };
            ThemeManager.BindText(empty, secondary: true);
            cards.Children.Add(empty);
            return;
        }

        foreach (var group in filtered.GroupBy(p => p.GroupKey))
        {
            var items = group.OrderBy(p => p.ExpiryDate).ToList();
            var product = items[0];
            cards.Children.Add(CreateProductCard(product, items));
        }
    }

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
            VerticalOptions = LayoutOptions.Fill
        };
        ThemeManager.BindText(imageLabel, secondary: true);
        
        var imagePlaceholder = new Border
        {
            WidthRequest = imageSize,
            HeightRequest = imageSize,
            BackgroundColor = Colors.White,
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
        if (!iconOnly)
        {
            textContent.Children.Add(details);
            textContent.Children.Add(expiryLabels);
        }
        else
        {
            textContent.Children.Add(expiryLabels);
        }

        // --- Точки и корзина (Image + Тонирование) ---
        var menuImg = new Image { Source = "icon_dots.png", WidthRequest = 24, HeightRequest = 24 };
        menuImg.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var menuButton = new Border { Content = menuImg, Padding = 4, BackgroundColor = Colors.Transparent };
        menuButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                var options = new List<string> { "Изменить характеристики", "Удалить один экземпляр" };
                if (items.Count > 1) options.Add("Удалить все экземпляры");
                var choice = await DisplayActionSheet(product.Name, "Отмена", null, options.ToArray());

                if (choice == "Изменить характеристики") await Navigation.PushAsync(new ProductEditPage(repo, items[0]));
                else if (choice == "Удалить один экземпляр") { await repo.Delete(items[0]); all = await repo.All(); Render(); }
                else if (choice == "Удалить все экземпляры") { foreach (var item in items) await repo.Delete(item); all = await repo.All(); Render(); }
            })
        });

        var deleteImg = new Image { Source = "icon_trash.png", WidthRequest = 24, HeightRequest = 24 };
        // Корзина всегда красная (не меняет цвет под акцент)
        deleteImg.Behaviors.Add(new IconTintColorBehavior { TintColor = Color.FromArgb("#FF647D") });
        var deleteButton = new Border { Content = deleteImg, Padding = 4, BackgroundColor = Colors.Transparent };
        deleteButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                var options = items.Count > 1 ? new[] { "Удалить один экземпляр", "Удалить все экземпляры" } : new[] { "Удалить продукт" };
                var choice = await DisplayActionSheet("Удаление продукта", "Отмена", null, options);

                if (choice == "Удалить один экземпляр" || choice == "Удалить продукт") await repo.Delete(items[0]);
                else if (choice == "Удалить все экземпляры") foreach (var item in items) await repo.Delete(item);
                else return;

                all = await repo.All();
                Render();
            })
        });

        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = iconOnly ? 8 : 14,
            VerticalOptions = LayoutOptions.Center
        };

        row.Add(imagePlaceholder, 0, 0);
        row.Add(textContent, 1, 0);

        var controls = new VerticalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center, Children = { menuButton, deleteButton } };
        row.Add(controls, 2, 0);

        var originalTint = CategoryStyle.Tint(product.Category);
        var translucentTint = Color.FromRgba(originalTint.Red, originalTint.Green, originalTint.Blue, 0.22f);

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
}
