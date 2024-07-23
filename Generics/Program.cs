public class Animal
{
    public virtual void Speak() => Console.WriteLine("Animal sound");
}

public class Dog : Animal
{
    public override void Speak() => Console.WriteLine("Woof");
}

public class Cat : Animal
{
    public override void Speak() => Console.WriteLine("Meow");
}

// Covariant interface
public interface ICovariant<out T>
{
    T Get();
}

// Contravariant interface
public interface IContravariant<in T>
{
    void Set(T item);
}

public class CovariantClass<T> : ICovariant<T>
{
    private T _value;

    public CovariantClass(T value) => _value = value;

    public T Get() => _value;
}

public class ContravariantClass<T> : IContravariant<T>
{
    public void Set(T item) => Console.WriteLine(item.ToString());
}

public delegate T CovariantDelegate<out T>();
public delegate void ContravariantDelegate<in T>(T value);

class Program
{
    static void Main()
    {
        ICovariant<Dog> covariantDog = new CovariantClass<Dog>(new Dog());
        ICovariant<Animal> covariantAnimal = covariantDog; 
        Animal animalFromCovariant = covariantAnimal.Get();
        animalFromCovariant.Speak(); // Woof

        IContravariant<Animal> contravariantAnimal = new ContravariantClass<Animal>();
        IContravariant<Dog> contravariantDog = contravariantAnimal; 
        contravariantDog.Set(new Dog()); // Dog

        CovariantDelegate<Dog> covariantDelegate = () => new Dog();
        CovariantDelegate<Animal> animalDelegate = covariantDelegate;
        Animal animalFromDelegate = animalDelegate();
        animalFromDelegate.Speak(); // Woof

        ContravariantDelegate<Animal> contravariantDelegate = (Animal animal) => animal.Speak();
        ContravariantDelegate<Dog> dogDelegate = contravariantDelegate; 
        dogDelegate(new Dog()); // Woof

        Console.ReadKey();
    }

}

/*
Справка по дженерикам:

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

Вариантность в C#:

Ковариантность: Сохраняет совместимость присваивания. Это означает, что вы можете присвоить объект 
более производного типа переменной базового типа. Например, IEnumerable<Dog> может быть присвоен IEnumerable<Animal>.

Контравариантность: Присваивание работает противоположным образом. Это означает, что вы можете присвоить объект 
базового типа переменной производного типа. Например, IContravariant<Animal> может быть присвоен IContravariant<Dog>.

Вариативность поддерживается только для ссылочных типов. Типы значений, такие как int и double, не могут использовать ковариантность или контравариантность.
Массивы в C# ковариантны, но это может привести к исключениям времени выполнения (ArrayTypeMismatchException), если неправильно использовать типы.

 Когда вариантность присутствует по умолчанию:
- Массивы в C# ковариантны.
- Делегаты поддерживают ковариантность и контравариантность для совпадения сигнатур методов. Например, 
делегат с возвращаемым типом object может использовать метод, который возвращает string (ковариантность), 
а делегат с параметром object может использовать метод, принимающий string (контравариантность).

!!! Однако, для неявного преобразования между делегатами всё-таки необходимо указать in или out - https://learn.microsoft.com/ru-ru/dotnet/csharp/programming-guide/concepts/covariance-contravariance/variance-in-delegates

Важно: параметры `ref`, `in` и `out` в C# невозможно пометить как вариативные. 
В одном делегате можно реализовать поддержку вариативности и ковариации, но для разных параметров типа. Пример:
public delegate R DVariant<in A, out R>(A a);  

*/
