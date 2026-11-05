using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Behaviors;

namespace FoodExpiryApp;

public class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        Title = "Настройки";
        ThemeManager.BindPage(this);

        var heading = new Label { Text = "Настройки", FontSize = 28, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(heading);

        var subtitle = new Label { Text = "Персонализируйте и изменяйте приложение под себя", FontSize = 13 };
        ThemeManager.BindText(subtitle, secondary: true);
       
        var langButton = CreateMenuButton("icon_lang.png", Loc.T("Language"), Loc.T("ChooseLanguage"));
        langButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Navigation.PushAsync(new LanguagesPage()))
        });
                        
        var appearance = CreateMenuButton("icon_brush.png", "Внешний вид", "Тема, акцентный цвет и иконка");
        appearance.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Navigation.PushAsync(new CustomizationPage()))
        });

        var dateTime = CreateMenuButton("icon_clock.png", "Дата и время", "Дата, часы и настройки региона");
        dateTime.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Navigation.PushAsync(new DateTimeSettingsPage()))
        });

        var info = CreateMenuButton("icon_info.png", "Информация", "Цвета сроков годности и категорий");
        info.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Navigation.PushAsync(new InfoPage()))
        });

        var donation = CreateMenuButton("icon_coffee.png", "Поддержать автора", "Помочь развитию FoodExpiryApp");
        donation.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Navigation.PushAsync(new SupportPage()))
        });

        var help = CreateMenuButton("icon_help.png", "Помощь и поддержка", "Сообщить о проблеме или ошибке");
        help.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Navigation.PushAsync(new HelpPage()))
        });

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

    private Border CreateMenuButton(string iconSource, string title, string description)
    {
        // Сама иконка с тонированием
        var icon = new Image
        {
            Source = iconSource,
            WidthRequest = 24,
            HeightRequest = 24,
            VerticalOptions = LayoutOptions.Center
        };
        
        // Тонируем в акцентный цвет
        icon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });

        // Тексты
        var titleLabel = new Label { Text = title, FontSize = 16, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(titleLabel);

        var descLabel = new Label { Text = description, FontSize = 12 };
        ThemeManager.BindText(descLabel, secondary: true);

        var textStack = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children = { titleLabel, descLabel }
        };

        // Компоновка: Иконка слева, текст справа
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 16,
            Padding = new Thickness(16, 12)
        };
        grid.Add(icon, 0, 0);
        grid.Add(textStack, 1, 0);

        
        var border = new Border
        {
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Content = grid
        };
        
        border.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        border.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);

        return border;
    }
}
