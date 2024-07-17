class Program
{
    static void Main(string[] args)
    {
        var numbers = Enumerable.Range(1, 1000000).ToList();

        var filteredNumbersParallel = FilterAndCollectParallel(numbers, IsPrime);
        Console.WriteLine($"Parallel.ForEach: Найдено {filteredNumbersParallel.Count} простых чисел.");

        var filteredNumbersPLINQ = FilterAndCollectPLINQ(numbers, IsPrime);
        Console.WriteLine($"PLINQ: Найдено {filteredNumbersPLINQ.Count} простых чисел.");

        Console.ReadKey();
    }

    public static List<int> FilterAndCollectParallel(IEnumerable<int> source, Func<int, bool> filter)
    {
        var results = new List<int>();
        var lockObject = new object();

        Parallel.ForEach(
            source,
            () => new List<int>(),
            (item, loopState, localList) =>
            {
                if (filter(item))
                {
                    localList.Add(item);
                }
                return localList;
            },
            finalList =>
            {
                lock (lockObject)
                {
                    results.AddRange(finalList);
                }
            });

        return results;
    }

    public static List<int> FilterAndCollectPLINQ(IEnumerable<int> source, Func<int, bool> filter)
    {
        return source
            .AsParallel()
            .Where(filter)
            .ToList();
    }

    public static bool IsPrime(int number)
    {
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;
        var boundary = (int)Math.Floor(Math.Sqrt(number));

        for (int i = 3; i <= boundary; i += 2)
        {
            if (number % i == 0)
                return false;
        }

        return true;
    }
}

/*
 * Parallel.ForEach vs .AsParallel:
 *
 * Parallel.ForEach:
 * - Parallel.ForEach является многопоточной версией обычного цикла foreach.
 * - Позволяет задавать локальное состояние для каждого потока, что снижает накладные расходы на синхронизацию.
 * - Удобен для длительных вычислений, результаты которых независимы друг от друга.
 * - Поддерживает настройку максимального количества потоков через ParallelOptions.
 *
 * PLINQ .AsParallel:
 * - PLINQ является расширением LINQ для параллельной обработки данных.
 * - Использует метод AsParallel() для выполнения последующих операций параллельно.
 * - Обеспечивает автоматическое управление параллельностью, упрощая код.
 * - Удобен для операций, требующих сохранения порядка элементов, с использованием метода AsOrdered().
 * - Поддерживает ленивую материализацию данных, что особенно полезно для потоковой обработки.
 *
 * Основные различия:
 * - Parallel.ForEach позволяет более точно управлять параллельным выполнением и синхронизацией, что полезно в сложных сценариях.
 * - PLINQ обеспечивает более простой и лаконичный код для параллельных операций над данными, но может иметь накладные расходы на управление параллельностью.
 */

