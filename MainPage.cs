using FoodExpiryApp.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

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

        var densityButton = CreateIconButton("▤");
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

        ThemeManager.BindSearch(search);

        var searchFrame = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = 18
            },
            Padding = new Thickness(8, 0),
            Content = search
        };

        searchFrame.SetDynamicResource(
            VisualElement.BackgroundColorProperty,
            ThemeManager.Surface);

        search.TextChanged += (_, _) => Render();

        var scanButton = new Button
        {
            Text = "▦\nСканировать",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 19,
            HeightRequest = 120,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        scanButton.SetDynamicResource(
            VisualElement.BackgroundColorProperty,
            ThemeManager.Accent);

        scanButton.SetDynamicResource(
            Button.TextColorProperty,
            ThemeManager.AccentText);

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
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 19,
            HeightRequest = 120,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        addButton.SetDynamicResource(
            VisualElement.BackgroundColorProperty,
            ThemeManager.SurfaceAlt);

        addButton.SetDynamicResource(
            Button.TextColorProperty,
            ThemeManager.Text);

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
        var button = new Button
        {
            Text = symbol,
            FontSize = 25,
            CornerRadius = 19,
            WidthRequest = 58,
            HeightRequest = 58,
            Padding = 0
        };

        button.SetDynamicResource(
            VisualElement.BackgroundColorProperty,
            ThemeManager.SurfaceAlt);

        button.SetDynamicResource(
            Button.TextColorProperty,
            ThemeManager.Accent);

        return button;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        ThemeManager.Apply();

        try
        {
            all = await repo.All();
            Render();
        }
        catch (Exception ex)
        {
            cards.Children.Clear();

            var error = new Label
            {
                Text = "Не удалось загрузить продукты:\n" + ex.Message,
                FontSize = 14,
                TextColor = Color.FromArgb("#FF6878")
            };

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
                Text = all.Count == 0
                    ? "Здесь пока нет продуктов.\nНажми «Добавить», чтобы начать."
                    : "Ничего не найдено.",
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

        // Единый стиль символов вместо разноцветных эмодзи.
        var imageSymbol = product.Category switch
        {
            "Молочные" => "◉",
            "Напитки" => "◇",
            "Мясо и рыба" => "◈",
            "Овощи и фрукты" => "❋",
            "Заморозка" => "❄",
            _ => "□"
        };

        var imageLabel = new Label
        {
            Text = imageSymbol,
            FontSize = iconOnly ? 30 : compact ? 40 : 54,
            ThemeManager.BindText(imageLabel, secondary: true),
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
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
            Content = imageLabel
        };

        var name = new Label
        {
            Text = product.Name,
            FontSize = iconOnly ? 14 : compact ? 17 : 21,
            FontAttributes = FontAttributes.Bold,
            LineBreakMode = LineBreakMode.TailTruncation
        };

        ThemeManager.BindText(name);

        var details = new Label
        {
            Text = string.Join(" • ", new[]
            {
                product.Variant,
                product.Volume,
                items.Count > 1 ? $"{items.Count} шт." : ""
            }.Where(s => !string.IsNullOrWhiteSpace(s))),
            FontSize = compact ? 12 : 15,
            IsVisible = !iconOnly
        };

        ThemeManager.BindText(details, secondary: true);

        var expiryLabels = new FlexLayout
        {
            Direction = Microsoft.Maui.Layouts.FlexDirection.Row,
            Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap
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
            BackgroundColor = Colors.Transparent,
            WidthRequest = 35,
            Padding = 0
        };

        menuButton.SetDynamicResource(
            Button.TextColorProperty,
            ThemeManager.Accent);

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
            Text = "⌫",
            FontSize = 21,
            TextColor = Color.FromArgb("#FF647D"),
            BackgroundColor = Colors.Transparent,
            WidthRequest = 35,
            Padding = 0
        };

        deleteButton.Clicked += async (_, _) =>
        {
            var options = items.Count > 1
                ? new[] { "Удалить один экземпляр", "Удалить все экземпляры" }
                : new[] { "Удалить продукт" };

            var choice = await DisplayActionSheet(
                "Удаление продукта",
                "Отмена",
                null,
                options);

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

        // Полупрозрачная заливка сохраняет читаемость текста
        // и позволяет фону адаптироваться к текущей теме.
        var translucentTint = Color.FromRgba(
            originalTint.Red,
            originalTint.Green,
            originalTint.Blue,
            0.22f);

        var border = new Border
        {
            BackgroundColor = translucentTint,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = 22
            },
            Padding = new Thickness(iconOnly ? 10 : 16),
            Content = row
        };

        border.SetDynamicResource(
            Border.StrokeProperty,
            ThemeManager.Border);

        return border;
    }
}
