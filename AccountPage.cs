using Microsoft.Maui.Controls;

namespace FoodExpiryApp;

public class AccountPage : ContentPage
{
    public AccountPage(ProductRepository repo)
    {
        Title = "Аккаунт";
        ThemeManager.BindPage(this);

        var title = new Label { Text = "Аккаунт", FontSize = 28, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        ThemeManager.BindText(title);

        var grid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
        grid.Add(title, 0, 0);
        grid.Add(new BottomNavBar(2, repo), 0, 1);

        // Свайпы
        var swipeLeft = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        swipeLeft.Swiped += (_, _) => BottomNavBar.SwitchToTab(1, repo); // Влево -> Настройки
        grid.GestureRecognizers.Add(swipeLeft);

        var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeRight.Swiped += (_, _) => BottomNavBar.SwitchToTab(3, repo); // Вправо -> Магазин
        grid.GestureRecognizers.Add(swipeRight);

        Content = grid;
    }
}
