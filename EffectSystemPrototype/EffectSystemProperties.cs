namespace EffectSystemPrototype;

public class EffectSystemProperties
{
    internal Dictionary<string, double> Properties { get; private set; } = new();
    internal List<string> PermanentProperties { get; private set; } = new();

    private readonly Action<string, int, bool> _propertyAdded;
    private readonly Action<string> _propertyRemoved;

    public int Count => Properties.Count;
    public string[] PropertyNames => Properties.Keys.ToArray();

    internal EffectSystemProperties(Action<string, int, bool> propertyAdded, Action<string> propertyRemoved)
    {
        this._propertyAdded = propertyAdded;
        this._propertyRemoved = propertyRemoved;
    }

    public void Add(string name, double value, bool permanent = false, bool autoGenGroups = true)
    {
        if (Properties.TryAdd(name, value))
        {
            _propertyAdded(name, -1, autoGenGroups);
        }
    }

    public void Add(string name, double value, int position, bool permanent = false, bool autoGenGroups = true)
    {
        if (Properties.TryAdd(name, value))
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
        Properties[name] = value;
    }

    public double GetValue(string name)
    {
        return Properties[name];
    }

    public void MakePermanent(string property)
    {
        PermanentProperties.Add(property);
    }

    public bool RemovePermanent(string property)
    {
        return PermanentProperties.Remove(property);
    }

    public bool IsPermanent(string property)
    {
        return PermanentProperties.Contains(property);
    }

    public string[] GetPropertyArray()
    {
        return Properties.Keys.ToArray();
    }
    public double this[string property]
    {
        get => GetValue(property);
        set => SetValue(property, value);
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


}