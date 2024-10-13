namespace EffectSystemPrototype;

public enum EffectOp
{
    Add,
    Mul
}

public abstract class Effect
{
    private static long _maxId;
    public long Id { get; }

    protected Effect()
    {
        _maxId++;
        Id = _maxId;
    }
}

// Effekt, der eine Zahl liefert
public abstract class ValueEffect : Effect
{
    public string Property { get; }
    public string GroupName { get; }

    public ValueEffect(string property, string group)
    {
        Property = property;
        GroupName = group;
    }

    public abstract double GetValue(InputVector inputVector);
    public virtual void OnSystemEntered(InputVector inputVector) { }
    public virtual void OnSystemExited(InputVector inputVector) { }

}

// Effekt, der eine feste Zahl liefert
public class ConstantEffect : ValueEffect
{
    public double Value { get; }

    public ConstantEffect(string property, double value, string opGroup) : base(property, opGroup)
    {
        this.Value = value;
    }

    public override double GetValue(InputVector inputVector)
    {
        return Value;
    }
}

// Effekt, der eine Zahl auf Basis von input werten liefert
public class InputEffect : ValueEffect
{
    public Func<InputVector, double> effectFunction;
    public InputEffect(string property, Func<InputVector, double> function, string opGroup) : base(property, opGroup)
    {
        effectFunction = function;
    }

    public override double GetValue(InputVector inputVector)
    {
        return effectFunction(inputVector);
    }
}

public class DataEffect : InputEffect
{
    private Dictionary<string, object> Data { get; set; } = new();
    public Action<InputVector, Dictionary<string, object>> Constructor { get; set; } = DefaultConstructor;
    public Action<InputVector, Dictionary<string, object>> Destructor { get; set; } = DefaultDestructor;

    public DataEffect(
        string property, Func<InputVector, double> function,
        Action<InputVector, Dictionary<string, object>> constructor,
        Action<InputVector, Dictionary<string, object>> destructor,
        string opGroup) 
        : base(property, function, opGroup)
    {
        Constructor = constructor;
        Destructor = destructor;
    }

    public DataEffect(
        string property, Func<InputVector, double> function, string opGroup)
        : base(property, function, opGroup) { }

    public override void OnSystemEntered(InputVector inputVector)
    {
        Constructor(inputVector, Data);
    }

    public override void OnSystemExited(InputVector inputVector)
    {
        Constructor(inputVector, Data);
    }

    private static void DefaultConstructor(InputVector i, Dictionary<string, object> d) { }
    private static void DefaultDestructor(InputVector i, Dictionary<string, object> d) { }
}

// Effekt, der andere Effekte erzeugt
public class MetaEffect : Effect
{
    public Func<InputVector, Effect[]> MetaFunction { get; }

    public MetaEffect(Func<InputVector, Effect[]> metaFunction)
    {
        this.MetaFunction = metaFunction;
    }

    public Effect[] Execute(InputVector inputsVector)
    {
        return MetaFunction(inputsVector);
    }
}