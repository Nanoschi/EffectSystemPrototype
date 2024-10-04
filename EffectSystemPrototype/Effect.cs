using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public abstract double GetValue(InputVector inputsVector);

}

// Effekt, der eine feste Zahl liefert
public class ConstantEffect : ValueEffect
{
    public double Value { get; }

    public ConstantEffect(string property, double value, string opGroup) : base(property, opGroup)
    {
        this.Value = value;
    }

    public override double GetValue(InputVector inputsVector)
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

    public override double GetValue(InputVector inputsVector)
    {
        return effectFunction(inputsVector);
    }
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