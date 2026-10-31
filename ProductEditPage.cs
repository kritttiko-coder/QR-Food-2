
using FoodExpiryApp.Models;
using FoodExpiryApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public class ProductEditPage : ContentPage
{
    private readonly ProductRepository repo;
    private readonly Product? editingProduct;

    private static readonly string[] Categories =
    {
        "Молочные",
        "Напитки",
        "Мясо и рыба",
        "Овощи и фрукты",
        "Заморозка",
        "Другое"
    };

    private readonly Entry name = new()
    {
        Placeholder = "Название",
        PlaceholderColor = Color.FromArgb("#AAA3B5")
    };

    private readonly Entry brand = new()
    {
        Placeholder = "Бренд",
        PlaceholderColor = Color.FromArgb("#AAA3B5")
    };

    private readonly Entry variant = new()
    {
        Placeholder = "Разновидность, например 2.5%",
        PlaceholderColor = Color.FromArgb("#AAA3B5")
    };

    private readonly Entry volume = new()
    {
        Placeholder = "Объём / масса, например 0,930 л",
        PlaceholderColor = Color.FromArgb("#AAA3B5")
    };

    private readonly Entry code = new()
    {
        Placeholder = "Штрихкод / код маркировки",
        PlaceholderColor = Color.FromArgb("#AAA3B5")
    };

    private readonly Picker category = new()
    {
        Title = "Категория",
        ItemsSource = Categories,
        SelectedIndex = 5
    };

    private readonly DatePicker expiry = new()
    {
        Date = AppClock.Now.Date.AddDays(7)
    };

    public ProductEditPage(ProductRepository r, Product? product = null)
    {
        repo = r;
        editingProduct = product;

        bool isEditing = editingProduct != null;

        Title = isEditing
            ? "Редактировать продукт"
            : "Добавить продукт";

        if (isEditing)
        {
            name.Text = product!.Name;
            brand.Text = product.Brand;
            variant.Text = product.Variant;
            volume.Text = product.Volume;
            code.Text = product.Barcode;
            expiry.Date = product.ExpiryDate.Date;

            int index = Array.IndexOf(
                Categories,
                product.Category
            );

            category.SelectedIndex = index >= 0 ? index : 5;
        }

        var heading = new Label
        {
            Text = isEditing ? "Изменить продукт" : "Новый продукт",
            FontSize = 26,
            FontAttributes = FontAttributes.Bold
        };

        var expiryLabel = new Label
        {
            Text = "Срок годности",
            FontSize = 16
        };

        var save = new Button
        {
            Text = isEditing
                ? "Сохранить изменения"
                : "Сохранить",

            CornerRadius = 16,
            HeightRequest = 56,
            FontAttributes = FontAttributes.Bold
        };

        save.Clicked += Save;

        var layout = new VerticalStackLayout
        {
            Padding = 22,
            Spacing = 14,
            Children =
            {
                heading,
                name,
                brand,
                variant,
                volume,
                code,
                category,
                expiryLabel,
                expiry,
                save
            }
        };

        Content = new ScrollView
        {
            Content = layout
        };

        // Привязка оформления к текущей теме
        ThemeManager.BindPage(this);

        // Заголовок и обычный текст
        heading.SetDynamicResource(
            Label.TextColorProperty,
            "AppText"
        );

        expiryLabel.SetDynamicResource(
            Label.TextColorProperty,
            "AppText"
        );

        // Поля ввода
        BindEntryTheme(name);
        BindEntryTheme(brand);
        BindEntryTheme(variant);
        BindEntryTheme(volume);
        BindEntryTheme(code);

        // Категория
        category.SetDynamicResource(
            Picker.TextColorProperty,
            "AppText"
        );

        category.SetDynamicResource(
            Picker.BackgroundColorProperty,
            "AppField"
        );

        // Дата окончания срока годности
        expiry.SetDynamicResource(
            DatePicker.TextColorProperty,
            "AppText"
        );

        expiry.SetDynamicResource(
            DatePicker.BackgroundColorProperty,
            "AppField"
        );

        // Кнопка сохранения
        save.SetDynamicResource(
            Button.BackgroundColorProperty,
            "AppAccent"
        );

        save.SetDynamicResource(
            Button.TextColorProperty,
            "AppAccentText"
        );
    }

    private static void BindEntryTheme(Entry entry)
    {
        entry.SetDynamicResource(
            Entry.TextColorProperty,
            "AppText"
        );

        entry.SetDynamicResource(
            Entry.BackgroundColorProperty,
            "AppField"
        );
    }

    private async void Save(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(name.Text))
        {
            await DisplayAlert(
                "Название",
                "Введите название продукта.",
                "ОК"
            );

            return;
        }

        // При редактировании сохраняем существующую запись
        // и её Id. При добавлении создаём новую.
        var product = editingProduct ?? new Product();

        product.Name = name.Text.Trim();
        product.Brand = brand.Text?.Trim() ?? "";
        product.Variant = variant.Text?.Trim() ?? "";
        product.Volume = volume.Text?.Trim() ?? "";
        product.Barcode = code.Text?.Trim() ?? "";
        product.Category =
            category.SelectedItem?.ToString() ?? "Другое";

        product.ExpiryDate = expiry.Date.Date;

        try
        {
            await repo.Save(product);
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Ошибка сохранения",
                ex.Message,
                "ОК"
            );
        }
    }
}
