using System.Collections.Concurrent;

class Program
{
    static void Main(string[] args)
    {
        var numbers = Enumerable.Range(1, 1000000).ToList();

        // Пример использования различных стратегий секционирования с Parallel.ForEach
        var filteredNumbersParallel = FilterAndCollectParallel(numbers, IsPrime);
        Console.WriteLine($"Parallel.ForEach: Найдено {filteredNumbersParallel.Count} простых чисел.");

        // Пример использования различных стратегий секционирования с PLINQ
        var filteredNumbersPLINQ = FilterAndCollectPLINQ(numbers, IsPrime);
        Console.WriteLine($"PLINQ: Найдено {filteredNumbersPLINQ.Count} простых чисел.");

        Console.ReadKey();
    }

    public static List<int> FilterAndCollectParallel(IEnumerable<int> source, Func<int, bool> filter)
    {
        var results = new ConcurrentBag<int>();

        // Использование Static Range Partitioning
        var rangePartitioner = Partitioner.Create(0, source.Count());
        Parallel.ForEach(rangePartitioner, range =>
        {
            for (int i = range.Item1; i < range.Item2; i++)
            {
                var item = source.ElementAt(i);
                if (filter(item))
                {
                    results.Add(item);
                }
            }
        });

        // Использование Dynamic Range Partitioning (Chunk Partitioning)
        var chunkPartitioner = Partitioner.Create(source, EnumerablePartitionerOptions.NoBuffering);
        Parallel.ForEach(chunkPartitioner, item =>
        {
            if (filter(item))
            {
                results.Add(item);
            }
        });

        // Использование Custom Partitioner
        var customPartitioner = new CustomPartitioner(source.ToList());
        Parallel.ForEach(customPartitioner, item =>
        {
            if (filter(item))
            {
                results.Add(item);
            }
        });

        return results.ToList();
    }

    public static List<int> FilterAndCollectPLINQ(IEnumerable<int> source, Func<int, bool> filter)
    {
        List<int> results;

        // Использование стандартного PLINQ
        results = source.AsParallel().Where(filter).ToList();

        // Использование секционирования блоками (Chunk Partitioning) в PLINQ
        results = Partitioner.Create(source, EnumerablePartitionerOptions.NoBuffering)
                             .AsParallel()
                             .Where(filter)
                             .ToList();

        // Использование секционирования по диапазону (Range Partitioning) в PLINQ
        results = (source.ToList()).AsParallel()
                     .Where(filter)
                     .ToList();

        // Использование секционирования хешей (Hash Partitioning) в PLINQ
        results = source.AsParallel()
                        .GroupBy(x => x % Environment.ProcessorCount)
                        .SelectMany(group => group.Where(filter))
                        .ToList();

        return results;
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

public class CustomPartitioner : Partitioner<int>
{
    private readonly IList<int> _data;

    public CustomPartitioner(IList<int> data)
    {
        _data = data;
    }

    public override bool SupportsDynamicPartitions => true;

    public override IList<IEnumerator<int>> GetPartitions(int partitionCount)
    {
        var partitions = new List<IEnumerator<int>>(partitionCount);
        for (int i = 0; i < partitionCount; i++)
        {
            partitions.Add(GetPartitionEnumerator(i, partitionCount));
        }
        return partitions;
    }

    private IEnumerator<int> GetPartitionEnumerator(int partitionIndex, int partitionCount)
    {
        for (int i = partitionIndex; i < _data.Count; i += partitionCount)
        {
            yield return _data[i];
        }
    }

    public override IEnumerable<int> GetDynamicPartitions()
    {
        return _data;
    }
}

/*
 * Parallel.ForEach vs .AsParallel:
 *
 * Parallel.ForEach:
 * - Parallel.ForEach является многопоточной версией обычного цикла foreach.
 * - Позволяет задавать локальное состояние для каждого потока, что снижает накладные расходы на синхронизацию.
 * - Поддерживает настройку максимального количества потоков через ParallelOptions.
 * - Использует Partitioner для разбиения данных на части, обеспечивая равномерное распределение нагрузки.
 * - Подходит для задач, где результаты независимы друг от друга и требуется высокий контроль над параллелизмом.
 *
 * Partitioner:
 * - Static Range Partitioning: Делит данные на фиксированные диапазоны, которые назначаются потокам для обработки.
 * - Dynamic Range Partitioning (Chunk Partitioning): Делит данные на небольшие динамические блоки, которые распределяются между потоками по мере готовности.
 * - Custom Partitioner: Позволяет создавать собственные стратегии разбиения данных.
 *
 * PLINQ .AsParallel:
 * - PLINQ является расширением LINQ для параллельной обработки данных.
 * - Использует метод AsParallel() для выполнения последующих операций параллельно.
 * - Обеспечивает автоматическое управление параллельностью, упрощая код.
 * - Удобен для операций, требующих сохранения порядка элементов, с использованием метода AsOrdered().
 * - Поддерживает ленивую материализацию данных, что особенно полезно для потоковой обработки.
 * - Использует автоматическое секционирование данных (chunk, range, hash partitioning) для распределения нагрузки.
 *
 * Основные различия:
 * - Parallel.ForEach позволяет задавать локальное состояние для каждого потока и управлять количеством параллельных потоков, а также использовать кастомные стратегии разбиения данных через Partitioner.
 * - PLINQ обеспечивает более простой и лаконичный код для параллельных операций над данными, но может иметь накладные расходы на управление параллельностью.
 *
 * Partitioning in PLINQ:
 * - Range Partitioning: Делит данные на фиксированные диапазоны и назначает их потокам. Подходит для индексируемых источников данных (списки, массивы).
 * - Chunk Partitioning: Рабочие потоки запрашивают данные порциями, подходит для неиндексируемых источников данных (IEnumerable).
 * - Striped Partitioning: Используется для операторов SkipWhile и TakeWhile, оптимизирован для обработки элементов в начале источника данных.
 * - Hash Partitioning: Используется для операторов, требующих сравнения элементов (Join, GroupBy, Distinct и т.д.), распределяет элементы по хэшам.
 */
