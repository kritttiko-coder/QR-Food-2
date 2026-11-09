using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Behaviors;

namespace FoodExpiryApp;

public class BottomNavBar : ContentView
{
    public BottomNavBar(int currentTab, ProductRepository repo)
    {
        var navGrid = new Grid
        {
            ColumnDefinitions = { 
                new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), 
                new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) 
            },
            Padding = new Thickness(10, 8, 10, 16)
        };

        // 4 кнопки: Меню, Магазин, Аккаунт, Настройки
        string[] icons = { "icon_menu.png", "icon_store.png", "icon_account.png", "icon_gear.png" };

        for (int i = 0; i < 4; i++)
        {
            var img = new Image { Source = icons[i], WidthRequest = 26, HeightRequest = 26, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            img.Behaviors.Add(new IconTintColorBehavior { TintColor = Colors.White });

            var border = new Border
            {
                StrokeThickness = i == currentTab ? 2 : 0,
                Stroke = i == currentTab ? ThemeManager.AccentColor : Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                WidthRequest = 48, HeightRequest = 48, Padding = 0,
                HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center,
                Content = img
            };

            int tabIndex = i; 
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => 
            {
                if (tabIndex == currentTab) return; // Уже здесь
                SwitchToTab(tabIndex, repo); // Вызываем наш новый метод
            };
            border.GestureRecognizers.Add(tap);

            navGrid.Add(border, i, 0);
        }

        var bgColor = (Color)Application.Current!.Resources[ThemeManager.SurfaceAlt];
        var transparentBg = Color.FromRgba(bgColor.Red, bgColor.Green, bgColor.Blue, 0.67f);

        Content = new Border
        {
            BackgroundColor = transparentBg,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24, 24, 0, 0) },
            Content = navGrid
        };
    }

    // МЕТОД ДЛЯ ПЕРЕКЛЮЧЕНИЯ 
    public static void SwitchToTab(int tabIndex, ProductRepository repo)
    {
        Page nextPage = tabIndex switch
        {
            0 => new MainPage(repo),
            1 => new MainPage(repo), // Заглушка для магазина
            2 => new MainPage(repo), // Заглушка для аккаунта
            3 => new SettingsPage(repo), // Настройки
            _ => new MainPage(repo)
        };
        
        Application.Current!.MainPage = new NavigationPage(nextPage);
    }
}
