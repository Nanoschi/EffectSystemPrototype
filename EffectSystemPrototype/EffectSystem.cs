namespace EffectSystemPrototype;

public class EffectSystem
{
    private readonly EffectSystemProperties _baseProperties; // Basiswerte
    private EffectSystemProperties _processedProperties; // Endwerte
    private readonly List<MetaEffect> _metaEffects = new(); // Effekte, die Effekte erzeugen
    private readonly EffectSystemPipelines _basePipelines  = new();
    private readonly InputVector _inputVector;

    public int PipelineCount => _basePipelines.Count;

    private EffectSystemThresholds _effectThresholds = new();
    private EffectSystemPropertyRanges _propertyRanges = new();

    public EffectSystemProperties Properties => _baseProperties;
    public EffectSystemProperties Results => _processedProperties;
    public EffectSystemThresholds Thresholds => _effectThresholds;
    public EffectSystemPropertyRanges Ranges => _propertyRanges;
    public EffectSystemPipelines Pipelines => _basePipelines;
    public InputVector Inputs => _inputVector;
    public MetaEffect[] MetaEffects => _metaEffects.ToArray();

    public (string Name, object Value)[] InputVector => _inputVector.Inputs.ToArray();

    public EffectSystem()
    {
        _baseProperties = new EffectSystemProperties(OnPropertyAdded, OnPropertyRemoved);
        _processedProperties = _baseProperties.Copy();
        _inputVector = new InputVector(this);
    }

    public int GetGroupEffectCount(string property, string group)
    {
        return _basePipelines[property].GroupNames[group].Count;
    }

    public void AddEffect(Effect effect)
    {
        if (effect is ValueEffect valueEffect)
        {
            _basePipelines[valueEffect.Property].AddPermanentEffect(valueEffect);
        }

        if (effect is MetaEffect metaEffect)
        {
            _metaEffects.Add(metaEffect);
        }
    }

    public void AddTempEffect(ValueEffect effect)
    {
        _basePipelines[effect.Property].AddGeneratedEffect(effect);
    }

    public void Process()
    {
        _processedProperties = _baseProperties.Copy();
        Thresholds.RemoveOutOfThreshold(this, _inputVector);

        var newMetaEffects = new List<MetaEffect>(MetaEffects);
        do
        {
            newMetaEffects =  ApplyMetaEffects(newMetaEffects); // Gibt Meta Effekte zurück, die von Meta effekten erzeugt wurden
        }
        while (newMetaEffects.Count > 0);

        foreach (var pipeline in _basePipelines.Pipelines.Values) // Iterates over properties in pipeline order
        {
            double baseValue = _baseProperties.GetValue(pipeline.Property);
            double result = pipeline.Calculate(baseValue, _inputVector);
            double clampedResult = _propertyRanges.ClampValue(pipeline.Property, result);
            _processedProperties[pipeline.Property] = clampedResult;
        }
        _basePipelines.ClearGeneratedEffects();
    }

    public bool RemoveEffect(Effect effect)
    {
        if (effect is ValueEffect valueEffect)
        {
            return RemoveValueEffect(valueEffect);
        }

        if (effect is MetaEffect metaEffect)
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
                else if (effect is ValueEffect valueEffect)
                {
                    _basePipelines[valueEffect.Property].AddGeneratedEffect(valueEffect);
                }
            }
        }
        return newMetaEffects;
    }

    private void OnPropertyAdded(string property, int position, bool autoGenGroups)
    {
        Pipeline pipeline = new(property);
       
        _basePipelines.Add(property, pipeline, position);
        if (autoGenGroups)
        {
            AutoGenerateGroups(property);
        }
        _propertyRanges.Add(property);
    }

    private void OnPropertyRemoved(string property)
    {
        _basePipelines.Remove(property);
        _propertyRanges.Remove(property);
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
        