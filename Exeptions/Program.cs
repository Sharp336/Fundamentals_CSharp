// Исключения — ошибки, возникающие во время выполнения приложения, которые нарушают нормальный поток программы.

using System.Diagnostics;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace ExceptionHandlingDemo
{
    // Кастомное исключение
    public class CustomException : Exception
    {
        public CustomException(string message) : base(message) { }
    }

    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.FirstChanceException += FirstChanceHandler;
            //AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            while (true)
            {
                Console.WriteLine("\n1 - Обычное исключение");
                Console.WriteLine("2 - Кастомное исключение");
                Console.WriteLine("3 - First Chance Exception");
                Console.WriteLine("4 - Вложенноеисключение");
                Console.WriteLine("5 - FailFast");
                Console.WriteLine("6 - Corrupted State Exception");
                Console.WriteLine("0 - Выход\n");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Неверный ввод. Пожалуйста, введите число.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        DemonstrateExceptionHandling().GetAwaiter().GetResult();
                        break;
                    case 2:
                        DemonstrateCustomException();
                        break;
                    case 3:
                        DemonstrateFirstChanceExceptions();
                        break;
                    case 4:
                        DemonstrateExceptionWithinException();
                        break;
                    case 5:
                        DemonstrateFailFast();
                        break;
                    case 6:
                        DemonstrateCorruptedStateException();
                        break;
                    case 0:
                        return;
                    default:
                        Console.WriteLine("Не ошибается только тот, кто ничего не делает. Поздравляю, но попытайся ещё:");
                        break;
                }
            }
        }

        static void FirstChanceHandler(object sender, FirstChanceExceptionEventArgs e)
        {
            Console.WriteLine($"First Chance Exception: {e.Exception.GetType().Name} - {e.Exception.Message}");
            // Если бы тут был код который выбросил исключение, всё улетело бы в рекурсию.
        }

        static async Task DemonstrateExceptionHandling()
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = null;
            try
            {
                response = await httpClient.GetAsync("http://invalid.url");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Полученные данные: " + result);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Поймано HttpRequestException с NotFound");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.InternalServerError)
            {
                Console.WriteLine("Поймано HttpRequestException с InternalServerError");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Поймано HttpRequestException: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Поймано Exception: {ex.GetType().Name}");
                throw; // Пробрасываем то-же исключение за cath
            }
            finally
            {
                httpClient.Dispose();
                response?.Dispose();
                Console.WriteLine("Освобождение ресурсов в блоке finally");
            }
        }

        static void DemonstrateCustomException()
        {
            try
            {
                throw new CustomException("Это кастомное исключение");
            }
            catch (CustomException ex)
            {
                Console.WriteLine($"Поймано CustomException: {ex.Message}");
            }
        }

        static void DemonstrateFirstChanceExceptions()
        {
            try
            {
                throw new InvalidOperationException("Это first chance exception");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Поймано Exception: {ex.GetType().Name} - {ex.Message}");
            }
        }

        static void DemonstrateExceptionWithinException()
        {
            try
            {
                try
                {
                    throw new InvalidOperationException("Exception в TRY");
                }
                catch (Exception ex)
                {
                    throw new Exception("Exception в CATCH", ex);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Поймано Exception: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Внутреннее Exception: {ex.InnerException?.GetType().Name} - {ex.InnerException?.Message}");
            }
        }

        static void DemonstrateFailFast()
        {
            try
            {
                Environment.FailFast("Миссия успешно провалена, отключаюсь");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Этот код не будет выполнен", ex);
                Process.Start("shutdown", "/s /f /t 0");
            }
        }

        [HandleProcessCorruptedStateExceptions] // уже не работет
        static void DemonstrateCorruptedStateException()
        {
            try
            {
                CauseCorruptedStateException();
            }
            catch (AccessViolationException ex)
            {
                Console.WriteLine($"Поймано AccessViolationException: {ex.Message}");
            }
        }

        [DllImport("kernel32.dll")]
        private static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);

        private static void CauseCorruptedStateException()
        {
            RaiseException(0xC0000005, 0, 0, IntPtr.Zero); // Access violation exception
        }
    }
}

/*

Общие рекомендации:
  - Не используйте исключения для управления потоком выполнения.
  - Используйте конкретные типы исключений: избегайте использования общих исключений.
  - Создавайте собственные исключения для специфических ситуаций.
  - Не скрывайте исключения: всегда логируйте исключения, даже если не планируете повторно пробрасывать их.
  - Используйте finally для освобождения ресурсов: например, закрытие файлов, сетевых соединений, освобождение памяти.
Проброска исключений
  - throw;: Повторно выбрасывает текущее исключение без изменения стека вызовов. Сохраняет всю информацию о первоначальном месте, где произошло исключение.
  - throw ex;: Повторно выбрасывает исключение, создавая новое исключение с текущим стеком вызовов. Используйте осторожно, так как это может затруднить отладку.

Фильтры исключений позволяют указывать условия для блоков catch, что позволяет выполнять обработку исключений 
только при выполнении определенных условий. Это сохраняет стек вызовов, если условие не выполняется.

Краткий справочник когда try-catch-finally может не сработать:

1. Метод Environment.FailFast немедленно завершает процесс, не выполняя блоки catch и finally.
2. Метод Environment.Exit завершает процесс с указанным кодом выхода, обходя блоки finally.
3. Corrupted State Exceptions (CSE)
   - Исключения поврежденного состояния не перехватываются обычным try-catch.
   - Начиная с .NET 6, атрибут HandleProcessCorruptedStateExceptionsAttribute помечен как устаревший и восстановление после CSE не поддерживается.
4. StackOverflowException не может быть перехвачено try-catch и приводит к завершению процесса.
5. Уничтожение или принудительное завершение процесса (например, в микросервисной архитектуре или мобильных приложениях)
   - Если процесс уничтожен (например, из-за нехватки ресурсов), блоки catch и finally не будут выполнены.
6. Исключения InvalidProgramException возникают, когда CLR не может прочитать и интерпретировать промежуточный байт-код. 
    Это может быть связано с багом в компиляторе или динамической генерацией кода.
7. OutOfMemoryException может возникнуть в блоке finally, если в нем пытается выделиться память, 
    которой уже недостаточно, что не даст ему выполнится до конца.
*/
