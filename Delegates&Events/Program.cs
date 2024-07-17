public class Program
{
    // Псевдо-бесконечный список чисел
    public static IEnumerable<int> GetPositiveIntNumbers()
    {
        int num = 0;
        while (num <= int.MaxValue) yield return num++;
    }

    // Ленивая выборка четных чисел
    public static IEnumerable<int> SelectEvenNumbers(IEnumerable<int> numbers)
    {
        foreach (var num in numbers)
        {
            if (num % 2 == 0) yield return num;
        }
    }

    // Вызов мультикаст делегата к элементам списка
    public static void InvokeDelegateForNumbers(IEnumerable<int> numbers, Action<int> action, int limit)
    {
        int count = 0;
        foreach (var number in numbers)
        {
            if (count++ >= limit) break;
            action(number);
        }
    }

    public static void Main()
    {
        var aLotOfNumbers = GetPositiveIntNumbers();

        var evenNumbers = SelectEvenNumbers(aLotOfNumbers);

        Action<int> numberActions = num => Console.WriteLine($"Взято число: {num}");
        numberActions += num => Console.WriteLine($"Его квадрат: {num * num}");

        InvokeDelegateForNumbers(evenNumbers, numberActions, limit: 10);

        Console.ReadKey();
    }
    


}


/*
Справка по делегатам и событиям:

Делегаты - типы, которые могут ссылаться на методы с определенной сигнатурой. Они могут быть переданы 
в качестве параметров другим методам и позволяют вызывать методы через экземпляры делегатов.

В C# имеются три основных встроенных делегата: 

   Func – который принимает от 0 до 16 входных параметров и возвращает значение.
   Возвращаемый тип всегда должен быть указан последним.
    Func<int, int, int> add = (x, y) => x + y;

   Action – принимает от 1 до 16 входных параметров и не возвращает значения.
    Action<string> showMessage = msg => Console.WriteLine(msg);

   Predicate – принимает один параметр и возвращает значение типа bool. Используется для проверки условий.
    Predicate<string> isNumeric = str =>
    {
        double retNum;
        return double.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
    };

События основаны на делегатах, но предоставляют механизм для публикации и подписки на действия. Они обеспечивают 
инкапсуляцию, позволяя подписчикам добавлять и удалять свои обработчики, но вызываются только изнутри класса, где они объявлены.

Когда подписчик подписывается на событие, создается ссылка на метод-обработчик этого подписчика. 
Если издатель события имеет более длительный жизненный цикл, чем подписчик, то сборщик мусора не сможет 
освободить память, занятую подписчиком, так как на него по-прежнему будет ссылка от издателя.

Для решения проблемы есть несколько методов:

    Явная отписка от событий
    Отписка от события после его вызова
    Отписка от события в финализаторе(сомнительно)
    Использование WeakEventManager
    Использование ConditionalWeakTable
    Обертка со слабой ссылкой

пример:

sealed class EventWrapper
{
    SourceObject eventSource;
    WeakReference wr;

    public EventWrapper(SourceObject eventSource, ListenerObject obj)
    {
        this.eventSource = eventSource;
        this.wr = new WeakReference(obj);
        eventSource.Event += OnEvent;
    }

    void OnEvent(object sender, EventArgs e)
    {
        ListenerObject obj = (ListenerObject)wr.Target;
        if (obj != null)
            obj.OnEvent(sender, e);
        else
            Deregister();
    }

    public void Deregister()
    {
        eventSource.Event -= OnEvent;
    }
}

void RegisterEvent()
{
    EventWrapper ew = new EventWrapper(eventSource, this);
}

void OnEvent(object sender, EventArgs e)
{
    // Обработка события
}

Для безопасного вызова события (во избежание теоретически возможного NullReferenceException) - OnChange?.Invoke(this, EventArgs.Empty);

При объявлении field-like события компилятор сам делает его потокобезопасным (то-ли через lock, то-ли через Interlocked.CompareExchange),
если самому реализовывать add и remove, то и безопасность на тебе.


*/