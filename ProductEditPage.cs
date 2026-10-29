
using FoodExpiryApp.Models;

namespace FoodExpiryApp;

public class ProductEditPage : ContentPage
{
    private readonly ProductRepository repo;
    private readonly Product? editingProduct;

    private readonly Entry name = new()
    {
        Placeholder = "Название",
        TextColor = Colors.White,
        PlaceholderColor = Color.FromArgb("#AAA3B5"),
        BackgroundColor = Color.FromArgb("#25232C")
    };

    private readonly Entry brand = new()
    {
        Placeholder = "Бренд",
        TextColor = Colors.White,
        PlaceholderColor = Color.FromArgb("#AAA3B5"),
        BackgroundColor = Color.FromArgb("#25232C")
    };

    private readonly Entry variant = new()
    {
        Placeholder = "Разновидность, например 2.5%",
        TextColor = Colors.White,
        PlaceholderColor = Color.FromArgb("#AAA3B5"),
        BackgroundColor = Color.FromArgb("#25232C")
    };

    private readonly Entry volume = new()
    {
        Placeholder = "Объём / масса, например 0,930 л",
        TextColor = Colors.White,
        PlaceholderColor = Color.FromArgb("#AAA3B5"),
        BackgroundColor = Color.FromArgb("#25232C")
    };

    private readonly Entry code = new()
    {
        Placeholder = "Штрихкод / код маркировки",
        TextColor = Colors.White,
        PlaceholderColor = Color.FromArgb("#AAA3B5"),
        BackgroundColor = Color.FromArgb("#25232C")
    };

    private readonly Picker category = new()
    {
        Title = "Категория",
        TextColor = Colors.White,
        BackgroundColor = Color.FromArgb("#25232C"),
        ItemsSource = new[]
        {
            "Молочные",
            "Напитки",
            "Мясо и рыба",
            "Овощи и фрукты",
            "Заморозка",
            "Другое"
        },
        SelectedIndex = 5
    };

    private readonly DatePicker expiry = new()
    {
        Date = DateTime.Today.AddDays(7),
        TextColor = Colors.White,
        BackgroundColor = Color.FromArgb("#25232C")
    };

    public ProductEditPage(ProductRepository r, Product? product = null)
    {
        repo = r;
        editingProduct = product;

        bool isEditing = editingProduct != null;

        Title = isEditing ? "Редактировать продукт" : "Добавить продукт";
        BackgroundColor = Color.FromArgb("#111016");

        if (isEditing)
        {
            name.Text = product!.Name;
            brand.Text = product.Brand;
            variant.Text = product.Variant;
            volume.Text = product.Volume;
            code.Text = product.Barcode;
            expiry.Date = product.ExpiryDate.Date;

            int index = Array.IndexOf(
                (string[])category.ItemsSource,
                product.Category);

            category.SelectedIndex = index >= 0 ? index : 5;
        }

        var heading = new Label
        {
            Text = isEditing ? "Изменить продукт" : "Новый продукт",
            FontSize = 26,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };

        var save = new Button
        {
            Text = isEditing ? "Сохранить изменения" : "Сохранить",
            BackgroundColor = Color.FromArgb("#A978F5"),
            TextColor = Colors.White,
            CornerRadius = 16,
            HeightRequest = 56,
            FontAttributes = FontAttributes.Bold
        };

        save.Clicked += Save;

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
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
                    new Label
                    {
                        Text = "Срок годности",
                        TextColor = Colors.White,
                        FontSize = 16
                    },
                    expiry,
                    save
                }
            }
        };
    }

    private async void Save(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(name.Text))
        {
            await DisplayAlert(
                "Название",
                "Введите название продукта.",
                "ОК");
            return;
        }

        // При редактировании используем существующую запись,
        // сохраняя её Id. При добавлении создаём новую.
        var product = editingProduct ?? new Product();

        product.Name = name.Text.Trim();
        product.Brand = brand.Text?.Trim() ?? "";
        product.Variant = variant.Text?.Trim() ?? "";
        product.Volume = volume.Text?.Trim() ?? "";
        product.Barcode = code.Text?.Trim() ?? "";
        product.Category = category.SelectedItem?.ToString() ?? "Другое";
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
                "ОК");
        }
    }
}
