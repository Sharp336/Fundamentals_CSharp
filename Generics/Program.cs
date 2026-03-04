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
    void Consume(T item);
}

public class CovariantClass<T> : ICovariant<T>
{
    private readonly T _value;

    public CovariantClass(T value) => _value = value;

    public T Get() => _value;
}

public class ContravariantClass<T> : IContravariant<T>
{
    public void Consume(T item) => Console.WriteLine(item);
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
        contravariantDog.Consume(new Dog()); // Dog

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
   - Позволяют писать универсальные классы, методы, интерфейсы и делегаты, не привязываясь к одному конкретному типу.
   - Пример: public class Repository<T>
   - Главный плюс: типобезопасность без кастов и object.

Элементы кода, к которым могут применяться дженерики:
   - Классы
   - Методы
   - Интерфейсы
   - Делегаты
   - Структуры
   - Записи

Ограничения generic-параметров (constraints):
   - where T : class      -> T должен быть ссылочным типом
   - where T : struct     -> T должен быть значимым типом (non-nullable value type)
   - where T : new()      -> у T должен быть публичный конструктор без параметров
   - where T : BaseClass  -> T должен наследоваться от указанного класса
   - where T : IInterface -> T должен реализовывать указанный интерфейс
   - Ограничения можно комбинировать, если это допустимо по синтаксису

Вариантность в C#:

По умолчанию generic-типы инвариантны.
Это значит, что если Dog : Animal, то:
   - List<Dog> НЕ является List<Animal>
   - Repository<Dog> НЕ является Repository<Animal>

Вариантность позволяет ослабить это правило только в безопасных случаях.

Ковариантность (out):
   - Используется, когда generic-параметр только "отдаётся наружу"
   - Позволяет присвоить более конкретный тип более общему
   - Пример: IEnumerable<Dog> -> IEnumerable<Animal>
   - Мнемоника: producer -> out

Контравариантность (in):
   - Используется, когда generic-параметр только "принимается внутрь"
   - Позволяет присвоить более общий тип более конкретному
   - Пример: IComparer<Animal> -> IComparer<Dog>
   - Мнемоника: consumer -> in

Где в C# можно объявить вариантность:
   - Только у интерфейсов
   - Только у делегатов

Примеры встроенной вариантности:
   - IEnumerable<out T>   -> ковариантен
   - IComparer<in T>      -> контравариантен
   - Func<in T, out TResult>
   - Action<in T>

Вариантность поддерживается только для ссылочных типов.
Типы значений (int, double, DateTime, структуры и т.д.) не участвуют в ковариантности и контравариантности.

Массивы в C#:
   - Массивы ковариантны: Dog[] можно присвоить Animal[]
   - Но это небезопасно и может привести к ошибке времени выполнения:
     ArrayTypeMismatchException
   - Поэтому ковариантность массивов - легаси абуз, а не пример хорошего дизайна

Делегаты и вариантность:
   - Делегаты поддерживают ковариантность и контравариантность
   - Если делегат возвращает значение, возможна ковариантность по возвращаемому типу
   - Если делегат принимает параметр, возможна контравариантность по параметру
   - Для неявного преобразования между generic-делегатами нужно явно указывать in / out

Пример:
   public delegate R DVariant<in A, out R>(A a);
*/
