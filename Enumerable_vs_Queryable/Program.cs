using Microsoft.EntityFrameworkCore;
using Simple.OData.Client;

public class MyDbContext : DbContext
{
    public DbSet<Number> Numbers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("TestDb");
    }
}

public class Number
{
    public int Id { get; set; }
    public int Value { get; set; }
}

public class Product
{
    public int ID { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        DemonstrateIEnumerable();
        DemonstrateIQueryable();
        await DemonstrateOData();
        Console.ReadKey();
    }

    private static void DemonstrateIEnumerable()
    {
        List<int> numbers = Enumerable.Range(1, 100).ToList();
        IEnumerable<int> enumerableNumbers = numbers;

        // Запрос фильтрации выполняется в памяти
        var filteredNumbers = enumerableNumbers.Where(n => n > 90).ToList();

        Console.WriteLine("IEnumerable<T>:");
        foreach (var number in filteredNumbers)
        {
            Console.WriteLine(number);
        }
        Console.WriteLine();
    }

    private static void DemonstrateIQueryable()
    {
        using (var dbContext = new MyDbContext())
        {
            if (!dbContext.Numbers.Any())
            {
                dbContext.Numbers.AddRange(Enumerable.Range(1, 100).Select(n => new Number { Value = n }));
                dbContext.SaveChanges();
            }

            IQueryable<int> queryableNumbers = dbContext.Numbers.Select(n => n.Value);

            // Запрос фильтрации выполняется на стороне БД
            var filteredNumbers = queryableNumbers.Where(n => n > 90).ToList();

            Console.WriteLine("IQueryable<T>:");
            foreach (var number in filteredNumbers)
            {
                Console.WriteLine(number);
            }

            var sql = queryableNumbers.Where(n => n > 90).ToQueryString();
            Console.WriteLine("Сгенерированный SQL запрос:");
            Console.WriteLine(sql + '\n');
        }
    }

    private static async Task DemonstrateOData()
    {
        var client = new ODataClient("https://services.odata.org/V4/OData/OData.svc/");

        // Запрос фильтрации выполняется на стороне OData сервера
        var products = await client.For<Product>().Filter(p => p.Price > 20).FindEntriesAsync();

        Console.WriteLine("OData IQueryable<T>:");
        foreach (var product in products)
        {
            Console.WriteLine($"{product.Name} - ${product.Price}");
        }
        Console.WriteLine();
    }
}

/*
    Разница между IEnumerable<T> и IQueryable<T>:
    1. IEnumerable<T>:
        - Интерфейс находится в пространстве имен System.Collections.
        - Используется для работы с данными, загруженными в память (in-memory collections).
        - Методы расширения принимают делегаты (например, Func<T, bool>).
        - Пример метода: 
            public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        - Все операции выполняются в памяти после загрузки данных.

    2. IQueryable<T>:
        - Интерфейс находится в пространстве имен System.Linq.
        - Используется для работы с данными из внешних источников (например, базы данных).
        - Методы расширения принимают выражения (Expression<Func<T, bool>>), что позволяет строить деревья выражений.
        - Пример метода:
            public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        - Операции передаются провайдеру данных для выполнения (например, переводятся в SQL для выполнения на сервере базы данных).
*/