
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class SettingsPage : ContentPage
{
    readonly Color panel = Color.FromArgb("#201D28");
    readonly Color purple = Color.FromArgb("#A978F5");
    readonly Color muted = Color.FromArgb("#B7B2C1");

    public SettingsPage()
    {
        Title = "Настройки";
        BackgroundColor = Color.FromArgb("#111116");

        var heading = new Label
        {
            Text = "Настройки",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };

        var lang = new Picker
        {
            Title = "Язык приложения",
            ItemsSource = new[] { "Русский", "English" },
            SelectedIndex = Preferences.Default.Get("app_language", 0),
            TextColor = Colors.White,
            TitleColor = muted
        };

        lang.SelectedIndexChanged += (_, _) =>
        {
            if (lang.SelectedIndex >= 0)
                Preferences.Default.Set("app_language", lang.SelectedIndex);
        };

        var languagePanel = new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                new Label
                {
                    Text = "Язык",
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 15
                },
                lang
            }
        };

        var appearance = CreateMenuButton(
            "картинка ", "Внешний вид",
            "Тема, акцентный цвет и иконка");

        appearance.Clicked += async (_, _) =>
            await Navigation.PushAsync(new CustomizationPage());

        var dateTime = CreateMenuButton(
            "◷", "Дата и время",
            "Дата, часы и настройки региона");

        dateTime.Clicked += async (_, _) =>
            await Navigation.PushAsync(new DateTimeSettingsPage());

        var info = CreateMenuButton(
            "ⓘ", "Информация",
            "Цвета сроков годности и категорий");

        info.Clicked += async (_, _) =>
            await Navigation.PushAsync(new InfoPage());

        var donation = CreateMenuButton(
            "☕", "Поддержать автора",
            "Помочь развитию FoodExpiryApp");

        donation.Clicked += async (_, _) =>
            await Navigation.PushAsync(new SupportPage());

        var help = CreateMenuButton(
            "?", "Помощь и поддержка",
            "Сообщить о проблеме или ошибке");

        help.Clicked += async (_, _) =>
            await Navigation.PushAsync(new HelpPage());

        var version = new Label
        {
            Text = "FoodExpiryApp · версия 0.2.0",
            TextColor = muted,
            FontSize = 12,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 8, 0, 0)
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 22,
                Spacing = 14,
                Children =
                {
                    heading,
                    subtitle,
                    languagePanel,
                    appearance,
                    dateTime,
                    info,
                    donation,
                    help,
                    version
                }
            }
        };
    }

    Button CreateMenuButton(string icon, string title, string description)
    {
        var button = new Button
        {
            Text = $"{icon}   {title}\n       {description}",
            FontSize = 15,
            HeightRequest = 76,
            CornerRadius = 16,
            BackgroundColor = panel,
            TextColor = Colors.White,
            BorderColor = Color.FromArgb("#383242"),
            BorderWidth = 1,
            HorizontalOptions = LayoutOptions.Center,
            Padding = new Thickness(16, 8)
        };

        return button;
    }
}
