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

    internal void AddTemporaryEffect(ValueEffect effect)
    {
        GroupNames[effect.GroupName].AddTemporaryEffect(effect);
    }

    public bool RemoveEffect(ValueEffect effect, InputVector inputs)
    {
        var group = GroupNames[effect.GroupName];
        return group.RemovePermanentEffect(effect, inputs);
    }

    public bool ContainsEffect(ValueEffect effect)
    {
        return GroupNames[effect.GroupName].ContainsEffect(effect);
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

    public void ClearTemporaryEffects(InputVector inputs)
    {
        foreach (var group in GroupNames.Values)
        {
            group.ClearTemporaryEffects(inputs);
        }
    }


}