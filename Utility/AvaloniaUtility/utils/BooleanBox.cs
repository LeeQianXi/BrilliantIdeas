namespace AvaloniaUtility.utils;

public sealed class BooleanBox
{
    public static readonly BooleanBox True = new(true);
    public static readonly BooleanBox False = new(false);

    private BooleanBox(bool value)
    {
        Value = value;
    }

    public bool Value { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is BooleanBox box ? Value.Equals(box.Value) : Value.Equals(obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator BooleanBox(bool value)
    {
        return new BooleanBox(value);
    }

    public static implicit operator bool(BooleanBox box)
    {
        return box.Value;
    }
}