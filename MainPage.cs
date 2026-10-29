
using FoodExpiryApp.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace FoodExpiryApp;

public class MainPage : ContentPage
{
    private readonly ProductRepository repo;

    private readonly SearchBar search = new()
    {
        Placeholder = "Поиск продуктов...",
        FontSize = 17,
        HeightRequest = 58,
        BackgroundColor = Color.FromArgb("#25232C"),
        TextColor = Colors.White,
        PlaceholderColor = Color.FromArgb("#B8B2C3"),
        CancelButtonColor = Color.FromArgb("#C5A0FF")
    };

    private readonly VerticalStackLayout cards = new()
    {
        Spacing = 14
    };

    private readonly Label countLabel = new()
    {
        FontSize = 16,
        TextColor = Color.FromArgb("#B8B2C3")
    };

    private readonly Color background = Color.FromArgb("#111016");
    private readonly Color panel = Color.FromArgb("#25232C");
    private readonly Color accent = Color.FromArgb("#A978F5");

    private int density;
    private List<Product> all = new();

    public MainPage(ProductRepository repository)
    {
        repo = repository;

        Title = "Мои продукты";
        BackgroundColor = background;
        Padding = 0;

        BuildInterface();
    }

    private void BuildInterface()
    {
        var title = new Label
        {
            Text = "Мои продукты",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center
        };

        var header = new VerticalStackLayout
        {
            Spacing = 5,
            Children = { title, countLabel }
        };

        var densityButton = CreateIconButton("☷");
        densityButton.Clicked += (_, _) =>
        {
            density = (density + 1) % 3;
            Render();
        };

        var settingsButton = CreateIconButton("⚙");
        settingsButton.Clicked += async (_, _) =>
        {
            await Navigation.PushAsync(new SettingsPage());
        };

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

        var searchFrame = new Border
        {
            BackgroundColor = panel,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = 18
            },
            Padding = new Thickness(8, 0),
            Content = search
        };

        search.TextChanged += (_, _) => Render();

        var scanButton = new Button
        {
            Text = "▥\nСканировать",
            FontSize = 19,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#7943B8"),
            CornerRadius = 19,
            HeightRequest = 120,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        scanButton.Clicked += async (_, _) =>
        {
            await DisplayAlert(
                "Сканирование QR",
                "Сканер ещё не подключён. Его реализацию нужно добавить отдельно.",
                "Понятно");
        };

        var addButton = new Button
        {
            Text = "+\nДобавить",
            FontSize = 19,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#30253D"),
            CornerRadius = 19,
            HeightRequest = 120,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        addButton.Clicked += async (_, _) =>
        {
            await Navigation.PushAsync(new ProductEditPage(repo));
        };

        var actions = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 12
        };

        actions.Add(scanButton, 0, 0);
        actions.Add(addButton, 1, 0);

        var content = new VerticalStackLayout
        {
            Padding = new Thickness(18, 22, 18, 24),
            Spacing = 18,
            Children =
            {
                top,
                searchFrame,
                actions,
                cards
            }
        };

        Content = new ScrollView
        {
            Content = content
        };
    }

    private Button CreateIconButton(string symbol)
    {
        return new Button
        {
            Text = symbol,
            FontSize = 25,
            TextColor = Color.FromArgb("#C5A0FF"),
            BackgroundColor = Color.FromArgb("#30253D"),
            CornerRadius = 19,
            WidthRequest = 62,
            HeightRequest = 62,
            Padding = 0
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            all = await repo.All();
            Render();
        }
        catch (Exception ex)
        {
            cards.Children.Clear();

            cards.Children.Add(new Label
            {
                Text = "Не удалось загрузить продукты:\n" + ex.Message,
                TextColor = Color.FromArgb("#FF6878"),
                FontSize = 14
            });
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
            cards.Children.Add(new Label
            {
                Text = all.Count == 0
                    ? "Здесь пока нет продуктов.\nНажми «Добавить», чтобы начать."
                    : "Ничего не найдено.",
                FontSize = 16,
                TextColor = Color.FromArgb("#B8B2C3"),
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 28)
            });

            return;
        }

