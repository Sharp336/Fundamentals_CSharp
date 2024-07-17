class Program
{
    public class Buyer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Shopping
    {
        public int Id { get; set; }
        public decimal Summa { get; set; }
        public int BuyerId { get; set; }
    }

    static void Main(string[] args)
    {
        List<Buyer> buyers = new List<Buyer>
        {
            new Buyer { Id = 1, Name = "Покупатель А" },
            new Buyer { Id = 2, Name = "Покупатель Б" },
            new Buyer { Id = 3, Name = "Покупатель В" }
        };

        List<Shopping> shoppingList = new List<Shopping>
        {
            new Shopping { Id = 1, Summa = 111.1m, BuyerId = 1 },
            new Shopping { Id = 2, Summa = 222m, BuyerId = 1 },
            new Shopping { Id = 3, Summa = 333m, BuyerId = 2 },
            new Shopping { Id = 4, Summa = 444.4m, BuyerId = 3 },
            new Shopping { Id = 5, Summa = 555m, BuyerId = 3 }
        };

        var bestBuyerName = shoppingList
            .GroupBy(s => s.BuyerId)
            .Select(group => new
            {
                BuyerId = group.Key,
                TotalSum = group.Sum(s => s.Summa)
            })
            .OrderByDescending(bg => bg.TotalSum)
            .Join(buyers, bg => bg.BuyerId, b => b.Id, (bg, b) => b.Name)
            .FirstOrDefault();

        //var bestBuyerName =
        //    (from shopping in shoppingList
        //     group shopping by shopping.BuyerId into buyerGroup
        //     let totalSum = buyerGroup.Sum(s => s.Summa)
        //     orderby totalSum descending
        //     join buyer in buyers on buyerGroup.Key equals buyer.Id
        //     select buyer.Name).FirstOrDefault();

        Console.WriteLine(bestBuyerName != null ? $"Лучший покупатель: {bestBuyerName}" : "Нет данных о покупателях.");

        Console.ReadKey();
    }
}

/* Справка по LINQ
LINQ (Language-Integrated Query) — язык запросов для работы с данными различных типов.

Разновидности LINQ:
1. LINQ to Objects — для работы с коллекциями и массивами.
2. LINQ to Entities — для работы с базами данных через Entity Framework.
3. LINQ to XML — для работы с XML документами.
4. LINQ to DataSet — для работы с DataSet.
5. Parallel LINQ (PLINQ) — для выполнения параллельных запросов.

   Примеры:
string[] people = { "Tom", "Bob", "Sam", "Tim", "Tomas", "Bill" };

// Операторы запросов LINQ
var selectedPeople = from p in people
                     where p.ToUpper().StartsWith("T")
                     orderby p
                     select p;

// Методы расширения LINQ
var selectedPeople = people.Where(p => p.ToUpper().StartsWith("T")).OrderBy(p => p);

foreach (string person in selectedPeople)
    Console.WriteLine(person);
 

   Методы LINQ:
Отложенное выполнение:
1. Select: проекция значений.
2. Where: фильтрация.
3. OrderBy/OrderByDescending: сортировка.
4. ThenBy/ThenByDescending: дополнительные критерии сортировки.
5. Join: соединение коллекций.
6. GroupBy: группировка.
7. ToLookup: группировка в словарь.
8. Aggregate: агрегатная функция.
9. Reverse: обратный порядок.
10. All: все ли элементы удовлетворяют условию.
11. Any: есть ли элемент, удовлетворяющий условию.
12. Contains: содержит ли коллекция элемент.
13. Distinct: удаление дубликатов.
14. Except: разность коллекций.
15. Union: объединение коллекций.
16. Intersect: пересечение коллекций.
17. Skip/Take: пропуск/выборка элементов.
18. SkipWhile/TakeWhile: пропуск/выборка, пока условие истинно.
19. Concat: объединение коллекций.
20. Zip: объединение коллекций по условию.

Немедленное выполнение:
1. Count: количество элементов.
2. Sum: сумма элементов.
3. Average: среднее значение.
4. Min/Max: минимальное/максимальное значение.
5. First/FirstOrDefault: первый элемент или значение по умолчанию.
6. Single/SingleOrDefault: единственный элемент или значение по умолчанию.
7. ElementAt/ElementAtOrDefault: элемент по индексу или значение по умолчанию.
8. Last/LastOrDefault: последний элемент или значение по умолчанию.
9. ToArray/ToList/ToDictionary: преобразование в массив/список/словарь.

Отложенное и немедленное выполнение:
Отложенное выполнение:
var selectedPeople = people.Where(s => s.Length == 3).OrderBy(s => s);
foreach (string s in selectedPeople)
    Console.WriteLine(s);

Немедленное выполнение:
var count = people.Where(s => s.Length == 3).OrderBy(s => s).Count();
Console.WriteLine(count);

Parallel LINQ (PLINQ):
1. AsParallel: распараллеливает запрос.
   var squares = numbers.AsParallel().Select(x => x * x);

2. ForAll: выполняет действие для каждого элемента параллельно.
   numbers.AsParallel().Select(n => n * n).ForAll(Console.WriteLine);

3. AsOrdered/AsUnordered: упорядочивание результатов.
   var squares = numbers.AsParallel().AsOrdered().Select(n => n * n);

4. WithCancellation: прерывание операции с использованием CancellationToken.
   var cts = new CancellationTokenSource();
   var squares = numbers.AsParallel().WithCancellation(cts.Token).Select(n => n * n);

Ошибки в PLINQ обрабатываются через AggregateException.
object[] numbers = { 1, 2, 3, 4, 5, "6" };
try
{
    var squares = numbers.AsParallel().Select(n => (int)n * (int)n);
    squares.ForAll(Console.WriteLine);
}
catch (AggregateException ex)
{
    foreach (var e in ex.InnerExceptions)
        Console.WriteLine(e.Message);
}

Прерывание операций через CancellationToken.
var cts = new CancellationTokenSource();
var task = Task.Run(() =>
{
    Thread.Sleep(400);
    cts.Cancel();
});

try
{
    int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8 };
    var squares = numbers.AsParallel().WithCancellation(cts.Token).Select(n => n * n);
    squares.ForAll(Console.WriteLine);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Операция была прервана");
}
*/
