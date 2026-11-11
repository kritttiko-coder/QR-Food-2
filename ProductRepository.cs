using SQLite;
using FoodExpiryApp.Models;

namespace FoodExpiryApp;

public class ProductRepository
{
    private readonly SQLiteAsyncConnection db;
    private bool _isInitialized = false;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public ProductRepository()
    {
        // 1. СМЕНИЛИ ИМЯ БАЗЫ (v2), чтобы сбросить старую схему
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "foodexpiry_v2.db3");
        
        db = new SQLiteAsyncConnection(dbPath, 
            SQLiteOpenFlags.ReadWrite | 
            SQLiteOpenFlags.Create | 
            SQLiteOpenFlags.SharedCache);
    }

    // Метод инициализации, который безопасно создает таблицы
    private async Task InitAsync()
    {
        if (_isInitialized) return;
        
        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized) return;
            
            // Создаем таблицу (если нет) с новыми колонками
            await db.CreateTableAsync<Product>();
            
            // Заполняем тестовыми данными
            await SeedAsync();
            
            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task SeedAsync()
    {
        if (await db.Table<Product>().CountAsync() > 0) return;
        
        await db.InsertAllAsync(new[]
        {
            new Product{Name="Молоко",Brand="Простоквашино",Variant="2.5%",Category="Молочные",Volume="0,930 л",ExpiryDate=DateTime.Today.AddDays(6)},
            new Product{Name="Молоко",Brand="Простоквашино",Variant="2.5%",Category="Молочные",Volume="0,930 л",ExpiryDate=DateTime.Today.AddDays(18)},
            new Product{Name="Вода",Brand="Байкальская",Category="Напитки",Volume="0,5 л",ExpiryDate=DateTime.Today.AddDays(22)}
        });
    }

    // 2. Все методы теперь асинхронные и сначала вызывают InitAsync
    public async Task<List<Product>> All()
    {
        await InitAsync();
        return await db.Table<Product>().OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<int> Save(Product p)
    {
        await InitAsync();
        return p.Id == 0 ? await db.InsertAsync(p) : await db.UpdateAsync(p);
    }

    public async Task<int> Delete(Product p)
    {
        await InitAsync();
        return await db.DeleteAsync(p);
    }
}
