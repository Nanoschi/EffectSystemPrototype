using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public class EffectSystemProperties
{
    public Dictionary<string, (double Value, double MinValue, double MaxValue)> properties = new();
    public int Count
    {
        get => properties.Count;
    }

    
    public void AddProperty(string name, double value = 0)
    {
        AddProperty(name, value, double.NegativeInfinity, double.PositiveInfinity);
    }

    public void AddProperty(string name, double value, double min, double max)
    {
        properties.TryAdd(name, (value, min, max));
    }

    public void RemoveProperty(string name)
    {
        properties.Remove(name);
    }

    public void SetPropertyValue(string name, double value)
    {
        if (properties.TryGetValue(name, out var currentValue))
        {
            currentValue.Value = Math.Clamp(value, currentValue.MinValue, currentValue.MaxValue); 
            properties[name] = currentValue;
        }
    }

    public double GetPropertyValue(string name)
    {
        if (properties.TryGetValue(name, out var value))
        {
            return value.Value;
        }
        return 0;
    }

    public void OverridePropertyValue(string name, double value)
    {
        if (properties.TryGetValue(name, out var currentValue))
        {
            currentValue.Value = value;
            properties[name] = currentValue;
        }
    }

    public void SetPropertyMin(string name, double newMin)
    {
        if (properties.TryGetValue(name, out var currentValue))
        {
            currentValue.MinValue = newMin;
            properties[name] = currentValue;
        }
    }

    public void SetPropertyMax(string name, double newMin)
    {
        if (properties.TryGetValue(name, out var currentValue))
        {
            currentValue.MinValue = newMin;
            properties[name] = currentValue;
        }
    }

    public (double Min, double Max) GetPropertyMinMax(string name)
    {
        if (properties.TryGetValue(name, out var currentValue))
        {
            return (currentValue.MinValue, currentValue.MaxValue);
        }
        return (0, 0);
    }

    public string[] GetPropertyArray()
    {
        return properties.Keys.ToArray();
    }

    public EffectSystemProperties Copy()
    {
        EffectSystemProperties copy = new();
        foreach (var property in properties)
        {
            copy.properties[property.Key] = property.Value; 
        }
        return copy;
    }

    public double this[string name]
    {
        get => properties[name].Value;
        set => SetPropertyValue(name, value);
    }

}

