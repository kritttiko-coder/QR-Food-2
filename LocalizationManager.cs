using Microsoft.Maui.Storage;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace FoodExpiryApp;

public class LocalizationManager : INotifyPropertyChanged
{
    public static LocalizationManager Instance { get; } = new();

    private string _currentLanguageCode;
    
    public static readonly string[] SupportedLanguages = { "ru", "en" };

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? LanguageChanged;

    private LocalizationManager()
    {
        _currentLanguageCode = Preferences.Default.Get("app_language", "ru");
        ApplyLanguage(_currentLanguageCode);
    }

    public string CurrentLanguageCode => _currentLanguageCode;

    public void SetLanguage(string langCode)
    {
        if (_currentLanguageCode == langCode) return;
        
        _currentLanguageCode = langCode;
        Preferences.Default.Set("app_language", langCode);
        
        ApplyLanguage(langCode);
        
        LanguageChanged?.Invoke();
        OnPropertyChanged(nameof(CurrentLanguageCode));
    }

    private void ApplyLanguage(string langCode)
    {
        var culture = new CultureInfo(langCode);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    public string this[string key]
    {
        get
        {
            return _currentLanguageCode switch
            {
                "en" => GetEnglish(key),
                _ => GetRussian(key)
            };
        }
    }

    // ==========================================
    // --- РУССКИЙ ЯЗЫК ---
    // ==========================================
    private string GetRussian(string key) => key switch
    {
        // Главная страница
        "MyProducts" => "Мои продукты",
        "FridgeCount" => "Ваш холодильник",
        "ProductsCount" => "продуктов",
        "SearchPlaceholder" => "Поиск продуктов...",
        "Scan" => "Сканировать",
        "Add" => "Добавить",
        "EmptyFridge" => "Здесь пока нет продуктов.\nНажми «Добавить», чтобы начать.",
        "NothingFound" => "Ничего не найдено.",
        
        // Всплывающие меню продуктов
        "Edit" => "Изменить",
        "DeleteOne" => "Удалить один",
        "DeleteAll" => "Удалить все",
        "DeleteProduct" => "Удалить",
        "Cancel" => "Отмена",
        
        // Дни и время
        "Days" => "дней",
        "Day" => "день",
        "Expired" => "Просрочен",
        "Today" => "Сегодня",
        
        // Настройки (SettingsPage)
        "Settings" => "Настройки",
        "Personalize" => "Персонализируйте и изменяйте приложение под себя",
        "Language" => "Язык",
        "ChooseLanguage" => "Выберите язык интерфейса",
        "Appearance" => "Внешний вид",
        "AppearanceDesc" => "Тема, акцентный цвет и иконка",
        "DateTime" => "Дата и время",
        "DateTimeDesc" => "Дата, часы и настройки региона",
        "Info" => "Информация",
        "InfoDesc" => "Цвета сроков годности и категорий",
        "Support" => "Поддержать автора",
        "SupportDesc" => "Помочь развитию FoodExpiryApp",
        "Help" => "Помощь и поддержка",
        "HelpDesc" => "Сообщить о проблеме или ошибке",
        "Version" => "версия 0.2.0",
        "Beta" => "Бета",

        // Кастомизация (CustomizationPage)
        "Theme" => "Тема",
        "ThemeBlack" => "Чёрная",
        "ThemeDark" => "Тёмная",
        "ThemeLight" => "Светлая",
        "AccentColor" => "Акцентный цвет",
        "AppIcon" => "Иконка приложения",
        "Preview" => "Предпросмотр",
        "IconClassic" => "Базовая",
        "IconWinter" => "Зимняя",
        "IconHalloween" => "Хэллоуин",
        "IconMono" => "Ч/Б",
        "IconNeon" => "Неон",

        // Добавление продукта (ProductEditPage)
        "NewProduct" => "Новый продукт",
        "Name" => "Название",
        "Brand" => "Бренд",
        "Variant" => "Разновидность, например 2.5%",
        "Volume" => "Объём / масса, например 0.930 л",
        "Barcode" => "Штрихкод / код маркировки",
        "Category" => "Категория",
        "ExpiryDate" => "Срок годности",
        "Save" => "Сохранить",
        "CatDairy" => "Молочные",
        "CatDrinks" => "Напитки",
        "CatMeat" => "Мясо и рыба",
        "CatVeg" => "Овощи и фрукты",
        "CatFreezer" => "Заморозка",
        "CatOther" => "Другое",

        // Нижнее меню
"Home" => "Главная",
"Store" => "Магазин",
"Statistics" => "Статистика",
"Account" => "Аккаунт",
"ScanTab" => "Сканер",

// Магазин
"StoreTitle" => "Магазин",
"SearchStores" => "Поиск по магазинам или продуктам",
"ShoppingList" => "Список покупок",
"Add" => "Добавить",
"Delete" => "Удалить",
"AvailableStores" => "Доступные магазины",
"TemplateList" => "Шаблон списка",
"TemplateDesc" => "Быстрый выбор товаров",

// Аккаунт
"Profile" => "Профиль",
"UserRole" => "Пользователь",
"Sync" => "Синхронизация",
"LastSync" => "Последняя: сегодня, 18:45",
"ChangeName" => "Изменить имя",
"Appearance" => "Оформление",
"AppearanceDesc" => "Тема, цвет, иконка",
"More" => "Дополнительно",
"SupportDev" => "Поддержать разработчика",
"AboutApp" => "О приложении",
"LogOut" => "Выйти из аккаунта",

// Статистика
"Week" => "Неделя",
"Month" => "Месяц",
"Year" => "Год",
"ProductsCount" => "Продукта",
"AvgShelfLife" => "Дней ср. срок",
"OnTime" => "Вовремя",
"Expired" => "Просрочено",
"ProductChanges" => "Изменение продуктов",
"Categories" => "Категории",
"CostEstimation" => "Оценка стоимости продуктов",
"Spent" => "Потрачено на продукты за период",
"CostWarning" => "Это экспериментальная функция. Данные примерные и находятся с помощью средней цены в ближайших магазинов. Точность зависит от вашего местоположения.",
"NearestExpirations" => "Ближайшие сроки",
        
        // Инфо (InfoPage)
"ExpiryColorsTitle" => "Цвета сроков годности",
"Exp0_3" => "0–3 дня",
"Exp4_7" => "4–7 дней",
"Exp8_14" => "8–14 дней",
"Exp15_21" => "15–21 день",
"Exp22Plus" => "22+ дня",
"CategoryBackgrounds" => "Фоны категорий:",
"ColorWarning" => "Цвет — визуальная подсказка, а не оценка безопасности пищи.",

        // Поддержка
        "SupportAuthor" => "Поддержать разработчика",
        "ThanksSupport" => "Спасибо, что помогаешь развивать FoodExpiryApp ❤️",
        "Coffee" => "На чашку кофе — 70 ₽",
        "Cookie" => "Пачку печенек — 120 ₽",
        "GoldCookie" => "Золотые печеньки — 500 ₽",
        "PaymentNote" => "Оплата пока не подключена. Для реальных пожертвований потребуется платёжный сервис или официальная ссылка.",

        _ => key
    };

    // ==========================================
    // --- English language ---
    // ==========================================
    private string GetEnglish(string key) => key switch
    {
        // Main Page
        "MyProducts" => "My Products",
        "FridgeCount" => "Your fridge",
        "ProductsCount" => "products",
        "SearchPlaceholder" => "Search products...",
        "Scan" => "Scan",
        "Add" => "Add",
        "EmptyFridge" => "No products yet.\nTap 'Add' to start.",
        "NothingFound" => "Nothing found.",
        
        // Action Sheets
        "Edit" => "Edit",
        "DeleteOne" => "Delete one",
        "DeleteAll" => "Delete all",
        "DeleteProduct" => "Delete",
        "Cancel" => "Cancel",
        
        // Days & Time
        "Days" => "days",
        "Day" => "day",
        "Expired" => "Expired",
        "Today" => "Today",
        
        // Settings
        "Settings" => "Settings",
        "Personalize" => "Personalize and customize the app",
        "Language" => "Language",
        "ChooseLanguage" => "Choose interface language",
        "Appearance" => "Appearance",
        "AppearanceDesc" => "Theme, accent color and icon",
        "DateTime" => "Date & Time",
        "DateTimeDesc" => "Date, clock and region settings",
        "Info" => "Information",
        "InfoDesc" => "Expiry and category colors",
        "Support" => "Support the author",
        "SupportDesc" => "Help develop FoodExpiryApp",
        "Help" => "Help & Support",
        "HelpDesc" => "Report a problem or error",
        "Version" => "version 0.2.0",
        "Beta" => "Beta",

        // Customization
        "Theme" => "Theme",
        "ThemeBlack" => "Black",
        "ThemeDark" => "Dark",
        "ThemeLight" => "Light",
        "AccentColor" => "Accent color",
        "AppIcon" => "App icon",
        "Preview" => "Preview",
        "IconClassic" => "Classic",
        "IconWinter" => "Winter",
        "IconHalloween" => "Halloween",
        "IconMono" => "B&W",
        "IconNeon" => "Neon",

        // Product Edit
        "NewProduct" => "New Product",
        "Name" => "Name",
        "Brand" => "Brand",
        "Variant" => "Variant, e.g. 2.5%",
        "Volume" => "Volume / weight, e.g. 0.930 L",
        "Barcode" => "Barcode",
        "Category" => "Category",
        "ExpiryDate" => "Expiry date",
        "Save" => "Save",
        "CatDairy" => "Dairy",
        "CatDrinks" => "Drinks",
        "CatMeat" => "Meat & Fish",
        "CatVeg" => "Vegetables & Fruits",
        "CatFreezer" => "Frozen",
        "CatOther" => "Other",

        // Bottom Nav
"Home" => "Home",
"Store" => "Store",
"Statistics" => "Statistics",
"Account" => "Account",
"ScanTab" => "Scan",

// Store
"StoreTitle" => "Store",
"SearchStores" => "Search stores or products",
"ShoppingList" => "Shopping List",
"Add" => "Add",
"Delete" => "Delete",
"AvailableStores" => "Available stores",
"TemplateList" => "List template",
"TemplateDesc" => "Quick product selection",

// Account
"Profile" => "Profile",
"UserRole" => "User",
"Sync" => "Synchronization",
"LastSync" => "Last: today, 18:45",
"ChangeName" => "Change name",
"Appearance" => "Appearance",
"AppearanceDesc" => "Theme, color, icon",
"More" => "More",
"SupportDev" => "Support developer",
"AboutApp" => "About app",
"LogOut" => "Log out",

// Statistics
"Week" => "Week",
"Month" => "Month",
"Year" => "Year",
"ProductsCount" => "Products",
"AvgShelfLife" => "Avg shelf life",
"OnTime" => "On time",
"Expired" => "Expired",
"ProductChanges" => "Product changes",
"Categories" => "Categories",
"CostEstimation" => "Product cost estimation",
"Spent" => "Spent on products",
"CostWarning" => "This is an experimental feature. Data is approximate and based on the average price in nearby stores. Accuracy depends on your location.",
"NearestExpirations" => "Nearest expirations",

        // info (InfoPage)
"ExpiryColorsTitle" => "Expiry Date Colors",
"Exp0_3" => "0–3 days",
"Exp4_7" => "4–7 days",
"Exp8_14" => "8–14 days",
"Exp15_21" => "15–21 days",
"Exp22Plus" => "22+ days",
"CategoryBackgrounds" => "Category backgrounds:",
"ColorWarning" => "Color is a visual hint, not a food safety assessment.",

        // Support
        "SupportAuthor" => "Support the developer",
        "ThanksSupport" => "Thank you for helping develop FoodExpiryApp ❤️",
        "Coffee" => "A cup of coffee — $1",
        "Cookie" => "A pack of cookies — $2",
        "GoldCookie" => "Golden cookies — $5",
        "PaymentNote" => "Payment is not connected yet. A payment service or official link is required for real donations.",

        _ => key
    };

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}


public static class Loc
{
    public static string T(string key) => LocalizationManager.Instance[key];
}
