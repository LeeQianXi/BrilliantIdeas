namespace NetUtility;

public interface ISupplier<out T>
{
    T GetValue();

    SupplierFactory<T> AsSupplier()
    {
        return GetValue;
    }
}

public delegate T SupplierFactory<out T>();

public interface IConsumer<in T>
{
    void Consume(T value);

    ConsumeFactory<T> AsConsumer()
    {
        return Consume;
    }
}

public delegate void ConsumeFactory<in T>(T value);