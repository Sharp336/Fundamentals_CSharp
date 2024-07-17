using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

// Насчёт импорта неймспейсов какой-то интересной информации не нашёл,
// кроме недавних нововведений в язык:
//
//     В C# 10 добавлена возможность объявлять глобальные директивы using, используя модификатор global.
//     Это позволяет применять директиву using ко всем исходным файлам в сборке.
//     Например: global using System.Text.Json;
//
//     В C# 10 также представлена новая форма объявления неймспейсов, называемая "file-scoped namespace declaration".
//     Это сокращает вертикальное и горизонтальное пространство, занимаемое объявлениями неймспейсов. Пример:
//     namespace DisposeExample;

//     public class UnmanagedTopicsResourceHandler
//     {
//         // Код класса
//     }

public class UnmanagedTopicsResourceHandler : IDisposable
{
    // Спросить является ли FileStream неуправляемым ресурсом
    private IntPtr _fileHandle;
    private bool _disposed = false;
    private List<string> _topics = [];

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadFile(
        IntPtr hFile,
        byte[] lpBuffer,
        uint nNumberOfBytesToRead,
        out uint lpNumberOfBytesRead,
        IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    public UnmanagedTopicsResourceHandler(string filePath)
    {
        // Открытие файла и получение дескриптора неуправляемого ресурса
        _fileHandle = CreateFile(
            filePath,
            0x80000000, // GENERIC_READ
            1, // FILE_SHARE_READ
            IntPtr.Zero,
            3, // OPEN_EXISTING
            0,
            IntPtr.Zero);

        if (_fileHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Не удалось открыть файл.");
        }
    }

    public List<string> ReadTopics()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(UnmanagedTopicsResourceHandler));
        }

        byte[] buffer = new byte[1024];
        uint bytesRead;
        bool success = ReadFile(_fileHandle, buffer, (uint)buffer.Length, out bytesRead, IntPtr.Zero);

        if (!success)
        {
            throw new IOException("Ошибка чтения файла.");
        }

        try
        {
            string jsonString = Encoding.UTF8.GetString(buffer, 0, (int)bytesRead);
            _topics = JsonSerializer.Deserialize<List<string>>(jsonString);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Ошибка при десериализации JSON.", ex);
        }
        // Спросить про корректность возвращения пустого списка
        return _topics ?? new List<string>();
    }

    // Реализация IDisposable для освобождения ресурсов
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Метод для освобождения управляемых и неуправляемых ресурсов
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Освобождение управляемых ресурсов
                _topics = null;
            }
            // Освобождение неуправляемых ресурсов
            if (_fileHandle != IntPtr.Zero)
            {
                CloseHandle(_fileHandle);
                _fileHandle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    // Финализатор на случай, если Dispose не был вызван явно
    ~UnmanagedTopicsResourceHandler()
    {
        Dispose(false);
    }
}

class Program
{
    static void Main()
    {
        string filePath = "topics.json";

        if (!File.Exists(filePath))
        {
            var topics = new List<string>
            {
                "Основы C#",
                "ORM",
                "MS SQL Server",
                "Технологии",
                "JavaScript / Typescript",
                "Reactjs"
            };
            string jsonString = JsonSerializer.Serialize(topics);
            File.WriteAllText(filePath, jsonString);
        }

        using (UnmanagedTopicsResourceHandler handler = new UnmanagedTopicsResourceHandler(filePath))
        {
            List<string> topics = handler.ReadTopics();
            foreach (var topic in topics)
            {
                Console.WriteLine(topic);
            }
        } 

        Console.ReadKey();
    }
}
