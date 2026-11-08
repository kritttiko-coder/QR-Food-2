using FoodExpiryApp.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Behaviors;
using Microsoft.Maui.Layouts; 

namespace FoodExpiryApp;

public class MainPage : ContentPage
{
    private readonly ProductRepository repo;

    private readonly SearchBar search = new()
    {
        Placeholder = "Поиск продуктов...",
        FontSize = 17,
        HeightRequest = 58,
        CancelButtonColor = Color.FromArgb("#A978F5")
    };

    private readonly FlexLayout cards = new()
    {
        Direction = Microsoft.Maui.Layouts.FlexDirection.Row,
        Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
        JustifyContent = Microsoft.Maui.Layouts.FlexJustify.SpaceBetween,
        AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Start,
        Margin = new Thickness(0, 10, 0, 10)
    };

    private readonly Label countLabel = new()
    {
        FontSize = 15
    };

    private int density;
    private int activeTab; // 0 - Меню, 1 - Магазин, 2 - Аккаунт, 3 - Настройки
    private List<Product> all = new();
    
    private Image? densityImage; 

    // Ссылки на иконки нижней панели, чтобы менять им рамки
    private Border[] navBorders = new Border[4];

    public MainPage(ProductRepository repository)
    {
        repo = repository;
        Title = "Мои продукты";
        Padding = 0;

        ThemeManager.Apply();
        ThemeManager.BindPage(this);

        BuildInterface();
    }

