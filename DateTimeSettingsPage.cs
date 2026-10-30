using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using FoodExpiryApp.Services;

namespace FoodExpiryApp;

public class DateTimeSettingsPage : ContentPage
{
    private readonly DatePicker _datePicker;
    private readonly TimePicker _timePicker;
    private readonly Switch _manualSwitch;
    private readonly Label _warningLabel;
    private readonly Picker _regionPicker;

    public DateTimeSettingsPage()
    {
        Title = "Дата и время";
        BackgroundColor = Color.FromArgb("#101014");

        _datePicker = new DatePicker
        {
            Date = AppClock.Now.Date,
            TextColor = Colors.White
        };

        _timePicker = new TimePicker
        {
            Time = AppClock.Now.TimeOfDay,
            TextColor = Colors.White
        };

        _manualSwitch = new Switch
        {
            IsToggled = AppClock.IsManual,
            OnColor = Color.FromArgb("#A978F5"),
            ThumbColor = Colors.White
        };

        _warningLabel = new Label
        {
            Text = "⚠ Используется ручное время. Автоматическая синхронизация отключена.",
            TextColor = Colors.Orange,
            FontSize = 13,
            IsVisible = AppClock.IsManual
        };

        _regionPicker = new Picker
        {
            Title = "Выберите регион",
            TextColor = Colors.White,
            ItemsSource = new List<string>
            {
                "Москва",
                "Калининград",
                "Екатеринбург",
                "Новосибирск",
                "Владивосток",
                "Чита",
                "Лондон",
                "Амстердам"
            }
        };

        string savedRegion = Preferences.Get("AppRegion", "Чита");
        _regionPicker.SelectedItem = savedRegion;

        _manualSwitch.Toggled += OnManualSwitchChanged;

        var title = new Label
        {
            Text = "Настройка времени",
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };

        var description = new Label
        {
            Text = "Выберите автоматическое системное время или задайте дату и время вручную.",
            FontSize = 14,
            TextColor = Color.FromArgb("#AAAAAA")
        };

        var manualRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };

        var manualLabel = new Label
        {
            Text = "Ручная настройка",
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center
        };

        manualRow.Add(manualLabel, 0, 0);
        manualRow.Add(_manualSwitch, 1, 0);

        var dateLabel = CreateLabel("Дата");
        var timeLabel = CreateLabel("Время");
        var regionLabel = CreateLabel("Регион");

        var saveButton = new Button
        {
            Text = "Сохранить",
            BackgroundColor = Color.FromArgb("#A978F5"),
            TextColor = Colors.White,
            CornerRadius = 14,
            HeightRequest = 50
        };

        saveButton.Clicked += OnSaveClicked;

        var content = new VerticalStackLayout
        {
            Padding = new Thickness(20),
            Spacing = 18,
            Children =
            {
                title,
                description,
                manualRow,
                _warningLabel,
                dateLabel,
                _datePicker,
                timeLabel,
                _timePicker,
                regionLabel,
                _regionPicker,
                saveButton
            }
        };

        Content = new ScrollView
        {
            Content = content
        };

        UpdateControls();
    }

    private static Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            FontSize = 15
        };
    }

    private void OnManualSwitchChanged(
        object? sender,
        ToggledEventArgs e)
    {
        _warningLabel.IsVisible = e.Value;
        UpdateControls();
    }

    private void UpdateControls()
    {
        bool manual = _manualSwitch.IsToggled;

        _datePicker.IsEnabled = manual;
        _timePicker.IsEnabled = manual;
    }

    private async void OnSaveClicked(
        object? sender,
        EventArgs e)
    {
        if (_manualSwitch.IsToggled)
        {
            DateTime selectedDateTime = _datePicker.Date
                .Date
                .Add(_timePicker.Time);

            AppClock.SetManualDateTime(selectedDateTime);
        }
        else
        {
            AppClock.UseSystemTime();
        }

        if (_regionPicker.SelectedItem is string region)
        {
            Preferences.Set("AppRegion", region);
        }

        await DisplayAlert(
            "Сохранено",
            "Настройки даты и времени сохранены.",
            "ОК"
        );

        await Navigation.PopAsync();
    }
}
