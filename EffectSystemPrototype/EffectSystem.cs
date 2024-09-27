public class EffectSystem
{
    public Dictionary<string, double> BaseProperties = new(); // Basiswerte
    public Dictionary<string, double> ProcessedProperties = new(); // Endwerte
    public Dictionary<string, (Pipeline add, Pipeline mul)> BasePipelines = new(); // Basis Zahleneffekte
    public Dictionary<string, (Pipeline add, Pipeline mul)> ProcessedPipelines = new(); // End Zahleneffekte
    public LinkedList<MetaEffect> MetaEffects = new(); // Effekte, die Effekte erzeugen
    public Dictionary<string, object> Inputs = new();

    public List<(LinkedListNode<ValueEffect> effect_node, Pipeline pipeline, double end_time)> TimedValueNodes = new();
    public List<(LinkedListNode<MetaEffect> effect_node, double end_time)> TimedMetaNodes = new();

    public double CurrentTime = 0;


    public void AddProperty(string name, double startValue = 0)
    {
        BasePipelines.Add(name, (new(EffectOp.Add), new(EffectOp.Mul)));
        BaseProperties.Add(name, startValue);
    }

    public void RemoveProperty(string name)
    {
        BaseProperties.Remove(name);
        BasePipelines.Remove(name);
    }

    public void AddEffect(Effect effect, double duration = 0)
    {
        AddEffect(effect, BasePipelines);
    }

    void AddEffect(Effect effect, Dictionary<string, (Pipeline add, Pipeline mul)> pipelines)
    {
        if (effect is ValueEffect valueEffect)
        {
            if (valueEffect.Op == EffectOp.Add)
            {
                pipelines[valueEffect.Property].add.AddEffect(valueEffect);
                TryAddValueTimer(pipelines[valueEffect.Property].add.Effects.Last, pipelines[valueEffect.Property].add);

            }
            else if (valueEffect.Op == EffectOp.Mul)
            {
                pipelines[valueEffect.Property].mul.AddEffect(valueEffect);
                TryAddValueTimer(pipelines[valueEffect.Property].mul.Effects.Last, pipelines[valueEffect.Property].mul);
            }
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
        RemoveTimedOutEffects();
        var all_properties = BaseProperties.Keys.ToArray();
        ProcessedProperties = BaseProperties.ToDictionary();
        CopyPipelinesToProcessed(); // Alle Properties und Effekte werden kopiert, damit die ursprünglichen nicht verändert werden

        LinkedList<MetaEffect> newMetaEffects = MetaEffects;
        do
        {
            newMetaEffects =  ApplyMetaEffects(newMetaEffects); // Gibt Meta Effekte zurück, die von Meta effekten erzeugt wurden
        }
        while (newMetaEffects.Count > 0);


        foreach (string property in all_properties)
        {
            double base_value = BaseProperties[property];
            var multiplied = ProcessedPipelines[property].mul.Calculate(base_value, Inputs);
            var final_value = ProcessedPipelines[property].add.Calculate(multiplied, Inputs);
            ProcessedProperties[property] = final_value;
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
            return RemoveValueEffect(valueEffect.Id, valueEffect.Property, valueEffect.Op);
        }
        else if (effect is MetaEffect metaEffect)
        {
            return RemoveMetaEffect(metaEffect.Id);
        }
        return false;
    }

    public bool RemoveEffect(long effectId)
    {
        foreach (var propertyKv in BasePipelines)
        {
            bool removed = RemoveValueEffect(effectId, propertyKv.Key, EffectOp.Add) ||
                           RemoveValueEffect(effectId, propertyKv.Key, EffectOp.Mul);
            if (removed)
            {
                return true;
            }
        }
        return RemoveMetaEffect(effectId);
    }

    bool RemoveValueEffect(long effect_id, string property, EffectOp op)
    {
        if (BasePipelines.TryGetValue(property, out var pipelines))
        {
            var pipeline = op == EffectOp.Add ? pipelines.add : pipelines.mul;
            return pipeline.RemoveEffect(effect_id);
        }
        return false;
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
        foreach (MetaEffect metaEffect in MetaEffects)
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
            ProcessedPipelines.Add(propertyKv.Key, (propertyKv.Value.add.Copy(), propertyKv.Value.mul.Copy()));
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

    void RemoveTimedOutEffects()
    {
        foreach (var nodeData in TimedValueNodes)
        {
            if (CurrentTime > nodeData.end_time)
            {
                nodeData.pipeline.Effects.Remove(nodeData.effect_node);
            }
        }
        foreach (var nodeData in TimedMetaNodes)
        {
            if (CurrentTime > nodeData.end_time)
            {
                MetaEffects.Remove(nodeData.effect_node);
            }
        }
    }
}