    private void BuildInterface()
    {
        var title = new Label
        {
            Text = "Мои продукты",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };
        ThemeManager.BindText(title);

        ThemeManager.BindText(countLabel, secondary: true);

        var header = new VerticalStackLayout
        {
            Spacing = 5,
            Children = { title, countLabel }
        };

        // --- Кнопка плотности (Смена иконок) ---
        densityImage = new Image { WidthRequest = 28, HeightRequest = 28 };
        UpdateDensityIcon(); 
        densityImage.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });

        var densityButton = CreateIconButtonWithImage(densityImage);
        densityButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() =>
            {
                density = (density + 1) % 3; // 0 -> 1 -> 2 -> 0
                UpdateDensityIcon();
                Render();
            })
        });

        // --- Кнопка настроек ---
        var settingsImg = new Image { Source = "icon_gear.png", WidthRequest = 28, HeightRequest = 28 };
        settingsImg.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var settingsButton = CreateIconButtonWithImage(settingsImg);
        settingsButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Navigation.PushAsync(new SettingsPage()))
        });

        var top = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10
        };

        top.Add(header, 0, 0);
        top.Add(densityButton, 1, 0);
        top.Add(settingsButton, 2, 0);

        ThemeManager.BindSearch(search);

        var searchFrame = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = new Thickness(8, 0),
            Content = search
        };
        searchFrame.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.Surface);
        search.TextChanged += (_, _) => Render();

        // --- Кнопка QR ---
        var scanImg = new Image { Source = "icon_qr.png", WidthRequest = 40, HeightRequest = 40, HorizontalOptions = LayoutOptions.Center };
        var scanLabel = new Label { Text = "Сканировать", FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center };
        
        var scanButton = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 19 },
            BackgroundColor = ThemeManager.AccentColor,
            Padding = 10,
            Content = new VerticalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center, Children = { scanImg, scanLabel } }
        };
        scanButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await DisplayAlert("Сканирование QR", "Сканер ещё не подключён.", "Понятно"))
        });

        // --- Кнопка Добавить ---
        var addImg = new Image { Source = "icon_plus.png", WidthRequest = 40, HeightRequest = 40, HorizontalOptions = LayoutOptions.Center };
        addImg.Behaviors.Add(new IconTintColorBehavior { TintColor = (Color)Application.Current!.Resources[ThemeManager.Text] });
        var addLabel = new Label { Text = "Добавить", FontSize = 16, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center };
        ThemeManager.BindText(addLabel);

        var addButton = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 19 },
            Padding = 10,
            Content = new VerticalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center, Children = { addImg, addLabel } }
        };
        addButton.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.SurfaceAlt);
        addButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await Navigation.PushAsync(new ProductEditPage(repo)))
        });

        var actions = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 12
        };
        actions.Add(scanButton, 0, 0);
        actions.Add(addButton, 1, 0);

        var content = new VerticalStackLayout
        {
            Padding = new Thickness(18, 22, 18, 24),
            Spacing = 18,
            Children = { top, searchFrame, actions, cards }
        };

        // --- КАСТОМНЫЙ НИЖНИЙ БАР ---
        var bottomNav = CreateBottomNav();

        // Основной контейнер: контент сверху, бар снизу
        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };

        mainGrid.Add(new ScrollView { Content = content }, 0, 0);
        mainGrid.Add(bottomNav, 0, 1);

        Content = mainGrid;

        // Добавляем свайпы на страницу
        AddSwipeGestures();
    }

    private Border CreateBottomNav()
    {
        var navGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)
            },
            Padding = new Thickness(10, 8, 10, 16) // Нижний отступ для системной навигации
        };

        // Иконки: Меню, Магазин, Аккаунт, Настройки
        string[] icons = { "icon_menu.png", "icon_store.png", "icon_account.png", "icon_gear.png" };
        
        for (int i = 0; i < 4; i++)
        {
            var img = new Image { Source = icons[i], WidthRequest = 26, HeightRequest = 26, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            // Все иконки всегда белые
            img.Behaviors.Add(new IconTintColorBehavior { TintColor = Colors.White });

            var border = new Border
            {
                StrokeThickness = 0, // По умолчанию без рамки
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                WidthRequest = 48, HeightRequest = 48,
                Padding = 0,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Content = img
            };

            navBorders[i] = border;

            int tabIndex = i; // Замыкание для правильного индекса
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => SwitchTab(tabIndex);
            border.GestureRecognizers.Add(tap);

            navGrid.Add(border, i, 0);
        }

        // Фон панели: берем SurfaceAlt и делаем прозрачность 67%
        var bgColor = (Color)Application.Current!.Resources[ThemeManager.SurfaceAlt];
        var transparentBg = Color.FromRgba(bgColor.Red, bgColor.Green, bgColor.Blue, 0.67f);

        var navContainer = new Border
        {
            BackgroundColor = transparentBg,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24, 24, 0, 0) },
            Content = navGrid
        };

        // Инициализируем выделение первого таба (Меню)
        UpdateNavSelection(0);

        return navContainer;
    }

    private void UpdateNavSelection(int tabIndex)
    {
        activeTab = tabIndex;
        for (int i = 0; i < navBorders.Length; i++)
        {
            if (i == activeTab)
            {
                // Выбранная: рамка акцентным цветом
                navBorders[i].Stroke = ThemeManager.AccentColor;
                navBorders[i].StrokeThickness = 2;
            }
            else
            {
                // Невыбранная: без рамки
                navBorders[i].Stroke = Colors.Transparent;
                navBorders[i].StrokeThickness = 0;
            }
        }
    }

    private void SwitchTab(int tabIndex)
    {
        if (activeTab == tabIndex) return; // Уже на этой вкладке

        UpdateNavSelection(tabIndex);

        // Логика переключения вкладок
        switch (tabIndex)
        {
            case 0:
                // Мы уже на главной (Меню)
                break;
            case 1:
                // Магазин
                DisplayAlert("Магазин", "Интерфейс магазина в разработке.", "ОК");
                break;
            case 2:
                // Аккаунт
                DisplayAlert("Аккаунт", "Профиль пользователя в разработке.", "ОК");
                break;
            case 3:
                // Настройки
                Navigation.PushAsync(new SettingsPage());
                // Возвращаем выделение на "Меню", так как мы уходим со страницы
                UpdateNavSelection(0); 
                break;
        }
    }

    private void AddSwipeGestures()
    {
        var swipeLeft = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        swipeLeft.Swiped += (_, _) =>
        {
            int nextTab = (activeTab + 1) % 4;
            SwitchTab(nextTab);
        };

        var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeRight.Swiped += (_, _) =>
        {
            int prevTab = (activeTab - 1 + 4) % 4;
            SwitchTab(prevTab);
        };

       if (Content != null)
        {
            Content.GestureRecognizers.Add(swipeLeft);
            Content.GestureRecognizers.Add(swipeRight);
        }
    }

    private void UpdateDensityIcon()
    {
        if (densityImage == null) return;

        densityImage.Source = density switch
        {
            0 => "icon_layout_2.png", 
            1 => "icon_layout_3.png", 
            2 => "icon_layout_4.png", 
            _ => "icon_layout_2.png"
        };
    }

    private Border CreateIconButtonWithImage(Image img)
    {
        var border = new Border
        {
            WidthRequest = 58,
            HeightRequest = 58,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 19 },
            Content = img,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        border.SetDynamicResource(VisualElement.BackgroundColorProperty, ThemeManager.SurfaceAlt);
        return border;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ThemeManager.Apply();

        if (densityImage != null) 
        {
            densityImage.Behaviors.Clear();
            densityImage.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        }

        try
        {
            all = await repo.All();
            Render();
        }
        catch (Exception ex)
        {
            cards.Children.Clear();
            var error = new Label { Text = "Не удалось загрузить продукты:\n" + ex.Message, FontSize = 14, TextColor = Color.FromArgb("#FF6878") };
            cards.Children.Add(error);
        }
    }

    private void Render()
    {
        cards.Children.Clear();
        var query = search.Text?.Trim() ?? "";

        var filtered = all
            .Where(p =>
                string.IsNullOrWhiteSpace(query) ||
                (p.Name ?? "").Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (p.Brand ?? "").Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (p.Barcode ?? "").Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        countLabel.Text = $"Ваш холодильник • {all.Count} продуктов";

        if (filtered.Count == 0)
        {
            var empty = new Label
            {
                Text = all.Count == 0 ? "Здесь пока нет продуктов.\nНажми «Добавить», чтобы начать." : "Ничего не найдено.",
                FontSize = 16,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 28),
                WidthRequest = 300 // Ограничиваем ширину, чтобы во FlexLayout встало по центру
            };
            ThemeManager.BindText(empty, secondary: true);
            cards.Children.Add(empty);
            return;
        }

        foreach (var group in filtered.GroupBy(p => p.GroupKey))
        {
            var items = group.OrderBy(p => p.ExpiryDate).ToList();
            var product = items[0];
            
            var card = CreateProductCard(product, items);

            // Настраиваем ширину карточки в зависимости от плотности
            if (density == 0) 
            {
                // 1 в ряд (100%)
                card.WidthRequest = -1; 
                FlexLayout.SetBasis(card, new FlexBasis(1f, true));
            }
            else if (density == 1)
            {
                // 2 в ряд (48% ширины)
                FlexLayout.SetBasis(card, new FlexBasis(0.48f, true));
                card.Margin = new Thickness(0, 0, 0, 10);
            }
            else
            {
                // 4 квадрата (23% ширины) - пока оставим как 3 в ряд для красоты
                FlexLayout.SetBasis(card, new FlexBasis(0.31f, true));
                card.Margin = new Thickness(0, 0, 0, 10);
            }

            cards.Children.Add(card);
        }
    }

    private Border CreateProductCard(Product product, List<Product> items)
    {
        var compact = density >= 1; // В сетке карточки компактнее
        var iconOnly = density == 2;

        // Размер иконки адаптируем
        var imageSize = iconOnly ? 50 : compact ? 64 : 90;

        var imageSymbol = product.Category switch
        {
            "Молочные" => "◉", "Напитки" => "◇", "Мясо и рыба" => "◈",
            "Овощи и фрукты" => "❋", "Заморозка" => "❄", _ => "□"
        };

        var imageLabel = new Label
        {
            Text = imageSymbol,
            FontSize = iconOnly ? 24 : compact ? 30 : 44,           
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            TextColor = Colors.White // Белая иконка на черном фоне
        };
        
        
        var imagePlaceholder = new Border
        {
            WidthRequest = imageSize,
            HeightRequest = imageSize,
            BackgroundColor = Colors.Black, 
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 13 },
            Content = imageLabel
        };

        var name = new Label { Text = product.Name, FontSize = iconOnly ? 13 : compact ? 15 : 19, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.TailTruncation };
        ThemeManager.BindText(name);

        var details = new Label
        {
            Text = string.Join(" • ", new[] { product.Variant, product.Volume, items.Count > 1 ? $"{items.Count} шт." : "" }.Where(s => !string.IsNullOrWhiteSpace(s))),
            FontSize = compact ? 11 : 14,
            IsVisible = !iconOnly
        };
        ThemeManager.BindText(details, secondary: true);

        var expiryLabels = new FlexLayout { Direction = Microsoft.Maui.Layouts.FlexDirection.Row, Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
        foreach (var item in items)
        {
            expiryLabels.Children.Add(new Label
            {
                Text = ExpiryColor.Label(item.ExpiryDate),
                TextColor = ExpiryColor.For(item.ExpiryDate),
                FontSize = compact ? 11 : 15,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 2, 6, 0)
            });
        }

        var textContent = new VerticalStackLayout { Spacing = 3, VerticalOptions = LayoutOptions.Center, Children = { name } };
        if (!iconOnly)
        {
            textContent.Children.Add(details);
            textContent.Children.Add(expiryLabels);
        }
        else
        {
            textContent.Children.Add(expiryLabels);
        }

        // --- Точки и корзина ---
        var menuImg = new Image { Source = "icon_dots.png", WidthRequest = 20, HeightRequest = 20 };
        menuImg.Behaviors.Add(new IconTintColorBehavior { TintColor = ThemeManager.AccentColor });
        var menuButton = new Border { Content = menuImg, Padding = 4, BackgroundColor = Colors.Transparent, HeightRequest = 28 };
        menuButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                var options = new List<string> { "Изменить", "Удалить один" };
                if (items.Count > 1) options.Add("Удалить все");
                var choice = await DisplayActionSheet(product.Name, "Отмена", null, options.ToArray());

                if (choice == "Изменить") await Navigation.PushAsync(new ProductEditPage(repo, items[0]));
                else if (choice == "Удалить один") { await repo.Delete(items[0]); all = await repo.All(); Render(); }
                else if (choice == "Удалить все") { foreach (var item in items) await repo.Delete(item); all = await repo.All(); Render(); }
            })
        });

        var deleteImg = new Image { Source = "icon_trash.png", WidthRequest = 20, HeightRequest = 20 };
        deleteImg.Behaviors.Add(new IconTintColorBehavior { TintColor = Color.FromArgb("#FF647D") });
        var deleteButton = new Border { Content = deleteImg, Padding = 4, BackgroundColor = Colors.Transparent, HeightRequest = 28 };
        deleteButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                var options = items.Count > 1 ? new[] { "Удалить один", "Удалить все" } : new[] { "Удалить" };
                var choice = await DisplayActionSheet("Удаление", "Отмена", null, options);

                if (choice == "Удалить один" || choice == "Удалить") await repo.Delete(items[0]);
                else if (choice == "Удалить все") foreach (var item in items) await repo.Delete(item);
                else return;

                all = await repo.All();
                Render();
            })
        });

        // Кнопки в правом верхнем углу, аккуратной вертикальной стопкой
        var controls = new VerticalStackLayout 
        { 
            Spacing = 2, 
            VerticalOptions = LayoutOptions.Start, 
            HorizontalOptions = LayoutOptions.End,
            Children = { menuButton, deleteButton } 
        };

        // Собираем карточку
        var row = new Grid
        {
            ColumnDefinitions = 
            { 
                new ColumnDefinition(GridLength.Auto), // Иконка
                new ColumnDefinition(GridLength.Star), // Текст
                new ColumnDefinition(GridLength.Auto)  // Кнопки
            },
            ColumnSpacing = 10,
            VerticalOptions = LayoutOptions.Center
        };

        row.Add(imagePlaceholder, 0, 0);
        row.Add(textContent, 1, 0);
        row.Add(controls, 2, 0);

        var originalTint = CategoryStyle.Tint(product.Category);
        var translucentTint = Color.FromRgba(originalTint.Red, originalTint.Green, originalTint.Blue, 0.45f);

        var border = new Border
        {
            BackgroundColor = translucentTint,
            StrokeThickness = 1.5, 
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = new Thickness(compact ? 10 : 14),
            Content = row
        };
        border.SetDynamicResource(Border.StrokeProperty, ThemeManager.Accent);

        return border;
    }
}
