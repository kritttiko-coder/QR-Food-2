using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class HelpPage : ContentPage
{
    public HelpPage()
    {
        Title = "Помощь";
        ThemeManager.BindPage(this);

        var title = new Label { Text = "Помощь и поддержка", FontSize = 25, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(title);

        var description = new Label { Text = "Возникла проблема с приложением? Здесь будет доступна информация, которая поможет её решить, и способ связаться с разработчиком.", FontSize = 15 };
        ThemeManager.BindText(description, secondary: true);

        var noteLabel = new Label { Text = "Раздел связи с разработчиком пока не подключён.", FontSize = 14 };
        ThemeManager.BindText(noteLabel);

        var note = new Border
        {
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = 16,
            Content = noteLabel
        };
        note.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        note.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 22,
                Spacing = 16,
                Children = { title, description, note }
            }
        };
        ThemeManager.Apply();
    }
}
