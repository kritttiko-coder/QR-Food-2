using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Behaviors;

namespace FoodExpiryApp;

public class SettingsPage : ContentPage
{
    public SettingsPage(ProductRepository repo) 
    {
        Title = Loc.T("Settings");
        ThemeManager.BindPage(this);

        var heading = new Label { Text = Loc.T("Settings"), FontSize = 28, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(heading);
        var subtitle = new Label { Text = Loc.T("Personalize"), FontSize = 13 };
        ThemeManager.BindText(subtitle, secondary: true);

        var langButton = CreateMenuButton("icon_lang.png", Loc.T("Language"), Loc.T("ChooseLanguage"));
        langButton.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Navigation.PushAsync(new LanguagesPage())) });

        var appearance = CreateMenuButton("icon_brush.png", Loc.T("Appearance"), Loc.T("AppearanceDesc"));
        appearance.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Navigation.PushAsync(new CustomizationPage())) });

        var dateTime = CreateMenuButton("icon_clock.png", Loc.T("DateTime"), Loc.T("DateTimeDesc"));
        dateTime.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Navigation.PushAsync(new DateTimeSettingsPage())) });

        var info = CreateMenuButton("icon_info.png", Loc.T("Info"), Loc.T("InfoDesc"));
        info.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Navigation.PushAsync(new InfoPage())) });

        var donation = CreateMenuButton("icon_coffee.png", Loc.T("Support"), Loc.T("SupportDesc"));
        donation.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Navigation.PushAsync(new SupportPage())) });

        var help = CreateMenuButton("icon_help.png", Loc.T("Help"), Loc.T("HelpDesc"));
        help.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Navigation.PushAsync(new HelpPage())) });

        var version = new Label { Text = $"FoodExpiryApp · {Loc.T("Version")}", FontSize = 12, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 8, 0, 0) };
        ThemeManager.BindText(version, secondary: true);

        var scrollContent = new ScrollView { Content = new VerticalStackLayout { Padding = 22, Spacing = 14, Children = { heading, subtitle, langButton, appearance, dateTime, info, donation, help, version } } };

        var mainGrid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
        mainGrid.Add(scrollContent, 0, 0);
        mainGrid.Add(new BottomNavBar(3, repo), 0, 1);

        // СВАЙПЫ НА ВСЮ СТРАНИЦУ (Влево - на Главную)
        var swipeLeft = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        swipeLeft.Swiped += (_, _) => BottomNavBar.SwitchToTab(3, repo);
        mainGrid.GestureRecognizers.Add(swipeLeft);

        var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeRight.Swiped += (_, _) => BottomNavBar.SwitchToTab(0, repo);
        mainGrid.GestureRecognizers.Add(swipeRight);

        Content = mainGrid;
        ThemeManager.Apply();
    }

    private Border CreateMenuButton(string iconSource, string title, string description)
    {
        var icon = new Image { Source = iconSource, WidthRequest = 24, HeightRequest = 24, VerticalOptions = LayoutOptions.Center };
        icon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });

        var titleLabel = new Label { Text = title, FontSize = 16, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(titleLabel);
        var descLabel = new Label { Text = description, FontSize = 12 };
        ThemeManager.BindText(descLabel, secondary: true);

        var textStack = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { titleLabel, descLabel } };
        var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 16, Padding = new Thickness(16, 12) };
        grid.Add(icon, 0, 0); grid.Add(textStack, 1, 0);

        var border = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Content = grid };
        border.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        border.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);

        return border;
    }
}
