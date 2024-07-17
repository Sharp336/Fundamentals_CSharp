using System.Reflection;

// Кастомный атрибут
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class InfoAttribute : Attribute
{
    public string Description { get; }
    public string Version { get; }

    public InfoAttribute(string description, string version)
    {
        Description = description;
        Version = version;
    }
}

// Применение атрибута к классу и методу
[Info("Класс для демонстрации работы с атрибутами", "1.0")]
public class DemoClass
{
    [Info("Метод для демонстрации", "1.0")]
    public void DemoMethod()
    {
        // Получение атрибутов текущего метода
        MethodInfo method = typeof(DemoClass).GetMethod(nameof(DemoMethod));
        var attributes = method.GetCustomAttributes<InfoAttribute>();

        foreach (var attribute in attributes)
        {
            Console.WriteLine($"Внутри DemoMethod - Описание: {attribute.Description}, Версия: {attribute.Version}");
        }
    }
}

class Program
{
    static void Main()
    {
        bool continueExecution = true;
        while (continueExecution)
        {
            Console.WriteLine("\n1. Показать атрибуты класса");
            Console.WriteLine("2. Показать атрибуты метода");
            Console.WriteLine("3. Выполнить метод с атрибутами");
            Console.WriteLine("4. Выход\n");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    ShowClassAttributes();
                    break;
                case "2":
                    ShowMethodAttributes();
                    break;
                case "3":
                    ExecuteMethodWithAttributes();
                    break;
                case "4":
                    continueExecution = false;
                    break;
                default:
                    Console.WriteLine("OOOpsie, только 1-4");
                    break;
            }
        }
    }

    static void ShowClassAttributes()
    {
        Type type = typeof(DemoClass);
        var attributes = type.GetCustomAttributes<InfoAttribute>();

        foreach (var attribute in attributes)
        {
            Console.WriteLine($"Описание: {attribute.Description}, Версия: {attribute.Version}");
        }
    }

    static void ShowMethodAttributes()
    {
        MethodInfo method = typeof(DemoClass).GetMethod(nameof(DemoClass.DemoMethod));
        var attributes = method.GetCustomAttributes<InfoAttribute>();

        foreach (var attribute in attributes)
        {
            Console.WriteLine($"Описание: {attribute.Description}, Версия: {attribute.Version}");
        }
    }

    static void ExecuteMethodWithAttributes()
    {
        DemoClass demo = new DemoClass();
        demo.DemoMethod();
    }
}


/*
Атрибуты представляют собой специальные инструменты, которые позволяют встраивать в сборку дополнительные 
 метаданные и наследуются от класса System.Attribute.

Атрибуты могут применяться к следующим элементам кода:
   - Assembly (Сборка)
   - Module (Модуль)
   - Class (Класс)
   - Struct (Структура)
   - Enum (Перечисление)
   - Constructor (Конструктор)
   - Method (Метод)
   - Property (Свойство)
   - Field (Поле)
   - Event (Событие)
   - Interface (Интерфейс)
   - Parameter (Параметр)
   - Delegate (Делегат)
   - ReturnValue (Возвращаемое значение)

Встроенные атрибуты .NET:
   - [Obsolete]: Указывает, что элемент устарел.
   - [Conditional]: Указывает, что метод или атрибут выполняются только при определенных условиях.
   - [CallerMemberName]: Позволяет получить имя вызывающего метода.

*/
