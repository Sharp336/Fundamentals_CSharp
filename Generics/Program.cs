public class Animal
{
    public virtual void Speak() => Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
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
        Animal animalFromDelegate = covariantDelegate();
        animalFromDelegate.Speak(); // Woof

        ContravariantDelegate<Animal> contravariantDelegate = (Animal animal) => animal.Speak();
        contravariantDelegate(new Dog()); // Woof
        contravariantDelegate(new Cat()); // Meow

        List<Dog> dogs = new List<Dog> { new Dog(), new Dog() };
        IEnumerable<Animal> animals = dogs;
        foreach (var animal in animals)
        {
            animal.Speak(); // Woof
        }

        Action<Animal> animalAction = animal => animal.Speak();
        Action<Dog> dogAction = animalAction;
        dogAction(new Dog()); // Woof

        Func<Dog> dogFunc = () => new Dog();
        Func<Animal> animalFunc = dogFunc;
        animalFromDelegate = animalFunc();
        animalFromDelegate.Speak(); // Woof

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

В C# ковариантность и контравариантность позволяют использовать неявное преобразование ссылок для типов массивов, делегатов и аргументов универсального типа.

Ковариантность: Сохраняет совместимость присваивания. Это означает, что вы можете присвоить объект 
более производного типа переменной базового типа. Например, IEnumerable<Dog> может быть присвоен IEnumerable<Animal>.

Контравариантность: Присваивание работает противоположным образом. Это означает, что вы можете присвоить объект 
базового типа переменной производного типа. Например, IContravariant<Animal> может быть присвоен IContravariant<Dog>.

Вариативность поддерживается только для ссылочных типов. Типы значений, такие как int и double, не могут использовать ковариантность или контравариантность.
Массивы в C# ковариантны, но это может привести к исключениям времени выполнения (ArrayTypeMismatchException), если неправильно использовать типы.\

*/
