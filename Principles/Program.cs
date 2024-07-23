using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Принципы SOLID:");
        IWorker worker = new Worker();
        Manager manager = new Manager(worker);
        manager.Manage();

        Console.WriteLine("\nПринцип YAGNI:");
        TaskManager taskManager = new TaskManager();
        taskManager.AddTask("Срочно что-то сделать");
        taskManager.ListTasks();

        Console.WriteLine("\nDependency Injection и Inversion of Control:");
        IEngine engine = new Engine();
        Car car = new Car(engine);
        car.Start();

        Console.WriteLine("\nПринцип KISS:");
        EmailValidator emailValidator = new EmailValidator();
        Console.WriteLine("Email валиден: " + emailValidator.Validate("example@example.com"));

        Console.WriteLine("\nПаттерн MVC:");
        var model = new Product { Name = "Ноутбук", Price = 99999.99 };
        var view = new ProductView();
        var controller = new ProductController(model, view);
        controller.UpdateView();

        Console.ReadKey();
    }
}

public class BadWorker
{
    public void Work()
    {
        Console.WriteLine("Работник работает.");
    }

    public void Manage()
    {
        Console.WriteLine("Работник управляет.");
    }
}

public interface IWorker
{
    void Work();
}

public class Worker : IWorker
{
    public void Work()
    {
        Console.WriteLine("Работник работает.");
    }
}

public class BadManager
{
    public void Manage(IWorker worker)
    {
        if (worker is Worker)
        {
            worker.Work();
        }
        else if (worker is SuperWorker)
        {
            ((SuperWorker)worker).Work();
        }
    }
}

public class Manager
{
    private readonly IWorker _worker;
    public Manager(IWorker worker)
    {
        _worker = worker;
    }
    public void Manage()
    {
        _worker.Work();
    }
}

public class BadSuperWorker : Worker
{
    public void SuperWork()
    {
        Console.WriteLine("Супер работник вник и реально пашет.");
    }
    public new void Work()
    {
        throw new NotImplementedException("Супер работник не работает так.");
    }
}

public class SuperWorker : IWorker
{
    public void Work()
    {
        Console.WriteLine("Супер работник вник и реально пашет.");
    }
}

public interface IBadMultiFunctionDevice
{
    void Print();
    void Scan();
    void Fax();
    void Staple();
}

public interface IPrinter
{
    void Print();
}

public interface IScanner
{
    void Scan();
}

public class MultiFunctionDevice : IPrinter, IScanner
{
    public void Print()
    {
        Console.WriteLine("Печать...");
    }

    public void Scan()
    {
        Console.WriteLine("Сканирование...");
    }
}

public class TaskManager
{
    private List<string> tasks = new List<string>();

    public void AddTask(string task)
    {
        tasks.Add(task);
    }

    public void RemoveTask(string task)
    {
        tasks.Remove(task);
    }

    public void ListTasks()
    {
        foreach (var task in tasks)
        {
            Console.WriteLine(task);
        }
    }


    public void ExportTasksToCsv()
    {
        throw new NotImplementedException("Не нужно пока.");
    }

    public void ImportTasksFromCsv()
    {
        throw new NotImplementedException("Не нужно пока.");
    }
}

public interface IEngine
{
    void Start();
}

public class Engine : IEngine
{
    public void Start()
    {
        Console.WriteLine("Двигатель запущен.");
    }
}

public class BadCar
{
    private readonly Engine _engine = new Engine();
    public void Start()
    {
        _engine.Start();
    }
}

public class Car
{
    private readonly IEngine _engine;
    public Car(IEngine engine)
    {
        _engine = engine;
    }

    public void Start()
    {
        _engine.Start();
    }
}

public class EmailValidator
{
    public bool Validate(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }

    public bool BadValidate(string email)
    {
        if (email == null)
        {
            throw new ArgumentNullException(nameof(email));
        }
        if (!email.Contains("@") || !email.Contains("."))
        {
            return false;
        }

        string[] parts = email.Split('@');
        if (parts.Length != 2)
        {
            return false;
        }

        string[] domainParts = parts[1].Split('.');
        if (domainParts.Length < 2)
        {
            return false;
        }

        foreach (char c in email)
        {
            if (!char.IsLetterOrDigit(c) && c != '@' && c != '.' && c != '_')
            {
                return false;
            }
        }

        return true;
    }
}

public class Product
{
    public string Name { get; set; }
    public double Price { get; set; }
}

public class ProductView
{
    public void ShowProductDetails(string productName, double productPrice)
    {
        Console.WriteLine($"Продукт: {productName}, Цена: {productPrice}");
    }
}

public class ProductController
{
    private readonly Product _model;
    private readonly ProductView _view;

    public ProductController(Product model, ProductView view)
    {
        _model = model;
        _view = view;
    }

    public void UpdateView()
    {
        _view.ShowProductDetails(_model.Name, _model.Price);
    }

    public void BadUpdateView()
    {
        Console.WriteLine($"Продукт: {_model.Name}, Цена: {_model.Price}");
    }
}

// Аналоги паттерна MVC
// - MVP (Model-View-Presenter): Представление пассивно и управляется презентером.
// - MVVM (Model-View-ViewModel): Представление связывается с ViewModel с помощью привязки данных.
