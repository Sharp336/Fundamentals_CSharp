class Program
{
    static SynchronizationContext context;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Пример использования контекста синхронизации\n");

        // Сохранение текущего контекста синхронизации
        context = SynchronizationContext.Current ?? new SynchronizationContext();
        Console.WriteLine("Изначальный поток при сохранении контекста. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);

        // Запуск демонстрации асинхронного метода с использованием контекста синхронизации
        await DemonstrateSynchronizationContextMethods();

        Console.WriteLine("Все задачи завершены.");
        Console.ReadLine();
    }

    // Контекст синхронизации используется для управления выполнением потоков в различных средах.
    // В данном методе демонстрируются различные методы контекста синхронизации.
    static async Task DemonstrateSynchronizationContextMethods()
    {
        Console.WriteLine("Основной поток до асинхронного вызова. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);

        // Асинхронное выполнение в фоновом потоке
        await Task.Run(() =>
        {
            Console.WriteLine("Выполнение в фоновом потоке. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(1000);  // Симуляция работы

            // Синхронное выполнение кода в контексте синхронизации
            context.Send(_ =>
            {
                Console.WriteLine("Синхронное выполнение в основном потоке через Send. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
            }, null);

            // Асинхронное выполнение кода в контексте синхронизации
            context.Post(_ =>
            {
                Console.WriteLine("Асинхронное выполнение в основном потоке через Post. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
            }, null);
        });

        Console.WriteLine("Основной поток после асинхронного вызова. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);

        // Использование Task.Yield для возврата управления в основной поток и демонстрации продолжения в основном потоке
        await Task.Yield();
        Console.WriteLine("Возвращение в основной поток после Task.Yield. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);

        // Асинхронное выполнение кода с использованием метода Post
        context.Post(async _ =>
        {
            Console.WriteLine("Асинхронное выполнение через Post перед Task.Delay. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
            await Task.Delay(1000);  // Симуляция асинхронной работы
            Console.WriteLine("Асинхронное выполнение через Post после Task.Delay. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
        }, null);

        // Небольшая задержка, чтобы асинхронные операции успели завершиться
        await Task.Delay(2000);

        Console.WriteLine("Демонстрация контекста синхронизации завершена. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
    }
}

/*
Основные аспекты SynchronizationContext:
- Передача задач между потоками: SynchronizationContext позволяет передавать единицы работы (делегаты) между различными потоками. 
 Это критично для программ, где требуется выполнение задач в определенном контексте, например, в UI потоках.
- Контекст потока: Каждый поток может иметь текущий контекст SynchronizationContext. 
 Этот контекст может быть общим между несколькими потоками.
- Учет асинхронных операций: SynchronizationContext ведет учет незавершенных асинхронных операций, 
 что особенно полезно в ASP.NET для отслеживания выполнения запросов.

Реализации SynchronizationContext:
- Default SynchronizationContext(он же ThreadPool) (mscorlib.dll: System.Threading): Используется по умолчанию и ставит задачи 
 в очередь ThreadPool. Асинхронные делегаты выполняются в потоке пула, а синхронные — в вызывающем потоке.
- AspNetSynchronizationContext: В ASP.NET этот контекст обеспечивает выполнение делегатов с восстановлением идентичности и культуры запроса.
- WindowsFormsSynchronizationContext: Используется в приложениях Windows Forms для управления делегатами в UI потоке.
- DispatcherSynchronizationContext: Используется в WPF и Silverlight для управления задачами в UI потоке через диспетчер.

Реализация System.Threading.SynchronizationContext:
SynchronizationContext — это абстрактный класс, предоставляющий базовую функциональность для синхронизации в различных средах выполнения. 
Его можно расширять и создавать специфические контексты синхронизации для различных фреймворков и приложений.
Ключевые методы SynchronizationContext включают:
- Post(SendOrPostCallback d, object state): Асинхронно отправляет делегат в очередь контекста.
- Send(SendOrPostCallback d, object state): Синхронно отправляет делегат в очередь контекста и выполняет его.
- OperationStarted(): Увеличивает счетчик незавершенных операций.
- OperationCompleted(): Уменьшает счетчик незавершенных операций.
- Current: Статическое свойство, возвращающее текущий SynchronizationContext для потока.
- SetSynchronizationContext(SynchronizationContext syncContext): Статический метод, устанавливающий текущий SynchronizationContext для потока.

Примеры использования:
- AsyncOperationManager и AsyncOperation: Эти классы облегчают работу с асинхронными операциями, оборачивая SynchronizationContext.
- Task Parallel Library (TPL): Предоставляет TaskScheduler, который может использовать SynchronizationContext для постановки задач в очередь.
- Microsoft Reactive Extensions (Rx): Использует SynchronizationContext для обработки событий.

Замечания по реализации:
- Порядок выполнения: Не все реализации гарантируют порядок выполнения делегатов. Например, контекст пользовательского интерфейса 
 обеспечивает порядок, в то время как контекст по умолчанию — нет.
- Отсутствие прямого соответствия потокам: Контексты синхронизации не всегда сопоставлены одному потоку. Некоторые реализации, 
 такие как WindowsFormsSynchronizationContext, имеют 1:1 соответствие, но другие могут работать с несколькими потоками.
- Асинхронность метода Post: Метод Post в разных реализациях может работать синхронно или асинхронно. Например, AspNetSynchronizationContext выполняет делегаты синхронно.

SynchronizationContext предоставляет средства для написания компонент, которые могут работать в разных фреймворках. 
BackgroundWorker и WebClient - это два примера, которые одинаково хорошо работают в Windows Forms, WPF, Silverlight, консоли и ASP.NET приложениях.
*/

