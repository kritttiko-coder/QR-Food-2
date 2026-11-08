using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class CustomizationPage : ContentPage
{
    readonly VerticalStackLayout content = new() { Spacing = 16 };

    readonly string[] colors =
{
    // Ряд 1: Фиолетовые и синие (от светлого к темному)
    "#E8D5FF", "#C4A1FF", "#A978F5", "#8B5CF6", "#6750A4", "#4A3780",
    
    // Ряд 2: Голубые и циановые
    "#D5F0FF", "#A1D9FF", "#65B8FF", "#3978F6", "#1E5AC7", "#144394",
    
    // Ряд 3: Бирюзовые и зеленые
    "#D1F7F0", "#8CE5D5", "#36D1B6", "#25B99A", "#178C74", "#0D664E",
    
    // Ряд 4: Лаймовые и желто-зеленые
    "#E5F7C4", "#C7EE7F", "#A3D63A", "#55A630", "#3D7A1F", "#285213",
    
    // Ряд 5: Желтые и оранжевые
    "#FFFBCC", "#FFED80", "#FFD633", "#D5A52B", "#B07D15", "#7A5507",
    
    // Ряд 6: Оранжевые и красные
    "#FFE0CC", "#FFB380", "#F08A35", "#D65A1B", "#9E3E10", "#6B2608",
    
    // Ряд 7: Розовые и малиновые
    "#FFD1E0", "#FF99C2", "#E45B75", "#C42847", "#94172F", "#5E0D1B",
    
    // Ряд 8: Пурпурные и неоновые (добавка в стиле твоего скрина)
    "#F2D5FF", "#D994FF", "#B84DFF", "#D94A9B", "#A3246B", "#8B929E"
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
    // Используем FlexLayout для аккуратной сетки
    var flex = new FlexLayout
    {
        Direction = Microsoft.Maui.Layouts.FlexDirection.Row,
        Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
        JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start,
        AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Center
    };

    foreach (var colorHex in colors)
    {
        bool selected = ThemeManager.CurrentAccent == colorHex;

        // Делаем скругленные квадраты (Squarcles)
        var button = new Button
        {
            Text = selected ? "✓" : "",
            WidthRequest = 48,
            HeightRequest = 48,
            CornerRadius = 14, // Скругленный квадрат, а не круг
            Padding = 0,
            BackgroundColor = Color.FromArgb(colorHex),
            TextColor = Colors.White,
            BorderColor = selected ? Colors.White : Colors.Transparent,
            BorderWidth = selected ? 2 : 0,
            Margin = new Thickness(0, 0, 10, 10) // Отступы между квадратами
        };

        button.Clicked += (_, _) =>
        {
            ThemeManager.SetAccent(colorHex);
            BuildContent(); // Перерисовываем страницу, чтобы обновить галочки
        };

        flex.Children.Add(button);
    }

    content.Children.Add(flex);
}

void AddIcons()
{
    var grid = new Grid
    {
        ColumnDefinitions = {
            new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star),
            new ColumnDefinition(GridLength.Star)
        },
        RowSpacing = 10, ColumnSpacing = 10
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

        var stack = new VerticalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.Center };
        var img = new Image { Source = item.Source, WidthRequest = 60, HeightRequest = 60, Aspect = Aspect.AspectFit };
        var label = new Label { Text = item.Name, FontSize = 12, HorizontalTextAlignment = TextAlignment.Center };
        ThemeManager.BindText(label, secondary: !selected);

        stack.Children.Add(img);
        stack.Children.Add(label);

        var border = new Border
        {
            StrokeThickness = selected ? 2 : 1,
            Stroke = selected ? ThemeManager.AccentColor : Colors.Transparent,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            Padding = 8,
            Content = stack
        };
        border.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.SurfaceAlt);

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            ThemeManager.SetIcon(item.Id);
            BuildContent(); // Обновляем UI
            await Application.Current!.MainPage!.DisplayAlert("Иконка изменена", "Чтобы увидеть новую иконку на рабочем столе, закройте приложение и откройте его снова.", "ОК");
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
