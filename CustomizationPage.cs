using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class CustomizationPage : ContentPage
{
    readonly VerticalStackLayout content = new() { Spacing = 16 };

    readonly string[] colors =
    {
        "#A978F5", "#8B5CF6", "#6750A4",
        "#3978F6", "#21A6D7", "#25B99A",
        "#55A630", "#D5A52B", "#F08A35",
        "#E45B75", "#D94A9B", "#8B929E"
    };

    public CustomizationPage()
    {
        Title = "Внешний вид";
        ThemeManager.BindPage(this); // фон из AppBackground
        content.Padding = 20;

        BuildContent();

        Content = new ScrollView { Content = content };
        ThemeManager.Apply();
    }

    void BuildContent()
    {
        content.Children.Clear();

        AddHeader();
        AddSection("Тема приложения");
        AddThemes();
        AddSection("Акцентный цвет");
        AddColors();
        AddSection("Иконка приложения");
        AddIcons();
        AddPreview();
    }

    void AddHeader()
    {
        var heading = new Label
        {
            Text = "Внешний вид",
            FontSize = 27,
            FontAttributes = FontAttributes.Bold
        };
        ThemeManager.BindText(heading);
        content.Children.Add(heading);

        var subtitle = new Label
        {
            Text = "Настрой приложение под себя",
            FontSize = 14
        };
        ThemeManager.BindText(subtitle, secondary: true);
        content.Children.Add(subtitle);
    }

    void AddSection(string title)
    {
        var label = new Label
        {
            Text = title,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 12, 0, 0)
        };
        ThemeManager.BindText(label);
        content.Children.Add(label);
    }

    void AddThemes()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 8
        };

        AddThemeButton(grid, 0, "black", "Чёрная", "#050507");
        AddThemeButton(grid, 1, "dark",  "Тёмная", "#111116");
        AddThemeButton(grid, 2, "light", "Светлая", "#F3F1F7");

        content.Children.Add(grid);
    }

    void AddThemeButton(Grid grid, int column, string id, string title, string color)
    {
        // Превью темы остаётся с "родным" цветом, а рамка выбора — акцентная
        var selected = ThemeManager.CurrentTheme == id;

        var button = new Button
        {
            Text = (selected ? "✓\n" : "") + title,
            BackgroundColor = Color.FromArgb(color),
            TextColor = id == "light" ? Colors.Black : Colors.White,
            BorderColor = selected ? ThemeManager.AccentColor : Color.FromArgb("#48434F"),
            BorderWidth = selected ? 2 : 1,
            CornerRadius = 14,
            HeightRequest = 76,
            FontSize = 13
        };

        button.Clicked += (_, _) =>
        {
            ThemeManager.SetTheme(id);   // сохранит + Apply()
            BuildContent();              // пересоберёт UI с новым выбором
        };

        grid.Add(button, column);
    }

    void AddColors()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            RowSpacing = 12,
            ColumnSpacing = 12
        };

        for (int i = 0; i < colors.Length; i++)
        {
            int index = i;
            int row = i / 4;
            int column = i % 4;

            bool selected = string.Equals(
                ThemeManager.CurrentAccent, colors[i],
                StringComparison.OrdinalIgnoreCase);

            var button = new Button
            {
                Text = selected ? "✓" : "",
                WidthRequest = 48,
                HeightRequest = 48,
                CornerRadius = 24,
                Padding = 0,
                BackgroundColor = Color.FromArgb(colors[i]),
                TextColor = Colors.White,
                BorderColor = selected ? Colors.White : Colors.Transparent,
                BorderWidth = selected ? 3 : 0
            };

            button.Clicked += (_, _) =>
            {
                ThemeManager.SetAccent(colors[index]);
                BuildContent();
            };

            grid.Add(button, column, row);
        }

        content.Children.Add(grid);
    }

    void AddIcons()
{
    var grid = new Grid
    {
        ColumnDefinitions =
        {
            new ColumnDefinition(GridLength.Star),
            new ColumnDefinition(GridLength.Star),
            new ColumnDefinition(GridLength.Star)
        },
        RowDefinitions =
        {
            new RowDefinition(GridLength.Auto),
            new RowDefinition(GridLength.Auto)
        },
        ColumnSpacing = 10,
        RowSpacing = 10
    };

    var iconItems = new[]
    {
        (Id: "classic", Name: "Базовая", Source: "appicon_classic.png"),
        (Id: "winter", Name: "Зимняя", Source: "appicon_winter.png"),
        (Id: "halloween", Name: "Хэллоуин", Source: "appicon_halloween.png"),
        (Id: "mono", Name: "Ч/Б", Source: "appicon_mono.png"),
        (Id: "neon", Name: "Неон", Source: "appicon_neon.png")
    };

    for (int i = 0; i < iconItems.Length; i++)
    {
        var item = iconItems[i];
        int row = i / 3;
        int column = i % 3;

        bool selected = ThemeManager.CurrentIcon == item.Id;

        // Контейнер для иконки и подписи
        var stack = new VerticalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.Center };

        var img = new Image 
        { 
            Source = item.Source, 
            WidthRequest = 64, 
            HeightRequest = 64, 
            Aspect = Aspect.AspectFit 
        };

        var label = new Label 
        { 
            Text = item.Name, 
            FontSize = 12, 
            HorizontalTextAlignment = TextAlignment.Center 
        };
        ThemeManager.BindText(label, secondary: !selected);

        stack.Children.Add(img);
        stack.Children.Add(label);

        // Рамка вокруг иконки
        var border = new Border
        {
            StrokeThickness = selected ? 3 : 1,
            Stroke = selected ? ThemeManager.AccentColor : Color.FromArgb("#333333"),
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = 8,
            Content = stack
        };
        border.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.SurfaceAlt);

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            ThemeManager.SetIcon(item.Id);
            // Перерисовываем страницу, чтобы обновить выделение
            Content = null; // Очистка (костыль, но рабочий)
            Content = new ScrollView { Content = content }; 
            ThemeManager.Apply();
        };
        border.GestureRecognizers.Add(tap);

        grid.Add(border, column, row);
    }

    content.Children.Add(grid);
}

    void AddPreview()
    {
        var header = new Label
        {
            Text = "Предпросмотр",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 12, 0, 0)
        };
        ThemeManager.BindText(header);
        content.Children.Add(header);

        var card = new Border
        {
            Stroke = ThemeManager.AccentColor,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = 16
        };
        card.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);

        var title = new Label
        {
            Text = "Молоко",
            FontSize = 19,
            FontAttributes = FontAttributes.Bold
        };
        ThemeManager.BindText(title);

        var details = new Label
        {
            Text = "2.5% • 0,930 л • 2 шт.",
            FontSize = 13
        };
        ThemeManager.BindText(details, secondary: true);

        var days = new Label
        {
            Text = "6 дней   •   18 дней",
            FontSize = 14
        };
        ThemeManager.BindText(days);

        card.Content = new VerticalStackLayout
        {
            Spacing = 8,
            Children = { title, details, days }
        };

        content.Children.Add(card);
    }
}
