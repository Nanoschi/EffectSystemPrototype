namespace EffectSystemPrototype;

public class EffectSystem
{
    private readonly EffectSystemProperties _baseProperties; // Basiswerte
    private EffectSystemProperties _processedProperties; // Endwerte
    private readonly List<MetaEffect> _metaEffects = new(); // Effekte, die Effekte erzeugen
    private readonly EffectSystemPipelines _basePipelines  = new();
    private EffectSystemPipelines _processedPipelines  = new();
    private readonly InputVector _inputVector = new();

    public int PipelineCount => _basePipelines.Count;

    public EffectSystemThresholds EffectThresholds = new ();

    public EffectSystemProperties Properties { get => _baseProperties; }
    public EffectSystemProperties Results { get => _processedProperties; }
    public EffectSystemThresholds Thresholds { get => EffectThresholds; }

    public MetaEffect[] MetaEffects => _metaEffects.ToArray();

    public (string Name, object Value)[] InputVector => _inputVector.Inputs.ToArray();

    public EffectSystem()
    {
        _baseProperties = new EffectSystemProperties(OnPropertyAdded, OnPropertyRemoved);
        _processedProperties = _baseProperties.Copy();
    }

    public ValueEffect[] GetEffectsOfGroup(string property, string group)
    {
        return _basePipelines[property].GroupNames[group].Effects.ToArray();
    }
    public void AddEffect(Effect effect, double duration = 0)
    {
        AddEffect(effect, _basePipelines);
    }

    private void AddEffect(Effect effect, EffectSystemPipelines pipelines)
    {
        if (effect is ValueEffect valueEffect)
        {
            pipelines[valueEffect.Property].AddEffect(valueEffect);
        }

        if (effect is MetaEffect metaEffect)
        {
            _metaEffects.Add(metaEffect);
        }
    }

    public void SetInput(string name, object value)
    {
        _inputVector[name] = value;
    }

    public bool RemoveInput(string name)
    {
        return _inputVector.Remove(name);
    }

    public void Process()
    {
        var allProperties = _baseProperties.GetPropertyArray();
        _processedProperties = _baseProperties.Copy();
        Thresholds.RemoveOutOfThreshold(this);
        CopyPipelinesToProcessed(); // Alle Properties und Effekte werden kopiert, damit die ursprünglichen nicht verändert werden

        var newMetaEffects = new List<MetaEffect>(MetaEffects);
        do
        {
            newMetaEffects =  ApplyMetaEffects(newMetaEffects); // Gibt Meta Effekte zurück, die von Meta effekten erzeugt wurden
        }
        while (newMetaEffects.Count > 0);

        foreach (string property in allProperties)
        {
            double baseValue = _baseProperties.GetValue(property);
            _processedProperties[property] = _processedPipelines[property].Calculate(baseValue, _inputVector);
        }
    }

    public bool RemoveEffect(Effect effect)
    {
        if (effect is ValueEffect valueEffect)
        {
            return RemoveValueEffect(valueEffect);
        }
        else if (effect is MetaEffect metaEffect)
        {
            return RemoveMetaEffect(metaEffect);
        }
        return false;
    }

    private bool RemoveValueEffect(ValueEffect effect)
    {
        var pipeline = _basePipelines[effect.Property];
        return pipeline.RemoveEffect(effect);
    }

    private bool RemoveMetaEffect(MetaEffect effect)
    {
        return _metaEffects.Remove(effect);
    }

    private List<MetaEffect> ApplyMetaEffects(List<MetaEffect> metaEffects)
    {
        List<MetaEffect> newMetaEffects = new();
        foreach (MetaEffect metaEffect in metaEffects)
        {
            var newEffects = metaEffect.Execute(_inputVector);
            foreach (Effect effect in newEffects)
            {
                if (effect is MetaEffect newMetaEffect)
                {
                    newMetaEffects.Add(newMetaEffect);
                }
                else
                {
                    AddEffect(effect, _processedPipelines);
                }
            }
        }
        return newMetaEffects;
    }

    private void CopyPipelinesToProcessed()
    {
        _processedPipelines = new();
        _processedPipelines = _basePipelines.Copy();
    }

    private void OnPropertyAdded(string name, bool autoGenGroups)
    {
        Pipeline pipeline = new();
        _basePipelines[name] = pipeline;
        if (autoGenGroups)
        {
            AutoGenerateGroups(name);
        }
    }

    private void OnPropertyRemoved(string name)
    {
        _basePipelines.Remove(name);
    }

    private void AutoGenerateGroups(string property)
    {
        _basePipelines[property].AddGroup("mul", EffectOp.Mul, EffectOp.Mul);
        _basePipelines[property].AddGroup("add", EffectOp.Add, EffectOp.Add);
    }

    public void AddGroup(string property, string group, EffectOp baseOp, EffectOp effectOp)
    {
        _basePipelines[property].AddGroup(group, baseOp, effectOp);
    }

    public void RemoveGroup(string property, string group)
    {
        _basePipelines[property].RemoveGroup(group);
    }

    public IPipelineGroup[] GetGroups(string property)
    {
        return _basePipelines[property].EffectGroups;
    }

    public bool TryGetInputValue<T>(string name, out T value)
    {
        return _inputVector.TryGetValue(name, out value);
    }

    public GroupConfiguration CreateConfig(string property, double value)
    {
        return new GroupConfiguration(property, value, this);
    }

    internal void InternalAddGroupConfig(GroupConfiguration configuration)
    {
        Properties.Add(configuration.Property, configuration.Value, false);
        foreach (var grp in configuration.PipelineGroups)
        {
            AddGroup(configuration.Property,grp.Name, grp.PipelineGroup.BaseOperator, grp.PipelineGroup.EffectOperator);
        }

        foreach (var effect in configuration.Effects)
        {
            AddEffect(effect);
        }
    }
}
        