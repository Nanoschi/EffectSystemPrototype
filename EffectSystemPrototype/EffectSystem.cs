public class EffectSystem
{
    private readonly EffectSystemProperties _baseProperties; // Basiswerte
    private EffectSystemProperties _processedProperties; // Endwerte
    public Dictionary<string, Pipeline> BasePipelines { get; } = new();
    public Dictionary<string, Pipeline> ProcessedPipelines { get; private set; } = new();
    
    public List<MetaEffect> MetaEffects = new(); // Effekte, die Effekte erzeugen
    
    public Dictionary<string, object> Inputs = new();

    public List<(LinkedListNode<ValueEffect> effect_node, Pipeline pipeline, double end_time)> TimedValueNodes = new();
    public List<(LinkedListNode<MetaEffect> effect_node, double end_time)> TimedMetaNodes = new();

    public EffectSystemThresholds EffectThresholds = new ();

    public EffectSystemProperties Properties { get => _baseProperties; }
    public EffectSystemProperties Results { get => _processedProperties; }
    public EffectSystemThresholds Thresholds { get => EffectThresholds; }

    public EffectSystem()
    {
        _baseProperties = new EffectSystemProperties(OnPropertyAdded, OnPropertyRemoved);
        _processedProperties = _baseProperties.Copy();
    }


    public void AddEffect(Effect effect, double duration = 0)
    {
        AddEffect(effect, BasePipelines);
    }

    private void AddEffect(Effect effect, Dictionary<string, Pipeline> pipelines)
    {
        if (effect is ValueEffect valueEffect)
        {
            pipelines[valueEffect.Property].AddEffect(valueEffect);
        }

        if (effect is MetaEffect metaEffect)
        {
            MetaEffects.Add(metaEffect);
        }
    }

    public void SetInput(string name, object value)
    {
        if (!Inputs.ContainsKey(name))
        {
            Inputs.Add(name, value);
        }
        else
        {
            Inputs[name] = value;
        }

    }

    public void Process()
    {
        var allProperties = _baseProperties.GetPropertyArray();
        _processedProperties = _baseProperties.Copy();
        EffectThresholds.RemoveOutOfThreshold(this);
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
            _processedProperties[property] = ProcessedPipelines[property].Calculate(baseValue, Inputs);
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
        var pipeline = BasePipelines[effect.Property];
        return pipeline.RemoveEffect(effect);
    }

    private bool RemoveMetaEffect(MetaEffect effect)
    {
        return MetaEffects.Remove(effect);
    }

    private List<MetaEffect> ApplyMetaEffects(List<MetaEffect> metaEffects)
    {
        List<MetaEffect> newMetaEffects = new();
        foreach (MetaEffect metaEffect in metaEffects)
        {
            Effect[] newEffects = metaEffect.Execute(Inputs);
            foreach (Effect effect in newEffects)
            {
                if (effect is MetaEffect newMetaEffect)
                {
                    newMetaEffects.Add(newMetaEffect);
                }
                else
                {
                    AddEffect(effect, ProcessedPipelines);
                }
            }
        }
        return newMetaEffects;
    }

    private void CopyPipelinesToProcessed()
    {
        ProcessedPipelines = new();
        foreach (var propertyKv in BasePipelines)
        {
            ProcessedPipelines.Add(propertyKv.Key, propertyKv.Value.Copy());
        }
    }

    private void OnPropertyAdded(string name, bool autoGenGroups)
    {
        Pipeline pipeline = new();
        BasePipelines[name] = pipeline;
        if (autoGenGroups)
        {
            AutoGenerateGroups(name);
        }
    }

    private void OnPropertyRemoved(string name)
    {
        BasePipelines.Remove(name);
    }

    private void AutoGenerateGroups(string property)
    {
        BasePipelines[property].AddGroup("mul", EffectOp.Mul, EffectOp.Mul);
        BasePipelines[property].AddGroup("add", EffectOp.Add, EffectOp.Add);
    }
}

