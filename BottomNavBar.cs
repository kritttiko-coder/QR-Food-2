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
                new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star) 
            },
            Padding = new Thickness(0, 8, 0, 16)
        };

        // 0-Главная, 1-Магазин, 3-Статистика, 4-Аккаунт
        // 0-Главная, 1-Магазин, 3-Статистика, 4-Аккаунт
string[] icons = { "icon_home.png", "icon_shop_bag.png", "icon_chart_bar.png", "icon_user.png" };
string[] labels = { Loc.T("Home"), Loc.T("Store"), Loc.T("Statistics"), Loc.T("Account") };

// Внутри цикла, где создается QR (i == 2):
var qrImg = new Image { Source = "icon_camera.png", WidthRequest = 32, HeightRequest = 32 };
qrImg.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
qrBorder.Content = qrImg;
        for (int i = 0; i < 5; i++)
        {
            if (i == 2) // Центральная кнопка QR
            {
                var qrBorder = new Border
                {
                    WidthRequest = 64, HeightRequest = 64,
                    StrokeShape = new RoundRectangle { CornerRadius = 32 },
                    StrokeThickness = 4, Stroke = ThemeManager.AccentColor,
                    BackgroundColor = Color.FromArgb("#1C1C22"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, -20, 0, 0), // Поднимаем вверх
                    ZIndex = 10
                };
                
                var tapQr = new TapGestureRecognizer();
                tapQr.Tapped += async (_, _) => await Application.Current!.MainPage!.Navigation.PushAsync(new ScanPage(repo));
                qrBorder.GestureRecognizers.Add(tapQr);

                navGrid.Add(qrBorder, 2, 0);
            }
            else
            {
                int tabIndex = i > 2 ? i + 1 : i; // Сдвиг индекса из-за QR
                bool isSelected = tabIndex == currentTab;
                Color color = isSelected ? ThemeManager.AccentColor : Color.FromArgb("#888888");

                var stack = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
                
                var img = new Image { Source = icons[i], WidthRequest = 24, HeightRequest = 24 };
                img.Behaviors.Add(new IconTintColorBehavior { TintColor = color });

                var lbl = new Label { Text = labels[i], FontSize = 10, TextColor = color };

                stack.Children.Add(img);
                stack.Children.Add(lbl);

                var border = new Border
                {
                    StrokeThickness = 0, BackgroundColor = Colors.Transparent,
                    Padding = new Thickness(10, 8),
                    Content = stack
                };

                var tap = new TapGestureRecognizer();
                tap.Tapped += (_, _) => 
                {
                    if (tabIndex == currentTab) return;
                    SwitchToTab(tabIndex, repo); 
                };
                border.GestureRecognizers.Add(tap);

                navGrid.Add(border, i, 0);
            }
        }

        var bgColor = (Color)Application.Current!.Resources[ThemeManager.SurfaceAlt];
        var transparentBg = Color.FromRgba(bgColor.Red, bgColor.Green, bgColor.Blue, 0.67f);

        Content = new Border
        {
            BackgroundColor = transparentBg,
            StrokeThickness = 1, Stroke = Color.FromRgba(255,255,255,0.1),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24, 24, 0, 0) },
            Content = navGrid
        };
    }

    public static void SwitchToTab(int tabIndex, ProductRepository repo)
    {
        Page nextPage = tabIndex switch
        {
            0 => new MainPage(repo),
            1 => new StorePage(repo),
            3 => new StatisticsPage(repo),
            4 => new AccountPage(repo),
            _ => new MainPage(repo)
        };
        Application.Current!.MainPage = new NavigationPage(nextPage);
    }
}
