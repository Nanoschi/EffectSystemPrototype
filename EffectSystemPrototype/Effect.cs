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
    public static long maxId = 0;
    public long id;
    public double duration = 0;

    protected Effect()
    {
        maxId++;
        id = maxId;
    }
}

// Effekt, der eine Zahl liefert
public abstract class ValueEffect : Effect
{
    public string property;
    public EffectOp op;

    public ValueEffect(string property, EffectOp op)
    {
        this.property = property;
        this.op = op;
    }

    public abstract double GetValue(Dictionary<string, object> inputs);

}

// Effekt, der eine feste Zahl liefert
class ConstantEffect : ValueEffect
{
    public double value;
    public ConstantEffect(string property, double value, EffectOp op, double duration = 0) : base(property, op)
    {
        this.value = value;
        this.duration = duration;
    }

    public override double GetValue(Dictionary<string, object> inputs)
    {
        return value;
    }
}

// Effekt, der eine Zahl auf Basis von input werten liefert
class InputEffect : ValueEffect
{
    public Func<Dictionary<string, object>, double> effectFunction;
    public InputEffect(string property, Func<Dictionary<string, object>, double> function, EffectOp op, double duration = 0) : base(property, op)
    {
        this.duration = duration;
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
        this.duration = duration;
        this.metaFunction = metaFunction;
    }

    public Effect[] Execute(Dictionary<string, object> inputs)
    {
        return metaFunction(inputs);
    }
}

