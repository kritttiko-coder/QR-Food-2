using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class InfoPage : ContentPage
{
    public InfoPage()
    {
        Title = "Инфо";
        ThemeManager.BindPage(this);

        var title = new Label
        {
            Text = "Цвета сроков годности",
            FontSize = 26,
            FontAttributes = FontAttributes.Bold
        };
        ThemeManager.BindText(title);

        
        var content = new VerticalStackLayout
        {
            Padding = 22,
            Spacing = 10,
            Children = { title }
        };

        // Сроки годности
        AddColorBlock(content, "Просрочен", "#B00035");
        AddColorBlock(content, "0–3 дня", "#FF414D");
        AddColorBlock(content, "4–7 дней", "#FF962E");
        AddColorBlock(content, "8–14 дней", "#FF962E");
        AddColorBlock(content, "15–21 день", "#E8C928");
        AddColorBlock(content, "22+ дня", "#39C879");

        // Описание фонов категорий
        var desc = new Label
        {
            Text = "Фоны категорий:",
            Margin = new Thickness(0, 10, 0, 0),
            FontAttributes = FontAttributes.Bold
        };
        ThemeManager.BindText(desc, secondary: true);
        content.Children.Add(desc);

        
        AddColorBlock(content, "Молочные", "#FFFFFF4D");
        AddColorBlock(content, "Напитки", "#E0F7FA4D");
        AddColorBlock(content, "Мясо/рыба", "#FFEBEE4D");
        AddColorBlock(content, "Овощи/фрукты", "#E8F5E94D");
        AddColorBlock(content, "Заморозка", "#E3F2FD4D");
        AddColorBlock(content, "Прочие", "#F3E5F54D");

        // Предупреждение
        var warning = new Label
        {
            Text = "Цвет — визуальная подсказка, а не оценка безопасности пищи.",
            TextColor = Color.FromArgb("#FFB65C"),
            Margin = new Thickness(0, 10, 0, 0)
        };
        content.Children.Add(warning);

        Content = new ScrollView { Content = content };
        ThemeManager.Apply();
    }

    private void AddColorBlock(VerticalStackLayout s, string text, string colorHex)
    {
        s.Children.Add(new Border
        {
            BackgroundColor = Color.FromArgb(colorHex),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            Padding = 10,
            Content = new Label
            {
                Text = text,
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold
            }
        });
    }
}
