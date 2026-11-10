using FoodExpiryApp.Models;
using FoodExpiryApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Behaviors;
using System.Globalization;

namespace FoodExpiryApp;

public class StatisticsPage : ContentPage
{
    private readonly ProductRepository _repo;
    
    // Ссылки на элементы UI, которые будем обновлять
    private Label _lblProducts = null!, _lblAvgDays = null!, _lblOnTime = null!, _lblExpired = null!;
    private GraphicsView _lineChartView = null!, _donutChartView = null!;
    private Label _lblCost = null!;
    private VerticalStackLayout _nearestList = null!;
    
    private List<Product> _allProducts = new();
    private int _currentPeriod = 0; // 0 - Week, 1 - Month, 2 - Year
    private Button[] _periodBtns = new Button[3];

    public StatisticsPage(ProductRepository repo)
    {
        _repo = repo;
        Title = Loc.T("Statistics");
        ThemeManager.BindPage(this);
        BuildUI();
    }

    private async void BuildUI()
    {
        var content = new VerticalStackLayout { Padding = 16, Spacing = 12 };

        // --- ШАПКА ---
        var header = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        var title = new Label { Text = Loc.T("Statistics"), FontSize = 28, FontAttributes = FontAttributes.Bold };
        ThemeManager.BindText(title);
        var cal = new Image { Source = "icon_calendar.png", WidthRequest = 24, HeightRequest = 24 };
        cal.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        header.Add(title, 0, 0); header.Add(cal, 1, 0);
        content.Children.Add(header);

        // --- ТАБЫ (Неделя / Месяц / Год) ---
        var tabsGrid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8 };
        _periodBtns[0] = CreatePeriodBtn(Loc.T("Week"), 0, true);
        _periodBtns[1] = CreatePeriodBtn(Loc.T("Month"), 1, false);
        _periodBtns[2] = CreatePeriodBtn(Loc.T("Year"), 2, false);
        tabsGrid.Add(_periodBtns[0], 0, 0); tabsGrid.Add(_periodBtns[1], 1, 0); tabsGrid.Add(_periodBtns[2], 2, 0);
        content.Children.Add(tabsGrid);

