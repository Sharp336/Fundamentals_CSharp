// Ковариантность
public interface IReadOnlyCollection<out T>
{
    T Get(int index);
    int Count { get; }
}

// Контрвариантность
public interface IService<in T>
{
    void ChangeCoil(T item);
}

public class ReadOnlyCollection<T> : IReadOnlyCollection<T>
{
    private readonly List<T> _items;

    public ReadOnlyCollection(List<T> items)
    {
        _items = items;
    }

    public T Get(int index)
    {
        return _items[index];
    }

    public int Count => _items.Count;
}

public class VapeService : IService<Vape>
{
    public void ChangeCoil(Vape puf)
    {
        Console.WriteLine($"Обслужен испаритель {puf.Model}");
    }
}

public class Vape
{
    public string Model { get; set; }
}

public class Podik : Vape
{
    public bool IsDisposable { get; set; }
}

public class Program
{
    public static void Main()
    {
        // Ковариантность
        List<Podik> Podiks = new List<Podik>
            {
                new Podik { Model = "HQD", IsDisposable = true },
                new Podik { Model = "Pasito", IsDisposable = false }
            };

        IReadOnlyCollection<Vape> VapeCollection = new ReadOnlyCollection<Podik>(Podiks);

        for (int i = 0; i < VapeCollection.Count; i++)
        {
            Vape puf = VapeCollection.Get(i);
            Console.WriteLine($"Vape from collection: {puf.Model}"); // HQD, Pasito
        }

        // Контрвариантность
        IService<Podik> PodService = new VapeService();
        PodService.ChangeCoil(new Podik { Model = "Knight", IsDisposable = false }); // Обслужен испаритель Knight

        Console.ReadKey();
    }
}

/*
Справка по дженерикам:

Ковариантность (out):
   - Позволяет использовать производные типы вместо базовых.
   - Применяется к интерфейсам и делегатам.
   - Пример: IReadOnlyRepository<out T>

Контрвариантность (in):
   - Позволяет использовать базовые типы вместо производных.
   - Применяется к интерфейсам и делегатам.
   - Пример: IWriteOnlyRepository<in T>

Обычные дженерики:
   - Позволяют создавать универсальные классы, методы и интерфейсы.
   - Пример: public class Repository<T>

Атрибуты для дженериков:
   - GenericVarianceAttribute (ковариантность и контрвариантность).
   - GenericTypeParametersConstraintAttribute (ограничения типа параметра).

Элементы кода, к которым могут применяться дженерики:
   - Классы
   - Методы
   - Интерфейсы
   - Делегаты
   - Структуры
   - Записи
*/
