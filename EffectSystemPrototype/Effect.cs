using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum EffectOp
{
    Add,
    Mul
}


public abstract class Effect
{
    public static long MaxId = 0;
    public long Id;
    public double Duration = 0;

    protected Effect()
    {
        MaxId++;
        Id = MaxId;
    }
}

// Effekt, der eine Zahl liefert
public abstract class ValueEffect : Effect
{
    public string Property;
    public EffectOp Op;

    public ValueEffect(string property, EffectOp op)
    {
        this.Property = property;
        this.Op = op;
    }

    public abstract double GetValue(Dictionary<string, object> inputs);

}

// Effekt, der eine feste Zahl liefert
public class ConstantEffect : ValueEffect
{
    public double Value;
    public ConstantEffect(string property, double value, EffectOp op, double duration = 0) : base(property, op)
    {
        this.Value = value;
        this.Duration = duration;
    }

    public override double GetValue(Dictionary<string, object> inputs)
    {
        return Value;
    }
}

// Effekt, der eine Zahl auf Basis von input werten liefert
public class InputEffect : ValueEffect
{
    public Func<Dictionary<string, object>, double> effectFunction;
    public InputEffect(string property, Func<Dictionary<string, object>, double> function, EffectOp op, double duration = 0) : base(property, op)
    {
        this.Duration = duration;
        this.effectFunction = function;
    }

    public override double GetValue(Dictionary<string, object> inputs)
    {
        return effectFunction(inputs);
    }
}

// Effekt, der andere Effekte erzeugt
public class MetaEffect : Effect
{
    public Func<Dictionary<string, object>, Effect[]> metaFunction;

    public MetaEffect(Func<Dictionary<string, object>, Effect[]> metaFunction, double duration = 0)
    {
        this.Duration = duration;
        this.metaFunction = metaFunction;
    }

    public Effect[] Execute(Dictionary<string, object> inputs)
    {
        return metaFunction(inputs);
    }
}

