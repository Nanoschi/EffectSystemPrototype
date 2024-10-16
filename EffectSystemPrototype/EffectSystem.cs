﻿using System.Runtime.ExceptionServices;

namespace EffectSystemPrototype;

public class EffectSystem
{
    private readonly Properties _baseProperties;
    private Properties _processedProperties;
    private readonly List<MetaEffect> _metaEffects = new();
    private readonly Pipelines _basePipelines  = new();
    private readonly InputVector _inputVector;

    public int PipelineCount => _basePipelines.Count;

    private Thresholds _effectThresholds = new();
    private PropertyRanges _propertyRanges = new();

    public Properties Properties => _baseProperties;
    public Properties Results => _processedProperties;
    public Thresholds Thresholds => _effectThresholds;
    public PropertyRanges Ranges => _propertyRanges;
    public Pipelines Pipelines => _basePipelines;
    public InputVector Inputs => _inputVector;
    public MetaEffect[] MetaEffects => _metaEffects.ToArray();

    public (string Name, object Value)[] InputVector => _inputVector.Inputs.ToArray();

    public EffectSystem()
    {
        _baseProperties = new Properties(OnPropertyAdded, OnPropertyRemoved);
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
            valueEffect.OnSystemEntered(_inputVector);
            _basePipelines[valueEffect.Property].AddPermanentEffect(valueEffect);
        }

        if (effect is MetaEffect metaEffect)
        {
            _metaEffects.Add(metaEffect);
        }
    }

    public void AddTempEffect(ValueEffect effect)
    {
        effect.OnSystemEntered(_inputVector);
        _basePipelines[effect.Property].AddTemporaryEffect(effect);
    }

    public bool ContainsEffect(Effect effect)
    {
        if (effect is ValueEffect valueEffect)
        {
            return _basePipelines.ContainsEffect(valueEffect);
        }
        else if (effect is MetaEffect metaEffect)
        {
            return MetaEffects.Contains(metaEffect);
        }
        return false;
    }

    public void Process()
    {
        SavePermanentProperties();
        _processedProperties = _baseProperties.Copy();
        Thresholds.RemoveOutOfThreshold(this, _inputVector);

        var newMetaEffects = new List<MetaEffect>(MetaEffects);
        do
        {
            newMetaEffects = ApplyMetaEffects(newMetaEffects);
        }
        while (newMetaEffects.Count > 0);

        foreach (var pipeline in _basePipelines.PipelinesList.Values) // Iterates over properties in pipeline order
        {
            double baseValue = _baseProperties.GetValue(pipeline.Property);
            double result = pipeline.Calculate(baseValue, _inputVector);
            double clampedResult = _propertyRanges.ClampValue(pipeline.Property, result);
            _processedProperties[pipeline.Property] = clampedResult;
        }
        _basePipelines.ClearTemporaryEffects(_inputVector);
    }

    public bool RemoveEffect(Effect effect)
    {
        if (effect is ValueEffect valueEffect)
        {
            RemoveValueEffect(valueEffect);
        }

        if (effect is MetaEffect metaEffect)
        {
            return RemoveMetaEffect(metaEffect);
        }
        return false;
    }

    private void SavePermanentProperties()
    {
        foreach (string property in Results.PermanentProperties)
        {
            if (Results.Contains(property)) // ignore properties that haven't been copied to results yet
            {
                Properties[property] = Results[property];
            }
        }
    }

    private bool RemoveValueEffect(ValueEffect effect)
    {
        var pipeline = _basePipelines[effect.Property];
        bool removed = pipeline.RemoveEffect(effect, _inputVector);

        return removed;
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
                    AddTempEffect(valueEffect);
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
        