using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class SupportPage : ContentPage
{
    public SupportPage()
    {
        Title = "Поддержка";
        ThemeManager.BindPage(this);

        var content = new VerticalStackLayout { Padding = 22, Spacing = 14 };

        var title = new Label { Text = "☕ Поддержать разработчика", FontSize = 26, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(title);
        content.Children.Add(title);

        var subtitle = new Label { Text = "Спасибо, что помогаешь развивать FoodExpiryApp ❤️" };
        ThemeManager.BindText(subtitle, secondary: true);
        content.Children.Add(subtitle);

        AddSupportButton(content, "☕ На чашку кофе — 70 ₽");
        AddSupportButton(content, "🍪 Пачку печенек — 120 ₽");
        AddSupportButton(content, "👑 Золотые печеньки — 500 ₽");

        content.Children.Add(new Label
        {
            Text = "Оплата пока не подключена. Для реальных пожертвований потребуется платёжный сервис или официальная ссылка.",
            TextColor = Color.FromArgb("#FFB65C"),
            Margin = new Thickness(0, 10, 0, 0)
        });

        Content = new ScrollView { Content = content };
        ThemeManager.Apply();
    }

    private void AddSupportButton(VerticalStackLayout layout, string text)
    {
        var b = new Button
        {
            Text = text,
            HeightRequest = 56,
            CornerRadius = 14
        };
        b.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.SurfaceAlt);
        b.SetDynamicResource(Button.TextColorProperty, ThemeManager.Text);
        b.Clicked += async (_, _) => await DisplayAlert("Поддержка", "Интерфейс готов, реальная оплата пока не подключена.", "ОК");
        
        layout.Children.Add(b);
    }
}
