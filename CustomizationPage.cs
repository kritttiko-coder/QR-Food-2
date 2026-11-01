
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

    readonly (string Id, string Name, string Symbol)[] icons =
    {
        ("classic", "Обычная", "▦"),
        ("winter", "Зимняя", "❄"),
        ("halloween", "Хэллоуин", "🎃"),
        ("mono", "Чёрно-белая", "◐"),
        ("neon", "Неоновая", "✦")
    };

    public CustomizationPage()
    {
        Title = "Внешний вид";
        BackgroundColor = Color.FromArgb("#111116");

        content.Padding = 20;

        content.Children.Add(new Label
        {
            Text = "Внешний вид",
            FontSize = 27,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        });

        content.Children.Add(new Label
        {
            Text = "Настрой приложение под себя",
            FontSize = 14,
            TextColor = Color.FromArgb("#B7B2C1")
        });

        AddSection("Тема приложения");
        AddThemes();

        AddSection("Акцентный цвет");
        AddColors();

        AddSection("Иконка приложения");
        AddIcons();

        AddPreview();

        Content = new ScrollView { Content = content };
        ThemeManager.Apply();
    }

    void AddSection(string title)
    {
        content.Children.Add(new Label
        {
            Text = title,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            Margin = new Thickness(0, 12, 0, 0)
        });
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
        AddThemeButton(grid, 1, "dark", "Тёмная", "#111116");
        AddThemeButton(grid, 2, "light", "Светлая", "#F3F1F7");

        content.Children.Add(grid);
    }

    void AddThemeButton(
        Grid grid, int column, string id, string title, string color)
    {
        var selected = ThemeManager.CurrentTheme == id;

        var button = new Button
        {
            Text = (selected ? "✓\n" : "") + title,
            BackgroundColor = Color.FromArgb(color),
            TextColor = id == "light" ? Colors.Black : Colors.White,
            BorderColor = selected ? Color.FromArgb(ThemeManager.CurrentAccent) : Color.FromArgb("#48434F"),
            BorderWidth = selected ? 2 : 1,
            CornerRadius = 14,
            HeightRequest = 76,
            FontSize = 13
        };

        button.Clicked += (_, _) =>
        {
            ThemeManager.SetTheme(id);
            RefreshPage();
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

            bool selected = ThemeManager.CurrentAccent == colors[i];

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
                RefreshPage();
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
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 10,
            RowSpacing = 10
        };

        for (int i = 0; i < icons.Length; i++)
        {
            var item = icons[i];
            int row = i / 2;
            int column = i % 2;

            bool selected = ThemeManager.CurrentIcon == item.Id;

            var button = new Button
            {
                Text = $"{item.Symbol}   {item.Name}" +
                       (selected ? "   ✓" : ""),
                HeightRequest = 64,
                CornerRadius = 14,
                BackgroundColor = Color.FromArgb("#201D28"),
                TextColor = Colors.White,
                BorderColor = selected ? Color.FromArgb(ThemeManager.CurrentAccent) : Color.FromArgb("#383242"),
                BorderWidth = selected ? 2 : 1,
                FontSize = 14
            };

            button.Clicked += (_, _) =>
            {
                ThemeManager.SetIcon(item.Id);
                RefreshPage();
            };

            grid.Add(button, column, row);
        }

        content.Children.Add(grid);
    }

    void AddPreview()
    {
        content.Children.Add(new Label
        {
            Text = "Предпросмотр",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            Margin = new Thickness(0, 12, 0, 0)
        });

        var preview = new Border
        {
            BackgroundColor = Color.FromArgb("#201D28"), 
            Stroke = Color.FromArgb(ThemeManager.CurrentAccent),
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = 18
            },
            Padding = 16,
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Label
                    {
                        Text = "Молоко",
                        FontSize = 19,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    new Label
                    {
                        Text = "2.5% • 0,930 л • 2 шт.",
                        FontSize = 13,
                        TextColor = Color.FromArgb("#B7B2C1")
                    },
                    new Label
                    {
                        Text = "6 дней   •   18 дней",
                        FontSize = 14,
                        TextColor = Colors.White
                    }
                }
            }
        };

        content.Children.Add(preview);
    }

    void RefreshPage()
    {
        content.Children.Clear();

        content.Children.Add(new Label
        {
            Text = "Внешний вид",
            FontSize = 27,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        });

        content.Children.Add(new Label
        {
            Text = "Настрой приложение под себя",
            FontSize = 14,
            TextColor = Color.FromArgb("#B7B2C1")
        });

        AddSection("Тема приложения");
        AddThemes();
        AddSection("Акцентный цвет");
        AddColors();
        AddSection("Иконка приложения");
        AddIcons();
        AddPreview();
    }
}

