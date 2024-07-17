using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private static readonly object lockObject = new object();
    private static Mutex mutex = new Mutex();
    private static Semaphore semaphore = new Semaphore(3, 3);
    private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);
    private static ManualResetEvent manualResetEvent = new ManualResetEvent(false);
    private static CountdownEvent countdownEvent = new CountdownEvent(3);
    private static Barrier barrier = new Barrier(3, (b) =>
    {
        Console.WriteLine("All threads have reached the barrier.");
    });

    static async Task Main(string[] args)
    {
        var tasks = new[]
        {
            Task.Run(() => DemonstrateLock()),
            Task.Run(() => DemonstrateMutex()),
            Task.Run(() => DemonstrateSemaphore()),
            Task.Run(() => DemonstrateAutoResetEvent(1)),
            Task.Run(() => DemonstrateAutoResetEvent(2)),
            Task.Run(() => DemonstrateManualResetEvent(1)),
            Task.Run(() => DemonstrateManualResetEvent(2)),
            Task.Run(() => DemonstrateCountdownEvent()),
            Task.Run(() => DemonstrateBarrier())
        };

        // Даем немного времени задачам на запуск и ожидание сигналов
        await Task.Delay(1000);

        // Отправка сигнала для AutoResetEvent
        Console.WriteLine("Сигнал для AutoResetEvent");
        autoResetEvent.Set();

        await Task.Delay(1000);  // Даем время первой задаче захватить сигнал

        // Отправка сигнала для AutoResetEvent еще раз
        Console.WriteLine("Сигнал для AutoResetEvent (второй раз)");
        autoResetEvent.Set();

        // Даем немного времени задачам на запуск и ожидание сигналов
        await Task.Delay(1000);

        // Отправка сигнала для ManualResetEvent
        Console.WriteLine("Сигнал для ManualResetEvent");
        manualResetEvent.Set();

        // Ожидание завершения всех задач
        await Task.WhenAll(tasks);

        Console.WriteLine("Все задачи завершены.");
        Console.ReadKey();
    }

    // Используется для внутрипроцессной синхронизации. Легковесный и быстрый.
    // Автоматически освобождается в конце блока lock.
    static void DemonstrateLock()
    {
        lock (lockObject)
        {
            Console.WriteLine("Lock: Поток захватил блокировку. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(1000);
            Console.WriteLine("Lock: Поток освободил блокировку. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
        }
    }

    // Используется для синхронизации как в пределах одного процесса, так и между процессами.
    // Требует явного освобождения через ReleaseMutex. Более тяжеловесный по сравнению с lock.
    static void DemonstrateMutex()
    {
        mutex.WaitOne();
        try
        {
            Console.WriteLine("Mutex: Поток захватил мьютекс. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(1000);
        }
        finally
        {
            Console.WriteLine("Mutex: Поток освободил мьютекс. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
            mutex.ReleaseMutex();
        }
    }

    // Управляет доступом к ресурсу с ограниченным количеством слотов.
    // Может использоваться для ограничения числа одновременно выполняемых потоков.
    static void DemonstrateSemaphore()
    {
        semaphore.WaitOne();
        try
        {
            Console.WriteLine("Semaphore: Поток захватил слот. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(1000);
        }
        finally
        {
            Console.WriteLine("Semaphore: Поток освободил слот. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
            semaphore.Release();
        }
    }

    // AutoResetEvent автоматически сбрасывается после того, как один поток получает сигнал.
    static void DemonstrateAutoResetEvent(int id)
    {
        Console.WriteLine($"AutoResetEvent Task {id}: Ожидание сигнала. (Thread: {Thread.CurrentThread.ManagedThreadId})");
        autoResetEvent.WaitOne();
        Console.WriteLine($"AutoResetEvent Task {id}: Сигнал получен. (Thread: {Thread.CurrentThread.ManagedThreadId})");
    }

    // ManualResetEvent остается в сигнальном состоянии до тех пор, пока его явно не сбросят.
    static void DemonstrateManualResetEvent(int id)
    {
        Console.WriteLine($"ManualResetEvent Task {id}: Ожидание сигнала. (Thread: {Thread.CurrentThread.ManagedThreadId})");
        manualResetEvent.WaitOne();
        Console.WriteLine($"ManualResetEvent Task {id}: Сигнал получен. (Thread: {Thread.CurrentThread.ManagedThreadId})");
    }

    // Позволяет одному или нескольким потокам ожидать, пока счетчик не станет равным нулю.
    // Полезен для координации завершения нескольких операций.
    static void DemonstrateCountdownEvent()
    {
        Console.WriteLine("CountdownEvent: Ожидание обнуления счетчика. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
        countdownEvent.Signal();
        countdownEvent.Signal();
        countdownEvent.Signal();
        countdownEvent.Wait();
        Console.WriteLine("CountdownEvent: Счетчик обнулен. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
    }

    // Обеспечивает точку синхронизации для нескольких потоков, позволяя всем им достичь определенной точки перед продолжением.
    // Запускаем три задачи, каждая из которых будет сигналить барьер.
    static void DemonstrateBarrier()
    {
        Task[] barrierTasks = new Task[3];
        for (int i = 0; i < 3; i++)
        {
            barrierTasks[i] = Task.Run(() =>
            {
                Console.WriteLine("Barrier: Ожидание других потоков. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
                barrier.SignalAndWait();
                Console.WriteLine("Barrier: Все потоки достигли барьера. (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
            });
        }

        Task.WaitAll(barrierTasks);
    }
}

/*
Асинхронное программирование позволяет выполнять длительные операциит (например, операции ввода-вывода) без блокировки основного потока.

Как это работает:
    - Компилятор преобразует асинхронные методы в конечные автоматы (машины состояний).
    - Метод выполняется до первой асинхронной операции, после чего управление возвращается 
      вызывающему коду.
    - Когда асинхронная операция завершена, выполнение метода возобновляется с сохраненного состояния.
    - Асинхронные методы не создают новые потоки, а используют существующие, освобождая поток 
      для других задач во время ожидания.

Контекст синхронизации:
    - По умолчанию, продолжение асинхронного метода выполняется в том же контексте (например, UI-потоке).
    - Можно изменить это поведение с помощью `ConfigureAwait(false)`, что позволяет выполнить продолжение 
      в любом доступном потоке, улучшая производительность.

Пример использования ConfigureAwait:
    public async Task PerformAsyncOperations() {
        await someTask.ConfigureAwait(false);
        // Продолжение может выполняться в любом потоке.
    }
*/

