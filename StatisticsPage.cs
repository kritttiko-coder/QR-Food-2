using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Behaviors;

namespace FoodExpiryApp;

public class StatisticsPage : ContentPage
{
    public StatisticsPage(ProductRepository repo)
    {
        Title = Loc.T("Statistics");
        ThemeManager.BindPage(this);

        var content = new VerticalStackLayout { Padding = 16, Spacing = 12 };

        // Шапка
        var header = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        var title = new Label { Text = Loc.T("Statistics"), FontSize = 28, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(title);
        var cal = new Image { Source = "icon_calendar.png", WidthRequest = 24, HeightRequest = 24 };
        cal.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        header.Add(title, 0, 0); header.Add(cal, 1, 0);
        content.Children.Add(header);

        // Табы
        var tabsGrid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        tabsGrid.Add(CreateTab(Loc.T("Week"), true), 0, 0);
        tabsGrid.Add(CreateTab(Loc.T("Month"), false), 1, 0);
        tabsGrid.Add(CreateTab(Loc.T("Year"), false), 2, 0);
        content.Children.Add(tabsGrid);

        // Верхние 4 карточки
        var topCards = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8 };
        topCards.Add(CreateStatCard("icon_cube.png", "24", Loc.T("ProductsCount")), 0, 0);
        topCards.Add(CreateStatCard("icon_clock.png", "12", Loc.T("AvgShelfLife")), 1, 0);
        topCards.Add(CreateStatCard("icon_check.png", "89%", Loc.T("OnTime")), 2, 0);
        topCards.Add(CreateStatCard("icon_trash.png", "2", Loc.T("Expired"), true), 3, 0);
        content.Children.Add(topCards);

        // График (Заглушка)
        var chartCard = CreateCard();
        var chartStack = new VerticalStackLayout { Spacing = 10 };
        chartStack.Children.Add(new Label { Text = Loc.T("ProductChanges"), FontSize = 14, FontAttributes = FontAttributes.Bold });
        var chartPlaceholder = new BoxView { HeightRequest = 120, BackgroundColor = Color.FromArgb("#2A2A35"), CornerRadius = 8 };
        chartStack.Children.Add(chartPlaceholder);
        chartCard.Content = chartStack;
        content.Children.Add(chartCard);

        // Категории
        var catCard = CreateCard();
        var catStack = new VerticalStackLayout { Spacing = 10 };
        catStack.Children.Add(new Label { Text = Loc.T("Categories"), FontSize = 14, FontAttributes = FontAttributes.Bold });
        var catPlaceholder = new BoxView { HeightRequest = 100, BackgroundColor = Color.FromArgb("#2A2A35"), CornerRadius = 8 };
        catStack.Children.Add(catPlaceholder);
        catCard.Content = catStack;
        content.Children.Add(catCard);

        // Оценка стоимости
        var costCard = CreateCard();
        var costStack = new VerticalStackLayout { Spacing = 8 };
        var costHeader = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        costHeader.Add(new Label { Text = Loc.T("CostEstimation"), FontSize = 14, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center }, 0, 0);
        var sw = new Switch { IsToggled = true, OnColor = ThemeManager.AccentColor };
        costHeader.Add(sw, 1, 0);
        costStack.Children.Add(costHeader);

        var priceStack = new HorizontalStackLayout { Spacing = 8 };
        var priceIcon = new Image { Source = "icon_money.png", WidthRequest = 24, HeightRequest = 24 };
        priceIcon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var priceText = new Label { Text = "≈ 2 480 ₽", FontSize = 22, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };
        ThemeManager.BindText(priceText);
        priceStack.Children.Add(priceIcon); priceStack.Children.Add(priceText);
        costStack.Children.Add(priceStack);

        var warn = new Label { Text = Loc.T("CostWarning"), FontSize = 11, TextColor = Colors.Gray };
        costStack.Children.Add(warn);
        costCard.Content = costStack;
        content.Children.Add(costCard);

        // Ближайшие сроки
        var nearCard = CreateCard();
        var nearStack = new VerticalStackLayout { Spacing = 10 };
        nearStack.Children.Add(new Label { Text = Loc.T("NearestExpirations"), FontSize = 14, FontAttributes = FontAttributes.Bold });
        nearStack.Children.Add(CreateExpiryRow("🥛", "Молоко", "6 дней", Color.FromArgb("#B00035")));
        nearStack.Children.Add(CreateExpiryRow("🧀", "Сыр", "12 дней", Color.FromArgb("#FF962E")));
        nearStack.Children.Add(CreateExpiryRow("🍎", "Яблоки", "18 дней", Color.FromArgb("#E8C928")));
        nearCard.Content = nearStack;
        content.Children.Add(nearCard);

        var scroll = new ScrollView { Content = content };
        
        var rootGrid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
        rootGrid.Add(scroll, 0, 0);
        rootGrid.Add(new BottomNavBar(3, repo), 0, 1);

        // Свайпы
        var swipeL = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        swipeL.Swiped += (_, _) => BottomNavBar.SwitchToTab(4, repo);
        rootGrid.GestureRecognizers.Add(swipeL);
        var swipeR = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeR.Swiped += (_, _) => BottomNavBar.SwitchToTab(1, repo);
        rootGrid.GestureRecognizers.Add(swipeR);

        Content = rootGrid;
        ThemeManager.Apply();
    }

    private Border CreateCard()
    {
        var b = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = 12 };
        b.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        b.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);
        return b;
    }

    private Border CreateTab(string text, bool isActive)
    {
        var b = new Border { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 12 }, Padding = 8, BackgroundColor = isActive ? ThemeManager.AccentColor : Colors.Transparent };
        b.Content = new Label { Text = text, TextColor = isActive ? Colors.White : Colors.Gray, FontSize = 12, HorizontalTextAlignment = TextAlignment.Center };
        return b;
    }

    private Border CreateStatCard(string icon, string value, string label, bool isRed = false)
    {
        var b = CreateCard();
        var stack = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.Center };
        var img = new Image { Source = icon, WidthRequest = 24, HeightRequest = 24 };
        img.Behaviors.Add(new IconTintColorBehavior { TintColor = isRed ? Color.FromArgb("#FF414D") : ThemeManager.AccentColor });
        var val = new Label { Text = value, FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center };
        var lbl = new Label { Text = label, FontSize = 10, TextColor = Colors.Gray, HorizontalTextAlignment = TextAlignment.Center };
        ThemeManager.BindText(val);
        stack.Children.Add(img); stack.Children.Add(val); stack.Children.Add(lbl);
        b.Content = stack;
        return b;
    }

    private View CreateExpiryRow(string emoji, string name, string days, Color color)
    {
        var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10 };
        var img = new Label { Text = emoji, FontSize = 24, VerticalOptions = LayoutOptions.Center };
        var lbl = new Label { Text = name, FontSize = 14, VerticalOptions = LayoutOptions.Center };
        ThemeManager.BindText(lbl);
        var tag = new Border { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Padding = new Thickness(8,4), BackgroundColor = color };
        tag.Content = new Label { Text = days, TextColor = Colors.White, FontSize = 12, FontAttributes = FontAttributes.Bold };
        grid.Add(img, 0, 0); grid.Add(lbl, 1, 0); grid.Add(tag, 2, 0);
        return grid;
    }
}
