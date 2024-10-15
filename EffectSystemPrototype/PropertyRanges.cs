using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectSystemPrototype;

public class PropertyRanges
{
    private Dictionary<string, (double minValue, double maxValue)> Ranges { get; set; } = new();

    public void SetMaxValue(string property, double value)
    {
        var current = Ranges[property];
        Ranges[property] = (current.minValue, value);
    }

    public void SetMinValue(string property, double value)
    {
        var current = Ranges[property];
        Ranges[property] = (value, current.maxValue);
    }

    public void SetMinMaxValue(string property, double minValue, double maxValue)
    {
        Ranges[property] = (minValue, maxValue);
    }

    public (double min, double max) GetMinMaxValue(string property)
    {
        return Ranges[property];
    }

    public double ClampValue(string property, double value)
    {
        var range = Ranges[property];
        return Math.Clamp(value, range.minValue, range.maxValue);
    }

    internal void Add(string property)
    {
        Add(property, double.NegativeInfinity, double.PositiveInfinity);
    }

    internal void Add(string property, double min, double max)
    {
        Ranges.Add(property, (min, max));
    }

    internal bool Remove(string property)
    {
        return Ranges.Remove(property);
    }
}

