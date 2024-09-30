public class EffectSystem
{
    EffectSystemProperties BaseProperties; // Basiswerte
    EffectSystemProperties ProcessedProperties; // Endwerte
    public Dictionary<string, Pipeline> BasePipelines = new(); // Basis Zahleneffekte
    public Dictionary<string, Pipeline> ProcessedPipelines = new(); // End Zahleneffekte
    public LinkedList<MetaEffect> MetaEffects = new(); // Effekte, die Effekte erzeugen
    public Dictionary<string, object> Inputs = new();

    public List<(LinkedListNode<ValueEffect> effect_node, Pipeline pipeline, double end_time)> TimedValueNodes = new();
    public List<(LinkedListNode<MetaEffect> effect_node, double end_time)> TimedMetaNodes = new();

    public double CurrentTime = 0;

    public EffectSystemProperties Properties { get => BaseProperties; }
    public EffectSystemProperties Results { get => ProcessedProperties; }

    public EffectSystem()
    {
        BaseProperties = new EffectSystemProperties(OnPropertyAdded, OnPropertyRemoved);
        ProcessedProperties = BaseProperties.Copy();
    }


    public void AddEffect(Effect effect, double duration = 0)
    {
        AddEffect(effect, BasePipelines);
    }

    void AddEffect(Effect effect, Dictionary<string, Pipeline> pipelines)
    {
        if (effect is ValueEffect valueEffect)
        {
            pipelines[valueEffect.Property].AddEffect(valueEffect);
        }

        if (effect is MetaEffect metaEffect)
        {
            MetaEffects.AddLast(metaEffect);
            TryAddMetaTimer(MetaEffects.Last);
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
        var allProperties = BaseProperties.GetPropertyArray();
        ProcessedProperties = BaseProperties.Copy();
        CopyPipelinesToProcessed(); // Alle Properties und Effekte werden kopiert, damit die ursprünglichen nicht verändert werden

        LinkedList<MetaEffect> newMetaEffects = MetaEffects;
        do
        {
            newMetaEffects =  ApplyMetaEffects(newMetaEffects); // Gibt Meta Effekte zurück, die von Meta effekten erzeugt wurden
        }
        while (newMetaEffects.Count > 0);


        foreach (string property in allProperties)
        {
            double baseValue = BaseProperties.GetValue(property);
            ProcessedProperties[property] = ProcessedPipelines[property].Calculate(baseValue, Inputs);
        }
    }

    public void IncreaseTime(double delta)
    {
        CurrentTime += delta;
    }

    public bool RemoveEffect(Effect effect)
    {
        if (effect is ValueEffect valueEffect)
        {
            return RemoveValueEffect(valueEffect);
        }
        else if (effect is MetaEffect metaEffect)
        {
            return RemoveMetaEffect(metaEffect.Id);
        }
        return false;
    }

    bool RemoveValueEffect(ValueEffect effect)
    {
        var pipeline = BasePipelines[effect.Property];
        return pipeline.RemoveEffect(effect);
    }

    bool RemoveMetaEffect(long effect_id)
    {
        var node = MetaEffects.First;
        for (int i = 0; i < MetaEffects.Count; i++)
        {
            if (node.Value.Id == effect_id)
            {
                MetaEffects.Remove(node);
                return true;
            }
        }
        return false;
    }

    LinkedList<MetaEffect> ApplyMetaEffects(LinkedList<MetaEffect> metaEffects)
    {
        LinkedList<MetaEffect> newMetaEffects = new();
        foreach (MetaEffect metaEffect in metaEffects)
        {
            Effect[] newEffects = metaEffect.Execute(Inputs);
            foreach (Effect effect in newEffects)
            {
                if (effect is MetaEffect newMetaEffect)
                {
                    newMetaEffects.AddLast(newMetaEffect);
                }
                else
                {
                    AddEffect(effect, ProcessedPipelines);
                }
            }
        }
        return newMetaEffects;
    }

    void CopyPipelinesToProcessed()
    {
        ProcessedPipelines = new();
        foreach (var propertyKv in BasePipelines)
        {
            ProcessedPipelines.Add(propertyKv.Key, propertyKv.Value.Copy());
        }
    }

    void TryAddValueTimer(LinkedListNode<ValueEffect> node, Pipeline pipeline)
    {
        if (node.Value.Duration > 0)
        {
            TimedValueNodes.Add((node, pipeline, CurrentTime + node.Value.Duration));
        }
    }

    void TryAddMetaTimer(LinkedListNode<MetaEffect> node)
    {
        if (node.Value.Duration > 0)
        {
            TimedMetaNodes.Add((node, CurrentTime + node.Value.Duration));
        }
    }

    void OnPropertyAdded(string name, bool autoGenGroups)
    {
        Pipeline pipeline = new();
        BasePipelines[name] = pipeline;
        if (autoGenGroups)
        {
            AutoGenerateGroups(name);
        }
    }

    void OnPropertyRemoved(string name)
    {
        BasePipelines.Remove(name);
    }

    void AutoGenerateGroups(string property)
    {
        BasePipelines[property].AddGroup("mul", EffectOp.Mul, EffectOp.Mul);
        BasePipelines[property].AddGroup("add", EffectOp.Add, EffectOp.Add);
    }
}

