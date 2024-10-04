namespace EffectSystemPrototype;

internal delegate void PropertyAddedCallback(string name, bool autoGenGroups);
internal delegate void PropertyRemovedCallback(string name);

public class EffectSystemProperties
{
    public Dictionary<string, (double Value, double MinValue, double MaxValue)> properties = new();

    private readonly PropertyAddedCallback _propertyAdded;
    private readonly PropertyRemovedCallback _propertyRemoved;

    public int Count => properties.Count;

    internal EffectSystemProperties(PropertyAddedCallback propertyAdded, PropertyRemovedCallback propertyRemoved)
    {
        this._propertyAdded = propertyAdded;
        this._propertyRemoved = propertyRemoved;
    }

    
    public void Add(string name, double value = 0, bool autoGenGroups = true)
    {
        Add(name, value, double.NegativeInfinity, double.PositiveInfinity, autoGenGroups);
    }

    public void Add(string name, double value, double min, double max, bool autoGenGroups = true)
    {
        if (properties.TryAdd(name, (value, min, max)))
        {
            _propertyAdded(name, autoGenGroups);
        }
    }

    public void Remove(string name)
    {
        if (properties.Remove(name))
        {
            _propertyRemoved(name);
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
    internal EffectSystemProperties Copy()
    {
        EffectSystemProperties copy = new(_propertyAdded, _propertyRemoved);
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