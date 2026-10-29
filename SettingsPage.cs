
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class SettingsPage : ContentPage
{
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
            TextColor = Colors.White
        };

        lang.SelectedIndexChanged += (_, _) =>
        {
            if (lang.SelectedIndex >= 0)
                Preferences.Default.Set("app_language", lang.SelectedIndex);
        };

        var customizationButton = new Button
        {
            Text = "🎨  Кастомизация",
            FontSize = 17,
            HeightRequest = 54,
            CornerRadius = 14,
            BackgroundColor = Color.FromArgb("#282431"),
            TextColor = Color.FromArgb("#A978F5")
        };

        customizationButton.Clicked += async (_, _) =>
            await Navigation.PushAsync(new CustomizationPage());

        var dateHeading = new Label
        {
            Text = "Дата и время приложения",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };

        var date = new DatePicker
        {
            Date = Preferences.Default.Get(
                "manual_app_date", DateTime.Today),
            TextColor = Colors.White
        };

        date.DateSelected += (_, e) =>
            Preferences.Default.Set("manual_app_date", e.NewDate);

        var time = new TimePicker
        {
            Time = TimeSpan.FromTicks(
                Preferences.Default.Get(
                    "manual_app_time_ticks",
                    DateTime.Now.TimeOfDay.Ticks)),
            TextColor = Colors.White
        };

        time.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TimePicker.Time))
            {
                Preferences.Default.Set(
                    "manual_app_time_ticks",
                    time.Time.Ticks);
            }
        };

        var timeWarning = new Label
        {
            Text = "⚠ Синхронизация времени пока не подключена",
            FontSize = 13,
            TextColor = Color.FromArgb("#FF962E")
        };

        var support = new Button
        {
            Text = "☕ Купить разработчику чашечку кофе",
            BackgroundColor = Color.FromArgb("#282431"),
            TextColor = Colors.White,
            CornerRadius = 12
        };

        support.Clicked += async (_, _) =>
            await Navigation.PushAsync(new SupportPage());

        var info = new Button
        {
            Text = "Информация",
            BackgroundColor = Color.FromArgb("#282431"),
            TextColor = Colors.White,
            CornerRadius = 12
        };

        info.Clicked += async (_, _) =>
            await Navigation.PushAsync(new InfoPage());

        var version = new Label
        {
            Text = "FoodExpiryApp · версия 0.2.0",
            TextColor = Color.FromArgb("#B7B2C1"),
            FontSize = 12,
            HorizontalTextAlignment = TextAlignment.Center
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 22,
                Spacing = 16,
                Children =
                {
                    heading,
                    lang,
                    customizationButton,
                    dateHeading,
                    date,
                    time,
                    timeWarning,
                    support,
                    info,
                    version
                }
            }
        };
    }
}
