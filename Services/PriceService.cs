using FoodExpiryApp.Models;

namespace FoodExpiryApp.Services;

public class PriceService
{
    // 🛒 "Каталог ближайшего магазина" (пока в памяти, потом можно заменить на API)
    // Формат: Ключевое слово в названии -> (Цена в рублях, Цена в долларах)
    private readonly Dictionary<string, (double Rub, double Usd)> _storeCatalog = new()
    {
        { "молоко", (85, 1.2) },
        { "хлеб", (45, 0.8) },
        { "яйца", (130, 2.5) },
        { "сыр", (450, 6.5) },
        { "яблоки", (160, 2.2) },
        { "банан", (90, 1.1) },
        { "мясо", (550, 8.0) },
        { "курица", (320, 4.5) },
        { "вода", (40, 0.6) },
        { "кола", (110, 1.5) },
        { "печенье", (80, 1.2) },
        { "масло", (180, 2.8) },
        { "кофе", (400, 6.0) },
        { "чай", (150, 2.5) },
    };

    // Определяем символ валюты в зависимости от языка
    public string CurrencySymbol => LocalizationManager.Instance.CurrentLanguageCode == "en" ? "$" : "₽";

    // Получить цену для продукта из базы данных
    public double GetPrice(Product product)
    {
        return GetPrice(product.Name);
    }

    // Получить цену по названию (для списка покупок)
    public double GetPrice(string productName)
    {
        if (string.IsNullOrWhiteSpace(productName)) return DefaultPrice;

        string name = productName.ToLowerInvariant();

        // Ищем ключевое слово в названии (например, "Молоко 2.5%" найдет "молоко")
        foreach (var item in _storeCatalog)
        {
            if (name.Contains(item.Key))
            {
                return LocalizationManager.Instance.CurrentLanguageCode == "en" ? item.Value.Usd : item.Value.Rub;
            }
        }

        // Если товар не найден в каталоге — ставим среднюю цену
        return DefaultPrice;
    }

    // Средняя цена для неизвестных товаров
    public double DefaultPrice => LocalizationManager.Instance.CurrentLanguageCode == "en" ? 3.5 : 180;
}
