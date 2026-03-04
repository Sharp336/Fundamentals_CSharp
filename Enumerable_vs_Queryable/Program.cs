using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Simple.OData.Client;

public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== IEnumerable<T> ===");
        DemonstrateEnumerable();

        Console.WriteLine("\n=== IQueryable<T> (EF Core + SQLite in-memory) ===");
        await DemonstrateQueryableAsync();

        Console.WriteLine("\n=== OData remote query ===");
        await DemonstrateODataAsync();

        Console.ReadKey();
    }

    private static void DemonstrateEnumerable()
    {
        var numbers = Enumerable.Range(1, 100);

        Console.WriteLine("Создаём IEnumerable-запрос. До перечисления фильтр ещё не выполняется.");

        IEnumerable<int> query = numbers.Where(n =>
        {
            Console.WriteLine($"Проверяем {n}");
            return n > 95;
        });

        Console.WriteLine("Запрос создан.");
        Console.WriteLine("Берём первые 2 элемента, удовлетворяющие условию:");

        var result = query.Take(2).ToList();

        Console.WriteLine("Результат:");
        foreach (var number in result)
        {
            Console.WriteLine(number);
        }
    }

    private static async Task DemonstrateQueryableAsync()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        if (!await dbContext.Numbers.AnyAsync())
        {
            var entities = Enumerable.Range(1, 100)
                .Select(n => new NumberEntity { Value = n });

            await dbContext.Numbers.AddRangeAsync(entities);
            await dbContext.SaveChangesAsync();
        }

        Console.WriteLine("Создаем IQueryable-запрос.");
        Console.WriteLine("До materialization SQL ещё не выполнялся.");
        IQueryable<NumberEntity> query = dbContext.Numbers
            .Where(x => x.Value > 95)
            .OrderBy(x => x.Value);

        Console.WriteLine("\nSQL, который сгенерирует EF:");
        Console.WriteLine(query.ToQueryString());

        Console.WriteLine("\nТеперь материализуем результат через ToListAsync():");

        var result = await query.ToListAsync();

        foreach (var item in result)
        {
            Console.WriteLine(item.Value);
        }
    }

    private static async Task DemonstrateODataAsync()
    {
        try
        {
            var client = new ODataClient("https://services.odata.org/V4/OData/OData.svc/");

            var products = await client
                .For<Product>()
                .Filter(p => p.Price > 20)
                .OrderBy(p => p.Name)
                .FindEntriesAsync();

            Console.WriteLine("Продукты с Price > 20:");

            foreach (var product in products)
            {
                Console.WriteLine($"{product.Name} - {product.Price}");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("Не удалось выполнить OData-запрос.");
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<NumberEntity> Numbers => Set<NumberEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NumberEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Value).IsRequired();
        });
    }
}

public class NumberEntity
{
    public int Id { get; set; }
    public int Value { get; set; }
}

public class Product
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

/*
Справка по IEnumerable<T>, IQueryable<T>, Expression Tree и OData:

IEnumerable<T>:
   - Базовый интерфейс для последовательностей, которые можно перечислять через foreach
   - Обычно используется для работы с данными, которые уже находятся в памяти
   - LINQ-операции над IEnumerable<T> работают через обычные делегаты Func<...>
   - Фильтрация, сортировка и прочая логика выполняются в .NET-коде, а не на стороне внешнего источника данных

IQueryable<T>:
   - Используется для запросов к внешнему источнику данных через provider
   - LINQ-операции над IQueryable<T> работают через Expression<Func<...>>
   - Запрос не обязан выполняться в памяти: provider может перевести его в SQL, HTTP query, HQL и т.д.

Важно:
   - IQueryable<T> наследуется от IEnumerable<T>
   - Поэтому IQueryable<T> можно присвоить переменной IEnumerable<T>
   - Но после этого дальнейшие LINQ-операции обычно будут вызываться уже как Enumerable.*, а не Queryable.*

Deferred execution (отложенное выполнение):
   - И IEnumerable<T>, и IQueryable<T> часто используют отложенное выполнение
   - Это значит, что сам вызов Where/Select/OrderBy ещё не выполняет запрос
   - Реальное выполнение начинается в момент перечисления или materialization

Когда обычно происходит materialization:
   - ToList()
   - ToArray()
   - ToDictionary()
   - First()/FirstOrDefault()
   - Single()/SingleOrDefault()
   - Count()
   - Any()
   - foreach

Важно:
   - Присваивание IQueryable<T> в IEnumerable<T> само по себе материализацию НЕ вызывает
   - AsEnumerable() тоже сам по себе не материализует данные
   - Он только переключает дальнейшую обработку на LINQ to Objects

Func<T, bool> vs Expression<Func<T, bool>>:
   - Func<T, bool> -> это уже исполняемая функция
   - Expression<Func<T, bool>> -> это описание кода как структуры данных (expression tree)
   - Func можно только выполнить
   - Expression можно анализировать, модифицировать(с оговорками), переводить в SQL/HQL/OData query и т.д.

Expression Tree:
   - Представляет код в виде дерева выражений
   - Полезен для ORM, LINQ provider'ов и динамического построения запросов
   - IQueryable<T> использует именно expression tree, а не обычные делегаты
   - Expression tree immutable: его нельзя изменить "на месте", можно только построить новое дерево на основе старого

Провайдеры и перевод запросов:
   - IQueryable<T> сам запрос не исполняет
   - Он хранит expression tree и передаёт его provider'у
   - Provider уже решает, как выполнить запрос
   - В EF Core provider может перевести выражение в SQL
   - В NHibernate LINQ provider переводит выражение во внутреннее представление/HQL
   - В OData клиент может построить удалённый HTTP-запрос

Важно:
   - Не любой .NET-код внутри Where/Select может быть переведён provider'ом
   - Простые сравнения, логические операции, Contains, StartsWith и т.д. обычно переводимы
   - Пользовательские методы и сложная логика часто уже не переводятся

Материализация и повторное выполнение:
   - Если несколько раз вызвать Count(), First(), ToList() и т.д. над одним и тем же IQueryable<T>, provider может несколько раз выполнить запрос
   - Поэтому если результат нужен несколько раз, его часто имеет смысл материализовать один раз и дальше работать уже с коллекцией в памяти

IQueryable<T> не равен "обязательно SQL":
   - IQueryable<T> это только абстракция над запросом
   - SQL появляется только если provider умеет его генерировать
   - Не каждый IQueryable<T> связан с реляционной БД
   - Например, EF Core InMemory provider не подходит для честной демонстрации SQL

OData:
   - OData (Open Data Protocol) - это стандарт для queryable REST API
   - Позволяет описывать фильтрацию, сортировку, выбор полей и навигацию через URL/query options
   - Часто используются:
       $filter
       $select
       $expand
       $orderby
       $top
       $skip
*/