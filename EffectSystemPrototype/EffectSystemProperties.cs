namespace EffectSystemPrototype;

internal delegate void PropertyAddedCallback(string name, int position, bool autoGenGroups);
internal delegate void PropertyRemovedCallback(string name);

internal struct PropertyData
{
    internal double Value { get; set; }
    internal bool IsPermanent { get; set; }

    internal PropertyData(double value, bool isPermanent)
    {
        Value = value;
        IsPermanent = isPermanent;
    }
}

public class EffectSystemProperties
{
    internal Dictionary<string, PropertyData> Properties { get; private set; } = new();

    private readonly PropertyAddedCallback _propertyAdded;
    private readonly PropertyRemovedCallback _propertyRemoved;

    public int Count => Properties.Count;
    public string[] PropertyNames => Properties.Keys.ToArray();

    internal EffectSystemProperties(PropertyAddedCallback propertyAdded, PropertyRemovedCallback propertyRemoved)
    {
        this._propertyAdded = propertyAdded;
        this._propertyRemoved = propertyRemoved;
    }

    public void Add(string name, double value, bool permanent = false, bool autoGenGroups = true)
    {
        if (Properties.TryAdd(name, new(value, permanent)))
        {
            _propertyAdded(name, -1, autoGenGroups);
        }
    }

    public void Add(string name, double value, int position, bool permanent = false, bool autoGenGroups = true)
    {
        if (Properties.TryAdd(name, new(value, permanent)))
        {
            _propertyAdded(name, position, autoGenGroups);
        }
    }

    public void Remove(string name)
    {
        if (Properties.Remove(name))
        {
            _propertyRemoved(name);
        }
    }

    public bool Contains(string name)
    {
        return Properties.ContainsKey(name);
    }

    public void SetValue(string name, double value)
    {
        PropertyData current = Properties[name];
        Properties[name] = new(value, current.IsPermanent);
    }

    public double GetValue(string name)
    {
        return Properties[name].Value;
    }

    public string[] GetPropertyArray()
    {
        return Properties.Keys.ToArray();
    }

    internal EffectSystemProperties Copy()
    {
        EffectSystemProperties copy = new(_propertyAdded, _propertyRemoved);
        foreach (var property in Properties)
        {
            copy.Properties[property.Key] = property.Value; 
        }
        return copy;
    }

    public double this[string name]
    {
        get => Properties[name].Value;
        set => SetValue(name, value);
    }

}