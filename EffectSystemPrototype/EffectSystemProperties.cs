namespace EffectSystemPrototype;

internal delegate void PropertyAddedCallback(string name, int position, bool autoGenGroups);
internal delegate void PropertyRemovedCallback(string name);

public class EffectSystemProperties
{
    public Dictionary<string, double> properties = new();

    private readonly PropertyAddedCallback _propertyAdded;
    private readonly PropertyRemovedCallback _propertyRemoved;

    public int Count => properties.Count;

    internal EffectSystemProperties(PropertyAddedCallback propertyAdded, PropertyRemovedCallback propertyRemoved)
    {
        this._propertyAdded = propertyAdded;
        this._propertyRemoved = propertyRemoved;
    }

    public void Add(string name, double value, bool autoGenGroups = true)
    {
        if (properties.TryAdd(name, value))
        {
            _propertyAdded(name, -1, autoGenGroups);
        }
    }

    public void Add(string name, double value, int position, bool autoGenGroups = true)
    {
        if (properties.TryAdd(name, value))
        {
            _propertyAdded(name, position, autoGenGroups);
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
        properties[name] = value;
    }

    public double GetValue(string name)
    {
        return properties[name];
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
        get => properties[name];
        set => SetValue(name, value);
    }

}