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
    List<double> values;

    public Pipeline(EffectOp op) 
    {
        accumOperator = op;
        values = new List<double>();
    }

    public double Calculate(double start_value)
    {
        double result = 0;
        if (accumOperator == EffectOp.Add)
        {
            result = values.Aggregate(start_value, (accum, x) => x + accum);
        }
        else if (accumOperator == EffectOp.Mul)
        {
            result = values.Aggregate(start_value, (accum, x) => x * accum);
        }

        return result;
    }

    public Pipeline Copy()
    {
        Pipeline copy = new(accumOperator);
        foreach (double value in values)
        {
            copy.values.Add(value);
        }

        return copy;
    }

    public void AddValue(double value)
    {
        values.Add(value);
    }
}