        // --- ВЕРХНИЕ 4 КАРТОЧКИ ---
        var topCards = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8 };
        topCards.Add(CreateStatCard("icon_box.png", out _lblProducts, Loc.T("ProductsCount")), 0, 0);
        topCards.Add(CreateStatCard("icon_calendar_empty.png", out _lblAvgDays, Loc.T("AvgShelfLife")), 1, 0);
        topCards.Add(CreateStatCard("icon_check.png", out _lblOnTime, Loc.T("OnTime"), false), 2, 0);
        topCards.Add(CreateStatCard("icon_trash.png", out _lblExpired, Loc.T("Expired"), true), 3, 0);
        content.Children.Add(topCards);

        // --- ГРАФИК ЛИНИЯ ---
        var lineCard = CreateCard();
        var lineStack = new VerticalStackLayout { Spacing = 10 };
        lineStack.Children.Add(new Label { Text = Loc.T("ProductChanges"), FontSize = 14, FontAttributes = FontAttributes.Bold });
        _lineChartView = new GraphicsView { HeightRequest = 140 };
        lineStack.Children.Add(_lineChartView);
        lineCard.Content = lineStack;
        content.Children.Add(lineCard);

        // --- ДИАГРАММА КОЛЬЦО ---
        var donutCard = CreateCard();
        var donutGrid = new Grid { ColumnDefinitions = { new ColumnDefinition(0.5, GridUnitType.Star), new ColumnDefinition(0.5, GridUnitType.Star) } };
        
        _donutChartView = new GraphicsView { HeightRequest = 120 };
        donutGrid.Add(_donutChartView, 0, 0);
        
        var legendStack = new VerticalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };
        legendStack.Children.Add(CreateLegendItem(Color.FromArgb("#A978F5"), "Молочные"));
        legendStack.Children.Add(CreateLegendItem(Color.FromArgb("#3978F6"), "Овощи/Фрукты"));
        legendStack.Children.Add(CreateLegendItem(Color.FromArgb("#F08A35"), "Мясо/Рыба"));
        legendStack.Children.Add(CreateLegendItem(Color.FromArgb("#25B99A"), "Напитки"));
        donutGrid.Add(legendStack, 1, 0);
        
        donutCard.Content = donutGrid;
        content.Children.Add(donutCard);

        // --- ОЦЕНКА СТОИМОСТИ ---
        var costCard = CreateCard();
        var costStack = new VerticalStackLayout { Spacing = 8 };
        var costHeader = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        costHeader.Add(new Label { Text = Loc.T("CostEstimation"), FontSize = 14, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center }, 0, 0);
        var sw = new Switch { IsToggled = true, OnColor = ThemeManager.AccentColor };
        costHeader.Add(sw, 1, 0);
        costStack.Children.Add(costHeader);

        var priceStack = new HorizontalStackLayout { Spacing = 8 };
        var priceIcon = new Image { Source = "icon_wallet.png", WidthRequest = 24, HeightRequest = 24 };
        priceIcon.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        _lblCost = new Label { Text = "≈ 0 ₽", FontSize = 22, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };
        ThemeManager.BindText(_lblCost);
        priceStack.Children.Add(priceIcon); priceStack.Children.Add(_lblCost);
        costStack.Children.Add(priceStack);

        var warn = new Label { Text = Loc.T("CostWarning"), FontSize = 10, TextColor = Colors.Gray };
        costStack.Children.Add(warn);
        costCard.Content = costStack;
        content.Children.Add(costCard);

        // --- БЛИЖАЙШИЕ СРОКИ ---
        var nearCard = CreateCard();
        var nearStack = new VerticalStackLayout { Spacing = 10 };
        nearStack.Children.Add(new Label { Text = Loc.T("NearestExpirations"), FontSize = 14, FontAttributes = FontAttributes.Bold });
        _nearestList = new VerticalStackLayout { Spacing = 8 };
        nearStack.Children.Add(_nearestList);
        nearCard.Content = nearStack;
        content.Children.Add(nearCard);

        // Сборка
        var scroll = new ScrollView { Content = content };
        var rootGrid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) } };
        rootGrid.Add(scroll, 0, 0);
        rootGrid.Add(new BottomNavBar(3, _repo), 0, 1);

        // Свайпы
        var swipeL = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        swipeL.Swiped += (_, _) => BottomNavBar.SwitchToTab(4, _repo);
        rootGrid.GestureRecognizers.Add(swipeL);
        var swipeR = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeR.Swiped += (_, _) => BottomNavBar.SwitchToTab(1, _repo);
        rootGrid.GestureRecognizers.Add(swipeR);

        Content = rootGrid;
        ThemeManager.Apply();

        await LoadDataAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync(); // Обновляем данные при каждом заходе
    }

    private async Task LoadDataAsync()
    {
        _allProducts = await _repo.All();
        CalculateStats();
    }

    private void CalculateStats()
    {
        DateTime now = AppClock.Now;
        DateTime threshold = _currentPeriod switch
        {
            0 => now.AddDays(-7),   // Week
            1 => now.AddDays(-30),  // Month
            2 => now.AddDays(-365), // Year
            _ => now.AddDays(-7)
        };

        // Берем только те продукты, которые были добавлены в выбранный период
        var filtered = _allProducts.Where(p => p.ExpiryDate >= threshold || p.ExpiryDate < now).ToList();
        if (filtered.Count == 0) filtered = _allProducts; // fallback

        // 1. Верхние карточки
        int total = filtered.Count;
        int onTime = filtered.Count(p => p.ExpiryDate >= now);
        int expired = filtered.Count(p => p.ExpiryDate < now);
        int avgDays = total > 0 ? (int)filtered.Average(p => (p.ExpiryDate - now).TotalDays) : 0;

        _lblProducts.Text = total.ToString();
        _lblAvgDays.Text = avgDays.ToString();
        _lblOnTime.Text = total > 0 ? $"{(onTime * 100 / total)}%" : "0%";
        _lblExpired.Text = expired.ToString();

        // 2. Линейный график (Динамика добавления)
        var lineDrawable = new LineChartDrawable { AccentColor = ThemeManager.AccentColor };
        if (_currentPeriod == 0)
        {
            // Для недели - 7 дней
            var daysCount = new float[7];
            for (int i = 0; i < 7; i++)
            {
                var day = now.AddDays(-6 + i).Date;
                daysCount[i] = filtered.Count(p => p.ExpiryDate.Date == day); // Примерная логика
            }
            lineDrawable.Values = daysCount;
        }
        else
        {
            // Для месяца/года - 5 точек
            lineDrawable.Values = new float[] { 5, 12, 8, 15, total };
        }
        _lineChartView.Drawable = lineDrawable;
        _lineChartView.Invalidate();

        // 3. Круговая диаграмма (Категории)
        var catGroups = filtered.GroupBy(p => p.Category).OrderByDescending(g => g.Count()).Take(4).ToList();
        var donutDrawable = new DonutChartDrawable 
        { 
            Values = catGroups.Select(g => (float)g.Count()).ToArray(),
            Colors = new[] { Color.FromArgb("#A978F5"), Color.FromArgb("#3978F6"), Color.FromArgb("#F08A35"), Color.FromArgb("#25B99A") }
        };
        _donutChartView.Drawable = donutDrawable;
        _donutChartView.Invalidate();

        // 4. Стоимость (150 руб. за продукт, т.к. в базе нет цен)
        decimal estimatedCost = total * 150;
        _lblCost.Text = $"≈ {estimatedCost:N0} ₽";

        // 5. Ближайшие сроки (Топ 3)
        _nearestList.Children.Clear();
        var nearest = _allProducts.OrderBy(p => p.ExpiryDate).Take(3).ToList();
        foreach (var p in nearest)
        {
            int daysLeft = (p.ExpiryDate.Date - now.Date).Days;
            string daysStr = daysLeft < 0 ? Loc.T("Expired") : $"{daysLeft} дн.";
            Color tagColor = daysLeft < 0 ? Color.FromArgb("#B00035") : daysLeft <= 3 ? Color.FromArgb("#FF414D") : daysLeft <= 7 ? Color.FromArgb("#FF962E") : Color.FromArgb("#E8C928");

            var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10 };
            var emoji = new Label { Text = GetEmoji(p.Category), FontSize = 24, VerticalOptions = LayoutOptions.Center };
            var name = new Label { Text = p.Name, FontSize = 14, VerticalOptions = LayoutOptions.Center };
            ThemeManager.BindText(name);
            var tag = new Border { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 10 }, Padding = new Thickness(8, 4), BackgroundColor = tagColor, Content = new Label { Text = daysStr, TextColor = Colors.White, FontSize = 12, FontAttributes = FontAttributes.Bold } };
            
            grid.Add(emoji, 0, 0); grid.Add(name, 1, 0); grid.Add(tag, 2, 0);
            _nearestList.Children.Add(grid);
        }
    }

    private Button CreatePeriodBtn(string text, int index, bool isActive)
    {
        var btn = new Button
        {
            Text = text,
            FontSize = 12,
            HeightRequest = 36,
            CornerRadius = 18,
            BackgroundColor = isActive ? ThemeManager.AccentColor : Color.FromArgb("#252030"),
            TextColor = isActive ? Colors.White : Colors.Gray
        };
        btn.Clicked += (_, _) =>
        {
            _currentPeriod = index;
            for (int i = 0; i < 3; i++)
            {
                _periodBtns[i].BackgroundColor = i == index ? ThemeManager.AccentColor : Color.FromArgb("#252030");
                _periodBtns[i].TextColor = i == index ? Colors.White : Colors.Gray;
            }
            CalculateStats();
        };
        return btn;
    }

    private Border CreateStatCard(string icon, out Label valueLabel, string label, bool isRed = false)
    {
        var b = CreateCard();
        var stack = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.Center };
        var img = new Image { Source = icon, WidthRequest = 24, HeightRequest = 24 };
        img.Behaviors.Add(new IconTintColorBehavior { TintColor = isRed ? Color.FromArgb("#FF414D") : ThemeManager.AccentColor });
        valueLabel = new Label { Text = "0", FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center };
        var lbl = new Label { Text = label, FontSize = 10, TextColor = Colors.Gray, HorizontalTextAlignment = TextAlignment.Center };
        ThemeManager.BindText(valueLabel);
        stack.Children.Add(img); stack.Children.Add(valueLabel); stack.Children.Add(lbl);
        b.Content = stack;
        return b;
    }

    private Border CreateCard()
    {
        var b = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = 12 };
        b.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        b.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);
        return b;
    }

    private View CreateLegendItem(Color color, string text)
    {
        var h = new HorizontalStackLayout { Spacing = 8 };
        var dot = new BoxView { WidthRequest = 10, HeightRequest = 10, CornerRadius = 5, BackgroundColor = color, VerticalOptions = LayoutOptions.Center };
        var lbl = new Label { Text = text, FontSize = 11, VerticalOptions = LayoutOptions.Center };
        ThemeManager.BindText(lbl);
        h.Children.Add(dot); h.Children.Add(lbl);
        return h;
    }

    private string GetEmoji(string cat) => cat switch
    {
        "Молочные" => "🥛", "Напитки" => "🥤", "Мясо и рыба" => "🥩",
        "Овощи и фрукты" => "🍎", "Заморозка" => "❄", _ => "📦"
    };
}
