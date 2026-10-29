
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class HelpPage : ContentPage
{
    public HelpPage()
    {
        Title = "Помощь";
        BackgroundColor = Color.FromArgb("#111116");

        var title = new Label
        {
            Text = "Помощь и поддержка",
            FontSize = 25,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };

        var description = new Label
        {
            Text = "Возникла проблема с приложением? Здесь будет доступна информация, которая поможет её решить, и способ связаться с разработчиком.",
            FontSize = 15,
            TextColor = Color.FromArgb("#B7B2C1")
        };

        var note = new Border
        {
            BackgroundColor = Color.FromArgb("#201D28"),
            Stroke = Color.FromArgb("#383242"),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = 16
            },
            Padding = 16,
            Content = new Label
            {
                Text = "Раздел связи с разработчиком пока не подключён.",
                TextColor = Colors.White,
                FontSize = 14
            }
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 22,
                Spacing = 16,
                Children = { title, description, note }
            }
        };
    }
}
