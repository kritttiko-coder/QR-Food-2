using Microsoft.Maui.Storage;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace FoodExpiryApp;

public class LocalizationManager : INotifyPropertyChanged
{
    public static LocalizationManager Instance { get; } = new();

    private string _currentLanguageCode;
    
    // Поддерживаемые языки
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
        
        // Оповещаем UI, что язык изменился
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

    // Главный метод перевода
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

    // --- СЛОВАРИ ПЕРЕВОДОВ ---
    private string GetRussian(string key) => key switch
    {
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
        _ => key
    };

    private string GetEnglish(string key) => key switch
    {
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
        _ => key
    };

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

// Удобный шорткат для использования в XAML или коде
public static class Loc
{
    public static string T(string key) => LocalizationManager.Instance[key];
}
