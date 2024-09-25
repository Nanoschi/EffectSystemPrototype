using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Speichert alle Werte, die für ein bestimmtes Property addiert und multipliziert werden müssen
// (Effekt wir nicht gespeichert, nur sein Wert)
class Pipeline
{
    EffectOp accumOperator;
    public LinkedList<ValueEffect> effects = new LinkedList<ValueEffect>();

    public Pipeline(EffectOp op) 
    {
        accumOperator = op;
    }

    public double Calculate(double start_value, Dictionary<string, object> inputs)
    {
        double result = 0;
        if (accumOperator == EffectOp.Add)
        {
            result = effects.Aggregate(start_value, (accum, x) => x.GetValue(inputs) + accum);
        }
        else if (accumOperator == EffectOp.Mul)
        {
            result = effects.Aggregate(start_value, (accum, x) => x.GetValue(inputs) * accum);
        }

        return result;
    }

    public Pipeline Copy()
    {
        Pipeline copy = new(accumOperator);
        foreach (ValueEffect effect in effects)
        {
            copy.effects.AddLast(effect);
        }

        return copy;
    }

    public void AddEffect(ValueEffect effect)
    {
        effects.AddLast(effect);
    }

    public bool RemoveEffect(long effect_id)
    {
        var node = effects.First;
        for (int i = 0; i < effects.Count; i++)
        {
            if (node.Value.id == effect_id)
            {
                effects.Remove(node);
                return true;
            }
        }
        return false;
    }
}

