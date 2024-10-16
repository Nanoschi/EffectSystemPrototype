namespace EffectSystemPrototype;

public class Properties
{
    internal Dictionary<string, double> Values { get; private set; } = new();
    internal List<string> PermanentProperties { get; private set; } = new();

    private readonly Action<string, int, bool> _propertyAdded;
    private readonly Action<string> _propertyRemoved;

    public int Count => Values.Count;
    public string[] PropertyNames => Values.Keys.ToArray();

    internal Properties(Action<string, int, bool> propertyAdded, Action<string> propertyRemoved)
    {
        this._propertyAdded = propertyAdded;
        this._propertyRemoved = propertyRemoved;
    }

    public void Add(string name, double value, bool permanent = false, bool autoGenGroups = true)
    {
        if (Values.TryAdd(name, value))
        {
            _propertyAdded(name, -1, autoGenGroups);
        }
    }

    public void Add(string name, double value, int position, bool permanent = false, bool autoGenGroups = true)
    {
        if (Values.TryAdd(name, value))
        {
            _propertyAdded(name, position, autoGenGroups);
        }
    }

    public void Remove(string name)
    {
        if (Values.Remove(name))
        {
            _propertyRemoved(name);
        }
    }

    public bool Contains(string name)
    {
        return Values.ContainsKey(name);
    }

    public void SetValue(string name, double value)
    {
        Values[name] = value;
    }

    public double GetValue(string name)
    {
        return Values[name];
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
        return Values.Keys.ToArray();
    }

    public double this[string property]
    {
        get => GetValue(property);
        set => SetValue(property, value);
    }

    internal Properties Copy()
    {
        Properties copy = new(_propertyAdded, _propertyRemoved);
        foreach (var property in Values)
        {
            copy.Values[property.Key] = property.Value; 
        }
        copy.PermanentProperties = PermanentProperties;
        return copy;
    }


}