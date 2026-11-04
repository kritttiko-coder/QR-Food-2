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
        ThemeManager.BindPage(this);

        _datePicker = new DatePicker { Date = AppClock.Now.Date };
        _datePicker.SetDynamicResource(DatePicker.TextColorProperty, ThemeManager.Text);

        _timePicker = new TimePicker { Time = AppClock.Now.TimeOfDay };
        _timePicker.SetDynamicResource(TimePicker.TextColorProperty, ThemeManager.Text);

        _manualSwitch = new Switch { IsToggled = AppClock.IsManual, OnColor = ThemeManager.AccentColor, ThumbColor = Colors.White };

        _warningLabel = new Label
        {
            Text = "⚠ Используется ручное время. Автоматическая синхронизация отключена.",
            TextColor = Colors.Orange,
            FontSize = 13,
            IsVisible = AppClock.IsManual
        };

        _regionPicker = new Picker { Title = "Выберите регион" };
        _regionPicker.SetDynamicResource(Picker.TextColorProperty, ThemeManager.Text);
        _regionPicker.ItemsSource = new List<string> { "Москва", "Калининград", "Екатеринбург", "Новосибирск", "Владивосток", "Чита", "Лондон", "Амстердам" };
        _regionPicker.SelectedItem = Preferences.Get("AppRegion", "Чита");
        _manualSwitch.Toggled += OnManualSwitchChanged;

        var title = new Label { Text = "Настройка времени", FontSize = 22, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(title);

        var description = new Label { Text = "Выберите автоматическое системное время или задайте дату и время вручную.", FontSize = 14 };
        ThemeManager.BindText(description, secondary: true);

        var manualRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        var manualLabel = new Label { Text = "Ручная настройка", VerticalOptions = LayoutOptions.Center };
        ThemeManager.BindText(manualLabel);
        
        manualRow.Add(manualLabel, 0, 0);
        manualRow.Add(_manualSwitch, 1, 0);

        var saveButton = new Button { Text = "Сохранить", CornerRadius = 14, HeightRequest = 50 };
        saveButton.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Accent);
        saveButton.SetDynamicResource(Button.TextColorProperty, ThemeManager.AccentText);
        saveButton.Clicked += OnSaveClicked;

        var content = new VerticalStackLayout
        {
            Padding = new Thickness(20),
            Spacing = 18,
            Children =
            {
                title, description, manualRow, _warningLabel,
                CreateLabel("Дата"), _datePicker,
                CreateLabel("Время"), _timePicker,
                CreateLabel("Регион"), _regionPicker,
                saveButton
            }
        };

        Content = new ScrollView { Content = content };
        UpdateControls();
        ThemeManager.Apply();
    }

    private Label CreateLabel(string text)
    {
        var l = new Label { Text = text, FontAttributes = FontAttributes.Bold, FontSize = 15 };
        ThemeManager.BindText(l);
        return l;
    }

    private void OnManualSwitchChanged(object? sender, ToggledEventArgs e)
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

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_manualSwitch.IsToggled)
            AppClock.SetManualDateTime(_datePicker.Date.Date.Add(_timePicker.Time));
        else
            AppClock.UseSystemTime();

        if (_regionPicker.SelectedItem is string region)
            Preferences.Set("AppRegion", region);

        await DisplayAlert("Сохранено", "Настройки даты и времени сохранены.", "ОК");
        await Navigation.PopAsync();
    }
}
