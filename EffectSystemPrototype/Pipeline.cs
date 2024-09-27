using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Speichert alle Werte, die für ein bestimmtes Property addiert und multipliziert werden müssen
// (Effekt wir nicht gespeichert, nur sein Wert)
public class Pipeline
{
    EffectOp AccumOperator;
    public LinkedList<ValueEffect> Effects = new LinkedList<ValueEffect>();

    public Pipeline(EffectOp op) 
    {
        AccumOperator = op;
    }

    public double Calculate(double startValue, Dictionary<string, object> inputs)
    {
        double result = 0;
        if (AccumOperator == EffectOp.Add)
        {
            result = Effects.Aggregate(startValue, (accum, x) => x.GetValue(inputs) + accum);
        }
        else if (AccumOperator == EffectOp.Mul)
        {
            result = Effects.Aggregate(startValue, (accum, x) => x.GetValue(inputs) * accum);
        }

        return result;
    }

    public Pipeline Copy()
    {
        Pipeline copy = new(AccumOperator);
        foreach (ValueEffect effect in Effects)
        {
            copy.Effects.AddLast(effect);
        }

        return copy;
    }

    public void AddEffect(ValueEffect effect)
    {
        Effects.AddLast(effect);
    }

    public bool RemoveEffect(long effectId)
    {
        var node = Effects.First;
        for (int i = 0; i < Effects.Count; i++)
        {
            if (node.Value.Id == effectId)
            {
                Effects.Remove(node);
                return true;
            }
        }
        return false;
    }
}