        foreach (var group in filtered.GroupBy(p => p.GroupKey))
        {
            var items = group.OrderBy(p => p.ExpiryDate).ToList();
            var product = items[0];

            var card = CreateProductCard(product, items);
            cards.Children.Add(card);
        }
    }

    private Border CreateProductCard(Product product, List<Product> items)
    {
        var compact = density == 1;
        var iconOnly = density == 2;

        var imageSize = iconOnly ? 62 : compact ? 88 : 114;

        // В текущей модели Product в присланном коде нет известного
        // поля с путём к изображению. Пока используем категорийный символ.
        var imageSymbol = product.Category switch
        {
            "Молочные" => "🥛",
            "Напитки" => "💧",
            "Мясо и рыба" => "🥩",
            "Овощи и фрукты" => "🥬",
            "Заморозка" => "🧊",
            _ => "📦"
        };

        var imagePlaceholder = new Border
        {
            WidthRequest = imageSize,
            HeightRequest = imageSize,
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = 13
            },
            Padding = 7,
            Content = new Label
            {
                Text = imageSymbol,
                FontSize = iconOnly ? 30 : compact ? 40 : 54,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            }
        };

        var name = new Label
        {
            Text = product.Name,
            FontSize = iconOnly ? 14 : compact ? 17 : 21,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            LineBreakMode = LineBreakMode.TailTruncation
        };

        var details = new Label
        {
            Text = string.Join(" • ", new[]
            {
                product.Variant,
                product.Volume,
                items.Count > 1 ? $"{items.Count} шт." : ""
            }.Where(s => !string.IsNullOrWhiteSpace(s))),
            FontSize = compact ? 12 : 16,
            TextColor = Color.FromArgb("#D0CAD8"),
            IsVisible = !iconOnly
        };

        var expiryLabels = new FlexLayout
        {
            Direction = FlexDirection.Row,
            Wrap = FlexWrap.Wrap
        };

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

        var textContent = new VerticalStackLayout
        {
            Spacing = 5,
            VerticalOptions = LayoutOptions.Center,
            Children = { name }
        };

        if (!iconOnly)
        {
            textContent.Children.Add(details);
            textContent.Children.Add(expiryLabels);
        }
        else
        {
            textContent.Children.Add(expiryLabels);
        }

        var menuButton = new Button
        {
            Text = "⋮",
            FontSize = 25,
            TextColor = Color.FromArgb("#C5A0FF"),
            BackgroundColor = Colors.Transparent,
            WidthRequest = 35,
            Padding = 0
        };

        menuButton.Clicked += async (_, _) =>
{
    var options = new List<string>
    {
        "Изменить характеристики",
        "Удалить один экземпляр"
    };

    if (items.Count > 1)
        options.Add("Удалить все экземпляры");

    var choice = await DisplayActionSheet(
        product.Name,
        "Отмена",
        null,
        options.ToArray());

    if (choice == "Изменить характеристики")
    {
        await Navigation.PushAsync(
            new ProductEditPage(repo, items[0]));
    }
    else if (choice == "Удалить один экземпляр")
    {
        await repo.Delete(items[0]);
        all = await repo.All();
        Render();
    }
    else if (choice == "Удалить все экземпляры")
    {
        foreach (var item in items)
            await repo.Delete(item);

        all = await repo.All();
        Render();
    }
};

        var deleteButton = new Button
        {
            Text = "▤",
            FontSize = 21,
            TextColor = Color.FromArgb("#FF647D"),
            BackgroundColor = Colors.Transparent,
            WidthRequest = 35,
            Padding = 0
        };

        deleteButton.Clicked += async (_, _) =>
        {
            var choice = await DisplayActionSheet(
                "Удаление продукта",
                "Отмена",
                null,
                items.Count > 1 ? "Удалить один экземпляр" : "Удалить продукт",
                items.Count > 1 ? "Удалить все экземпляры" : " ");

            if (choice == "Удалить один экземпляр" ||
                choice == "Удалить продукт")
            {
                await repo.Delete(items[0]);
            }
            else if (choice == "Удалить все экземпляры")
            {
                foreach (var item in items)
                    await repo.Delete(item);
            }
            else
            {
                return;
            }

            all = await repo.All();
            Render();
        };

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = iconOnly ? 8 : 14,
            VerticalOptions = LayoutOptions.Center
        };

        row.Add(imagePlaceholder, 0, 0);
        row.Add(textContent, 1, 0);

        var controls = new VerticalStackLayout
        {
            Spacing = 0,
            VerticalOptions = LayoutOptions.Center,
            Children = { menuButton, deleteButton }
        };

        row.Add(controls, 2, 0);

        var originalTint = CategoryStyle.Tint(product.Category);
        var translucentTint = Blend(originalTint, background, 0.72);

        return new Border
        {
            BackgroundColor = translucentTint,
            Stroke = Color.FromArgb("#302D38"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = 22
            },
            Padding = new Thickness(iconOnly ? 10 : 16),
            Content = row
        };
    }

    private static Color Blend(Color foreground, Color background, float backgroundWeight)
    {
        var foregroundWeight = 1f - backgroundWeight;

        return Color.FromRgba(
            foreground.Red * foregroundWeight + background.Red * backgroundWeight,
            foreground.Green * foregroundWeight + background.Green * backgroundWeight,
            foreground.Blue * foregroundWeight + background.Blue * backgroundWeight,
            1);
    }
}
