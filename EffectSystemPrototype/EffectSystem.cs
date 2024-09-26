using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


public class EffectSystem
{
    public Dictionary<string, double> baseProperties = new(); // Basiswerte
    public Dictionary<string, double> processedProperties = new(); // Endwerte
    public Dictionary<string, (Pipeline add, Pipeline mul)> basePipelines = new(); // Basis Zahleneffekte
    public Dictionary<string, (Pipeline add, Pipeline mul)> processedPipelines = new(); // End Zahleneffekte
    public LinkedList<MetaEffect> metaEffects = new(); // Effekte, die Effekte erzeugen
    public Dictionary<string, object> inputs = new();

    public List<(LinkedListNode<ValueEffect> effect_node, Pipeline pipeline, double end_time)> timedValueNodes = new();
    public List<(LinkedListNode<MetaEffect> effect_node, double end_time)> timedMetaNodes = new();

    public double currentTime = 0;


    public void AddProperty(string name, double startValue = 0)
    {
        basePipelines.Add(name, (new(EffectOp.Add), new(EffectOp.Mul)));
        baseProperties.Add(name, startValue);
    }

    public void RemoveProperty(string name)
    {
        baseProperties.Remove(name);
        basePipelines.Remove(name);
    }

    public void AddEffect(Effect effect, double duration = 0)
    {
        AddEffect(effect, basePipelines);
    }

    void AddEffect(Effect effect, Dictionary<string, (Pipeline add, Pipeline mul)> pipelines)
    {
        if (effect is ValueEffect valueEffect)
        {
            if (valueEffect.op == EffectOp.Add)
            {
                pipelines[valueEffect.property].add.AddEffect(valueEffect);
                TryAddValueTimer(pipelines[valueEffect.property].add.effects.Last, pipelines[valueEffect.property].add);

            }
            else if (valueEffect.op == EffectOp.Mul)
            {
                pipelines[valueEffect.property].mul.AddEffect(valueEffect);
                TryAddValueTimer(pipelines[valueEffect.property].mul.effects.Last, pipelines[valueEffect.property].mul);
            }
        }

        if (effect is MetaEffect metaEffect)
        {
            metaEffects.AddLast(metaEffect);
            TryAddMetaTimer(metaEffects.Last);
        }
    }

    public void SetInput(string name, object value)
    {
        if (!inputs.ContainsKey(name))
        {
            inputs.Add(name, value);
        }
        else
        {
            inputs[name] = value;
        }

    }

    public void Process()
    {
        RemoveTimedOutEffects();
        var all_properties = baseProperties.Keys.ToArray();
        processedProperties = baseProperties.ToDictionary();
        CopyPipelinesToProcessed(); // Alle Properties und Effekte werden kopiert, damit die ursprünglichen nicht verändert werden

        LinkedList<MetaEffect> new_meta_effects = metaEffects;
        do
        {
            new_meta_effects =  ApplyMetaEffects(new_meta_effects); // Gibt Meta Effekte zurück, die von Meta effekten erzeugt wurden
        }
        while (new_meta_effects.Count > 0);


        foreach (string property in all_properties)
        {
            double base_value = baseProperties[property];
            var multiplied = processedPipelines[property].mul.Calculate(base_value, inputs);
            var final_value = processedPipelines[property].add.Calculate(multiplied, inputs);
            processedProperties[property] = final_value;
        }
    }

    public void IncreaseTime(double delta)
    {
        currentTime += delta;
    }

    public bool RemoveEffect(Effect effect)
    {
        if (effect is ValueEffect value_effect)
        {
            return RemoveValueEffect(value_effect.id, value_effect.property, value_effect.op);
        }
        else if (effect is MetaEffect meta_effect)
        {
            return RemoveMetaEffect(meta_effect.id);
        }
        return false;
    }

    public bool RemoveEffect(long effect_id)
    {
        foreach (var property_kv in basePipelines)
        {
            bool removed = RemoveValueEffect(effect_id, property_kv.Key, EffectOp.Add) ||
                           RemoveValueEffect(effect_id, property_kv.Key, EffectOp.Mul);
            if (removed)
            {
                return true;
            }
        }
        return RemoveMetaEffect(effect_id);
    }

    bool RemoveValueEffect(long effect_id, string property, EffectOp op)
    {
        if (basePipelines.TryGetValue(property, out var pipelines))
        {
            var pipeline = op == EffectOp.Add ? pipelines.add : pipelines.mul;
            return pipeline.RemoveEffect(effect_id);
        }
        return false;
    }

    bool RemoveMetaEffect(long effect_id)
    {
        var node = metaEffects.First;
        for (int i = 0; i < metaEffects.Count; i++)
        {
            if (node.Value.id == effect_id)
            {
                metaEffects.Remove(node);
                return true;
            }
        }
        return false;
    }

    LinkedList<MetaEffect> ApplyMetaEffects(LinkedList<MetaEffect> meta_effects)
    {
        LinkedList<MetaEffect> new_meta_effects = new();
        foreach (MetaEffect meta_effect in metaEffects)
        {
            Effect[] new_effects = meta_effect.Execute(inputs);
            foreach (Effect effect in new_effects)
            {
                if (effect is MetaEffect new_meta_effect)
                {
                    new_meta_effects.AddLast(new_meta_effect);
                }
                else
                {
                    AddEffect(effect, processedPipelines);
                }
            }
        }
        return new_meta_effects;
    }

    void CopyPipelinesToProcessed()
    {
        processedPipelines = new();
        foreach (var property_kv in basePipelines)
        {
            processedPipelines.Add(property_kv.Key, (property_kv.Value.add.Copy(), property_kv.Value.mul.Copy()));
        }
    }

    void TryAddValueTimer(LinkedListNode<ValueEffect> node, Pipeline pipeline)
    {
        if (node.Value.duration > 0)
        {
            timedValueNodes.Add((node, pipeline, currentTime + node.Value.duration));
        }
    }

    void TryAddMetaTimer(LinkedListNode<MetaEffect> node)
    {
        if (node.Value.duration > 0)
        {
            timedMetaNodes.Add((node, currentTime + node.Value.duration));
        }
    }

    void RemoveTimedOutEffects()
    {
        foreach (var node_data in timedValueNodes)
        {
            if (currentTime > node_data.end_time)
            {
                node_data.pipeline.effects.Remove(node_data.effect_node);
            }
        }
        foreach (var node_data in timedMetaNodes)
        {
            if (currentTime > node_data.end_time)
            {
                metaEffects.Remove(node_data.effect_node);
            }
        }
    }
}

