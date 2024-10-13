namespace EffectSystemPrototype;

public class Pipeline
{
    public IPipelineGroup[] EffectGroups => GroupNames.Values.Cast<IPipelineGroup>().ToArray();
    public Dictionary<string, PipelineGroup> GroupNames { get; private set; } = new();
    public string Property { get; private set; }

    public int EffectCount => EffectGroups.Aggregate(0, (acc, g) => acc + g.Count);

    public Pipeline(string property)
    {
        Property = property;
    }

    public double Calculate(double startValue, InputVector inputsVector)
    {
        foreach (var group in GroupNames.Values)
        {
            double value = group.Calculate(inputsVector);
            if (group.BaseOperator == EffectOp.Add)
            {
                startValue += value;
            }
            else if (group.BaseOperator == EffectOp.Mul)
            {
                startValue *= value;
            }
        }
        return startValue;
    }

    internal void AddPermanentEffect(ValueEffect effect)
    {
        GroupNames[effect.GroupName].AddPermanentEffect(effect);
    }

    internal void AddGeneratedEffect(ValueEffect effect)
    {
        GroupNames[effect.GroupName].AddGeneratedEffect(effect);
    }

    public bool RemoveEffect(ValueEffect effect)
    {
        var group = GroupNames[effect.GroupName];
        return group.RemovePermanentEffect(effect);
    }

    public void AddGroup(string name, EffectOp baseOp, EffectOp effectOp)
    {
        PipelineGroup newGroup = new(baseOp, effectOp);
        GroupNames.Add(name, newGroup);
    }

    public bool RemoveGroup(string name)
    {
        return GroupNames.Remove(name);
    }

    public void ClearGeneratedEffects()
    {
        foreach ((_, var group) in GroupNames)
        {
            group.ClearGeneratedEffects();
        }
    }


}