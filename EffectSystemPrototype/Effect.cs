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
    public string GroupName;

    public ValueEffect(string property, string group)
    {
        this.Property = property;
        this.GroupName = group;
    }

    public abstract double GetValue(Dictionary<string, object> inputs);

}

// Effekt, der eine feste Zahl liefert
public class ConstantEffect : ValueEffect
{
    public double Value;
    public ConstantEffect(string property, double value, string opGroup) : base(property, opGroup)
    {
        this.Value = value;
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
    public InputEffect(string property, Func<Dictionary<string, object>, double> function, string opGroup) : base(property, opGroup)
    {
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

    public MetaEffect(Func<Dictionary<string, object>, Effect[]> metaFunction)
    {
        this.metaFunction = metaFunction;
    }

    public Effect[] Execute(Dictionary<string, object> inputs)
    {
        return metaFunction(inputs);
    }
}

