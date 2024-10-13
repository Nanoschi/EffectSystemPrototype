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
    internal virtual void OnSystemEntered(InputVector inputVector) { }
    internal virtual void OnSystemExited(InputVector inputVector) { }

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
    public Func<InputVector, double> EffectFunction { get; set; }
    public InputEffect(string property, Func<InputVector, double> function, string opGroup) : base(property, opGroup)
    {
        EffectFunction = function;
    }

    public override double GetValue(InputVector inputVector)
    {
        return EffectFunction(inputVector);
    }
}

public class DataEffect : ValueEffect
{
    public Dictionary<string, object> Data { get; set; } = new();
    public Action<InputVector, Dictionary<string, object>> Constructor { get; set; } = DefaultConstructor;
    public Action<InputVector, Dictionary<string, object>> Destructor { get; set; } = DefaultDestructor;
    public Func<InputVector, Dictionary<string, object>, double> EffectFunction { get; set; }

    public DataEffect(
        string property,
        Func<InputVector, Dictionary<string, object>, double> effectFunction,
        Action<InputVector, Dictionary<string, object>> constructor,
        Action<InputVector, Dictionary<string, object>> destructor,
        string opGroup) 
        : base(property, opGroup)
    {
        Constructor = constructor;
        Destructor = destructor;
        EffectFunction = effectFunction;
    }

    public DataEffect(
        string property,
        Func<InputVector, Dictionary<string, object>, double> effectFunction, 
        string opGroup)
        : base(property, opGroup)
    {
        EffectFunction = effectFunction;
    }

    internal override void OnSystemEntered(InputVector inputVector)
    {
        Constructor(inputVector, Data);
    }

    internal override void OnSystemExited(InputVector inputVector)
    {
        Destructor(inputVector, Data);
    }

    public override double GetValue(InputVector inputVector)
    {
        return EffectFunction(inputVector, Data);
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