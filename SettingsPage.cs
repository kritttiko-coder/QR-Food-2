using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace FoodExpiryApp;

public class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        Title = "Настройки";
        ThemeManager.BindPage(this); // Привязка фона страницы

        var heading = new Label
        {
            Text = "Настройки",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold
        };
        ThemeManager.BindText(heading);

        var subtitle = new Label
        {
            Text = "Персонализируйте и изменяйте приложение под себя",
            FontSize = 13
        };
        ThemeManager.BindText(subtitle, secondary: true);

        // Кнопка выбора языка (вместо Picker)
        var langButton = CreateMenuButton("🌐", "Язык", "Выберите язык интерфейса");
        langButton.Clicked += async (_, _) =>
        {
            var current = Preferences.Default.Get("app_language", 0) == 0 ? "Русский" : "English";
            var action = await DisplayActionSheet("Выберите язык", "Отмена", null, "Русский", "English");
            
            if (action == "Русский") Preferences.Default.Set("app_language", 0);
            else if (action == "English") Preferences.Default.Set("app_language", 1);
        };

        var appearance = CreateMenuButton("🎨", "Внешний вид", "Тема, акцентный цвет и иконка");
        appearance.Clicked += async (_, _) => await Navigation.PushAsync(new CustomizationPage());

        var dateTime = CreateMenuButton("◷", "Дата и время", "Дата, часы и настройки региона");
        dateTime.Clicked += async (_, _) => await Navigation.PushAsync(new DateTimeSettingsPage());

        var info = CreateMenuButton("ⓘ", "Информация", "Цвета сроков годности и категорий");
        info.Clicked += async (_, _) => await Navigation.PushAsync(new InfoPage());

        var donation = CreateMenuButton("☕", "Поддержать автора", "Помочь развитию FoodExpiryApp");
        donation.Clicked += async (_, _) => await Navigation.PushAsync(new SupportPage());

        var help = CreateMenuButton("?", "Помощь и поддержка", "Сообщить о проблеме или ошибке");
        help.Clicked += async (_, _) => await Navigation.PushAsync(new HelpPage());

        var version = new Label
        {
            Text = "FoodExpiryApp · версия 0.2.0",
            FontSize = 12,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 8, 0, 0)
        };
        ThemeManager.BindText(version, secondary: true);

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 22,
                Spacing = 14,
                Children = { heading, subtitle, langButton, appearance, dateTime, info, donation, help, version }
            }
        };
        
        ThemeManager.Apply();
    }

    private Button CreateMenuButton(string icon, string title, string description)
    {
        var button = new Button
        {
            Text = $"{icon}   {title}\n       {description}",
            FontSize = 15,
            HeightRequest = 76,
            CornerRadius = 16,
            BorderWidth = 1,
            HorizontalOptions = LayoutOptions.Fill,
            Padding = new Thickness(16, 8)
        };

        // Динамические цвета
        button.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        button.SetDynamicResource(Button.TextColorProperty, ThemeManager.Text);
        button.SetDynamicResource(Button.BorderColorProperty, ThemeManager.Border);

        return button;
    }
}
