using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class EffectSystem
{
    public Dictionary<string, double> baseProperties = new(); // Basiswerte
    public Dictionary<string, double> processedProperties = new(); // Endwerte
    public Dictionary<string, (Pipeline add, Pipeline mul)> basePipelines = new(); // Basis Zahleneffekte
    public Dictionary<string, (Pipeline add, Pipeline mul)> processedPipelines = new(); // End Zahleneffekte
    public List<MetaEffect> metaEffects = new(); // Effekte, die Effekte erzeugen
    public Dictionary<string, object> inputs = new();


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

    public void AddEffect(Effect effect)
    {
        AddEffect(effect, basePipelines);
    }

    void AddEffect(Effect effect, Dictionary<string, (Pipeline add, Pipeline mul)> pipelines)
    {
        var value_effect = effect as ValueEffect;
        if (value_effect != null)
        {
            if (value_effect.op == EffectOp.Add)
            {
                pipelines[value_effect.property].add.AddValue(value_effect.GetValue(inputs));
            }
            else if (value_effect.op == EffectOp.Mul)
            {
                pipelines[value_effect.property].mul.AddValue(value_effect.GetValue(inputs));
            }
        }
        var meta_effect = effect as MetaEffect;
        if (meta_effect != null)
        {
            metaEffects.Add(meta_effect);
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
        var all_properties = baseProperties.Keys.ToArray();
        processedProperties = baseProperties.ToDictionary();
        CopyPipelinesToProcessed(); // Alle Properties und Effekte werden kopiert, damit die ursprünglichen nicht verändert werden
        ApplyMetaEffects();

        foreach (string property in all_properties)
        {
            double base_value = baseProperties[property];
            var multiplied = processedPipelines[property].mul.Calculate(base_value);
            var final_value = processedPipelines[property].add.Calculate(multiplied);
            processedProperties[property] = final_value;
        }
    }

    void ApplyMetaEffects()
    {
        foreach (MetaEffect meta_effect in metaEffects)
        {
            Effect[] new_effects = meta_effect.Execute(inputs);
            foreach (Effect effect in new_effects)
            {
                AddEffect(effect, processedPipelines);
            }
        }
    }

    void CopyPipelinesToProcessed()
    {
        processedPipelines = new();
        foreach (var property_kv in basePipelines)
        {
            processedPipelines.Add(property_kv.Key, (property_kv.Value.add.Copy(), property_kv.Value.mul.Copy()));
        }
    } 
}

