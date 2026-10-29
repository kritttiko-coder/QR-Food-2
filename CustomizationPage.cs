using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class CustomizationPage : ContentPage
{
    readonly VerticalStackLayout colorButtons = new() { Spacing = 8 };
    readonly Label accentPreview = new()
    {
        Text = "Пример акцентного текста",
        FontSize = 20,
        FontAttributes = FontAttributes.Bold
    };

    readonly Label shadeLabel = new() { FontSize = 14 };
    int selectedColor;
    int selectedShade;
    string selectedTheme = "dark";

    readonly string[] colorNames =
    {
        "Фиолетовый", "Синий", "Голубой", "Бирюзовый",
        "Зелёный", "Лаймовый", "Жёлтый", "Оранжевый",
        "Красный", "Розовый", "Малиновый", "Серый"
    };

    readonly Color[] baseColors =
    {
        Color.FromArgb("#A978F5"),
        Color.FromArgb("#4169E1"),
        Color.FromArgb("#25A9E0"),
        Color.FromArgb("#20BFA9"),
        Color.FromArgb("#39B86B"),
        Color.FromArgb("#A5C928"),
        Color.FromArgb("#E8C928"),
        Color.FromArgb("#FF962E"),
        Color.FromArgb("#FF414D"),
        Color.FromArgb("#F078B8"),
        Color.FromArgb("#C52D72"),
        Color.FromArgb("#A0A0AE")
    };

    public CustomizationPage()
    {
        Title = "Кастомизация";
        BackgroundColor = Color.FromArgb("#111116");

        selectedColor = Preferences.Default.Get("accent_color", 0);
        selectedShade = Preferences.Default.Get("accent_shade", 5);
        selectedTheme = Preferences.Default.Get("app_theme", "dark");

        var heading = new Label
        {
            Text = "Внешний вид",
            FontSize = 26,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };

        var themeTitle = SectionTitle("Тема");
        var themes = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 8
        };

        AddThemeButton(themes, "Чёрная", "black", 0);
        AddThemeButton(themes, "Тёмная", "dark", 1);
        AddThemeButton(themes, "Светлая", "light", 2);

        var accentTitle = SectionTitle("Акцентный цвет");
        var colorGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            RowSpacing = 8,
            ColumnSpacing = 8
        };

        for (int i = 0; i < baseColors.Length; i++)
        {
            int index = i;
            var button = new Button
            {
                Text = colorNames[i],
                FontSize = 11,
                Padding = 4,
                BackgroundColor = baseColors[i],
                TextColor = GetReadableText(baseColors[i]),
                CornerRadius = 10
            };

            button.Clicked += (_, _) =>
            {
                selectedColor = index;
                SaveAccent();
                RefreshAccentControls();
            };

            colorGrid.Add(button, i % 4, i / 4);
        }

        var shadeTitle = SectionTitle("Оттенок");
        var shadePicker = new Picker
        {
            Title = "Выбери оттенок",
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#202027"),
            ItemsSource = Enumerable.Range(1, 10)
                .Select(i => $"{i} — {ShadeName(i)}")
                .ToList(),
            SelectedIndex = selectedShade - 1
        };

        shadePicker.SelectedIndexChanged += (_, _) =>
        {
            if (shadePicker.SelectedIndex < 0)
                return;

            selectedShade = shadePicker.SelectedIndex + 1;
            SaveAccent();
            RefreshAccentControls();
        };

        var previewBox = new Border
        {
            BackgroundColor = Color.FromArgb("#202027"),
            Stroke = Color.FromArgb("#3A3744"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = 16
            },
            Padding = 16,
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new Label
                    {
                        Text = "Предпросмотр",
                        TextColor = Colors.White,
                        FontSize = 15
                    },
                    accentPreview,
                    new Button
                    {
                        Text = "Пример кнопки",
                        BackgroundColor = GetAccent(),
                        TextColor = Colors.White
                    }
                }
            }
        };

        var iconTitle = SectionTitle("Иконка приложения");
        var iconPicker = new Picker
        {
            Title = "Выбери оформление",
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#202027"),
            ItemsSource = new[]
            {
                "Базовая — QR + фиолетовый",
                "Новогодняя",
                "Хэллоуин",
                "Чёрно-белая",
                "Неоновая"
            },
            SelectedIndex = Preferences.Default.Get("app_icon", 0)
        };

        iconPicker.SelectedIndexChanged += (_, _) =>
        {
            if (iconPicker.SelectedIndex >= 0)
                Preferences.Default.Set("app_icon", iconPicker.SelectedIndex);
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 18,
                Spacing = 16,
                Children =
                {
                    heading,
                    themeTitle,
                    themes,
                    accentTitle,
                    colorGrid,
                    shadeTitle,
                    shadePicker,
                    shadeLabel,
                    previewBox,
                    iconTitle,
                    iconPicker
                }
            }
        };

        RefreshAccentControls();
        ApplyTheme();
    }

    static Label SectionTitle(string text) => new()
    {
        Text = text,
        FontSize = 18,
        FontAttributes = FontAttributes.Bold,
        TextColor = Colors.White
    };

    void AddThemeButton(Grid grid, string text, string theme, int column)
    {
        var button = new Button
        {
            Text = text,
            FontSize = 13,
            Padding = 5,
            BackgroundColor = theme switch
            {
                "black" => Colors.Black,
                "light" => Color.FromArgb("#EEEEF4"),
                _ => Color.FromArgb("#292832")
            },
            TextColor = theme == "light" ? Colors.Black : Colors.White
        };

        button.Clicked += (_, _) =>
        {
            selectedTheme = theme;
            Preferences.Default.Set("app_theme", theme);
            ApplyTheme();
        };

        grid.Add(button, column);
    }

    void ApplyTheme()
    {
        Application.Current!.UserAppTheme = selectedTheme == "light"
            ? AppTheme.Light
            : AppTheme.Dark;

        BackgroundColor = selectedTheme == "black"
            ? Colors.Black
            : selectedTheme == "light"
                ? Color.FromArgb("#F4F3F8")
                : Color.FromArgb("#111116");
    }

    void SaveAccent()
    {
        Preferences.Default.Set("accent_color", selectedColor);
        Preferences.Default.Set("accent_shade", selectedShade);
    }

    void RefreshAccentControls()
    {
        var color = GetAccent();
        accentPreview.TextColor = color;
        shadeLabel.Text =
            $"{colorNames[selectedColor]} · оттенок {selectedShade}/10 · {color.ToArgbHex()}";
    }

    Color GetAccent()
    {
        var baseColor = baseColors[selectedColor];
        double amount = (selectedShade - 5) / 5.0;

        if (amount < 0)
            return Mix(baseColor, Colors.White, -amount * 0.75);

        return Mix(baseColor, Colors.Black, amount * 0.65);
    }

    static Color Mix(Color from, Color to, double amount)
    {
        amount = Math.Clamp(amount, 0, 1);

        return Color.FromRgb(
            (float)(from.Red + (to.Red - from.Red) * amount),
            (float)(from.Green + (to.Green - from.Green) * amount),
            (float)(from.Blue + (to.Blue - from.Blue) * amount));
    }

    static Color GetReadableText(Color background)
    {
        double brightness =
            background.Red * 0.299 +
            background.Green * 0.587 +
            background.Blue * 0.114;

        return brightness > 0.65 ? Colors.Black : Colors.White;
    }

    static string ShadeName(int shade) => shade switch
    {
        1 => "очень светлый",
        2 => "светлый",
        3 => "светлее среднего",
        4 => "слегка светлый",
        5 => "основной",
        6 => "слегка тёмный",
        7 => "темнее среднего",
        8 => "тёмный",
        9 => "очень тёмный",
        _ => "максимально тёмный"
    };
}
