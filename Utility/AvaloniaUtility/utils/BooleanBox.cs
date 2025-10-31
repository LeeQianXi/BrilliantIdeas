namespace AvaloniaUtility.utils;

public sealed class BooleanBox
{
    public static readonly BooleanBox True = new(true);
    public static readonly BooleanBox False = new(false);

    private readonly bool _value;

    private BooleanBox(bool value)
    {
        _value = value;
    }

    public override bool Equals(object? obj)
    {
        return obj is BooleanBox box ? _value.Equals(box._value) : _value.Equals(obj);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString();
    }

    public static implicit operator BooleanBox(bool value)
    {
        return new BooleanBox(value);
    }

    public static implicit operator bool(BooleanBox box)
    {
        return box._value;
    }
}