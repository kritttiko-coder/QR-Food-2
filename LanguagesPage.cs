using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class LanguagesPage : ContentPage
{
    public LanguagesPage()
    {
        Title = Loc.T("Language");
        ThemeManager.BindPage(this);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            RowSpacing = 12,
            ColumnSpacing = 12,
            Padding = 20
        };

        // Первый ряд (Рабочие)
        grid.Add(CreateLangButton("ru", "Русский", false), 0, 0);
        grid.Add(CreateLangButton("en", "English", false), 1, 0);
        grid.Add(CreateLangButton("es", "Español", true), 2, 0);

        // Второй ряд (Бета)
        grid.Add(CreateLangButton("ja", "日本語", true), 0, 1);
        grid.Add(CreateLangButton("zh", "中文", true), 1, 1);
        grid.Add(CreateLangButton("de", "Deutsch", true), 2, 1);

        Content = new ScrollView { Content = grid };
        ThemeManager.Apply();
    }

    private Border CreateLangButton(string code, string name, bool isBeta)
    {
        bool isSelected = LocalizationManager.Instance.CurrentLanguageCode == code;

        var titleLabel = new Label
        {
            Text = name,
            FontSize = 16,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        ThemeManager.BindText(titleLabel);

        var stack = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Spacing = 4,
            Children = { titleLabel }
        };

        // Если язык в бете, добавляем плашку
        if (isBeta)
        {
            var betaLabel = new Label
            {
                Text = Loc.T("Beta"),
                FontSize = 10,
                TextColor = Color.FromArgb("#FFB65C"),
                HorizontalTextAlignment = TextAlignment.Center
            };
            stack.Children.Add(betaLabel);
            
            // Бета-языки делаем полупрозрачными
            titleLabel.TextColor = Color.FromArgb("#777777"); 
        }

        var border = new Border
        {
            StrokeThickness = isSelected ? 2 : 1,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            HeightRequest = 60, // Чуть выше, чтобы поместилась плашка "Бета"
            Content = stack
        };

        // Настройка цветов
        if (isSelected)
        {
            border.Stroke = ThemeManager.AccentColor;
            border.BackgroundColor = Color.FromArgb("#252030"); // Чуть светлее фона
        }
        else
        {
            border.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
            border.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);
        }

        // Обработка нажатия
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            if (isBeta)
            {
                await DisplayAlert(Loc.T("Language"), $"Язык {name} находится в разработке.", "ОК");
                return;
            }

            if (isSelected) return; // Уже выбран

            // Меняем язык
            LocalizationManager.Instance.SetLanguage(code);
            
            // Перезагружаем страницу, чтобы обновить выделение и тексты
            Application.Current!.MainPage = new NavigationPage(new MainPage(new ProductRepository()))
            {
                BarBackgroundColor = ThemeManager.Theme == "dark" ? Color.FromArgb("#1C1C22") : Colors.White,
                BarTextColor = ThemeManager.Theme == "dark" ? Colors.White : Colors.Black
            };
        };
        border.GestureRecognizers.Add(tap);

        return border;
    }
}
