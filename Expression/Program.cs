using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

public class Program
{
    public static void Main()
    {
        // Пример десериализации JSON с использованием выражений
        string json = "{\"Name\":\"John\", \"Age\":30}";
        var person = DeserializeJson<Person>(json);
        Console.WriteLine($"Deserialized Person: Name = {person.Name}, Age = {person.Age}");

        // Динамически создаем экземпляр класса MyClass
        var instance = CreateInstance("MyClass");

        // Динамически создаем и выполняем выражение для вызова метода Print
        var methodCallExpression = CreateMethodCallExpression(instance, "Print", new object[] { "Hello, Combined!" });
        var lambda = Expression.Lambda<Action>(methodCallExpression);
        var action = lambda.Compile();
        action();

       Console.ReadKey();
    }

    // Метод для десериализации JSON с использованием выражений
    public static T DeserializeJson<T>(string json)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var obj = JsonSerializer.Deserialize<T>(json, options);

        var type = typeof(T);
        var instance = Expression.Variable(type, "instance");
        var blockExpressions = new List<Expression>();

        var bindings = new List<MemberBinding>();
        foreach (var prop in type.GetProperties())
        {
            if (prop.CanWrite)
            {
                var value = Expression.Constant(prop.GetValue(obj));
                var binding = Expression.Bind(prop, value);
                bindings.Add(binding);
            }
        }

        var initializer = Expression.MemberInit(Expression.New(type), bindings);
        var assign = Expression.Assign(instance, initializer);
        blockExpressions.Add(assign);
        blockExpressions.Add(instance);

        var block = Expression.Block(new[] { instance }, blockExpressions);
        var lambda = Expression.Lambda<Func<T>>(block);
        return lambda.Compile()();
    }

    // Метод для динамического создания экземпляра класса
    public static object CreateInstance(string typeName)
    {
        var type = Assembly.GetExecutingAssembly().GetType(typeName);
        return Activator.CreateInstance(type);
    }

    // Метод для создания выражения вызова метода
    public static Expression CreateMethodCallExpression(object instance, string methodName, object[] parameters)
    {
        var method = instance.GetType().GetMethod(methodName);
        var instanceExpression = Expression.Constant(instance);
        var parameterExpressions = method.GetParameters()
                                         .Select((param, index) => Expression.Constant(parameters[index], param.ParameterType))
                                         .ToArray();
        return Expression.Call(instanceExpression, method, parameterExpressions);
    }
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class MyClass
{
    public void Print(string message)
    {
        Console.WriteLine(message);
    }
}

/*
Делегат - тип, который инкапсулирует ссылку на метод. Делегаты могут использоваться для вызова методов динамически.

Дерево выражений - структура данных, которая описывает программный код на уровне выражений. 
Оно может быть проанализировано и изменено до выполнения, а затем скомпилировано в делегат для выполнения.

Использование и взаимодействие:
- Деревья выражений используются для создания динамических запросов, таких как LINQ-запросы, которые могут быть преобразованы в SQL-запросы в ORM, например, в Entity Framework.
- Деревья выражений можно компилировать в делегаты, что позволяет выполнять динамически созданный код.
- Компиляция дерева выражений в делегат: expression.Compile();
  Пример: Func<int, int, int> func = expression.Compile();
- Анализ и модификация дерева выражений позволяют извлекать структуру и логику выражения, что полезно для создания компиляторов, интерпретаторов и трансляторов.
*/
