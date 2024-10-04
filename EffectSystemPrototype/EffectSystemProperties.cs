using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public delegate void PropertyAddedCallback(string name, bool autoGenGroups);
public delegate void PropertyRemovedCallback(string name);

public class EffectSystemProperties
{
    public Dictionary<string, (double Value, double MinValue, double MaxValue)> properties = new();

    private PropertyAddedCallback propertyAdded;
    private PropertyRemovedCallback propertyRemoved;

    public int Count
    {
        get => properties.Count;
    }

    public EffectSystemProperties(PropertyAddedCallback propertyAdded, PropertyRemovedCallback propertyRemoved)
    {
        this.propertyAdded = propertyAdded;
        this.propertyRemoved = propertyRemoved;
    }

    
    public void Add(string name, double value = 0, bool autoGenGroups = true)
    {
        Add(name, value, double.NegativeInfinity, double.PositiveInfinity, autoGenGroups);
    }

    public void Add(string name, double value, double min, double max, bool autoGenGroups = true)
    {
        if (properties.TryAdd(name, (value, min, max)))
        {
            propertyAdded(name, autoGenGroups);
        }
    }

    public void Remove(string name)
    {
        if (properties.Remove(name))
        {
            propertyRemoved(name);
        }
    }

    public bool Contains(string name)
    {
        return properties.ContainsKey(name);
    }

    public void SetValue(string name, double value)
    {
        if (properties.TryGetValue(name, out var currentValue))
        {
            currentValue.Value = Math.Clamp(value, currentValue.MinValue, currentValue.MaxValue); 
            properties[name] = currentValue;
        }
    }

    public double GetValue(string name)
    {
        if (properties.TryGetValue(name, out var value))
        {
            return value.Value;
        }
        return 0;
    }

    public void OverrideValue(string name, double value)
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
        EffectSystemProperties copy = new(propertyAdded, propertyRemoved);
        foreach (var property in properties)
        {
            copy.properties[property.Key] = property.Value; 
        }
        return copy;
    }

    public double this[string name]
    {
        get => properties[name].Value;
        set => SetValue(name, value);
    }

}

