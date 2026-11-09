using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Behaviors;

namespace FoodExpiryApp;

public class SupportPage : ContentPage
{
    public SupportPage()
    {
        Title = Loc.T("Support");
        ThemeManager.BindPage(this);

        var content = new VerticalStackLayout { Padding = 22, Spacing = 14 };

        // Заголовок с иконкой
        var headerLayout = new HorizontalStackLayout { Spacing = 10, VerticalOptions = LayoutOptions.Center };
        var coffeeIcon = new Image { Source = "icon_coffee.png", WidthRequest = 30, HeightRequest = 30 };
        coffeeIcon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        
        var title = new Label { Text = Loc.T("SupportAuthor"), FontSize = 26, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };
        ThemeManager.BindText(title);
        
        headerLayout.Children.Add(coffeeIcon);
        headerLayout.Children.Add(title);
        content.Children.Add(headerLayout);

        var subtitle = new Label { Text = Loc.T("ThanksSupport") };
        ThemeManager.BindText(subtitle, secondary: true);
        content.Children.Add(subtitle);

        AddSupportButton(content, "icon_coffee.png", Loc.T("Coffee"));
        AddSupportButton(content, "icon_cookie.png", Loc.T("Cookie")); 
        AddSupportButton(content, "icon_crown.png", Loc.T("GoldCookie")); 

        content.Children.Add(new Label
        {
            Text = Loc.T("PaymentNote"),
            TextColor = Color.FromArgb("#FFB65C"),
            Margin = new Thickness(0, 10, 0, 0)
        });

        Content = new ScrollView { Content = content };
        ThemeManager.Apply();
    }

    private void AddSupportButton(VerticalStackLayout layout, string iconSource, string text)
    {
        var icon = new Image { Source = iconSource, WidthRequest = 24, HeightRequest = 24 };
        icon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });

        var label = new Label { Text = text, FontSize = 16, VerticalOptions = LayoutOptions.Center };
        ThemeManager.BindText(label);

        var hLayout = new HorizontalStackLayout { Spacing = 12, VerticalOptions = LayoutOptions.Center, Children = { icon, label } };

        var button = new Border
        {
            HeightRequest = 56,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Content = hLayout,
            Padding = new Thickness(16, 0)
        };
        
        button.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.SurfaceAlt);
        
        var tap = new TapGestureRecognizer();
        // Переводим и текст всплывающего окна, и кнопку "ОК"
        tap.Tapped += async (_, _) => await DisplayAlert(Loc.T("Support"), Loc.T("PaymentNotConnected"), Loc.T("OK"));
        button.GestureRecognizers.Add(tap);
        
        layout.Children.Add(button);
    }
}
