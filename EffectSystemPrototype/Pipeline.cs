namespace EffectSystemPrototype;

internal class Pipeline
{
    public IPipelineGroup[] EffectGroups => GroupNames.Values.Cast<IPipelineGroup>().ToArray();
    public Dictionary<string, PipelineGroup> GroupNames { get; private set; } = new();

    public int EffectCount => EffectGroups.Aggregate(0, (acc, g) => acc + g.Effects.Length);

    public double Calculate(double startValue, Dictionary<string, object> inputs)
    {
        foreach (var group in GroupNames.Values)
        {
            double value = group.Calculate(inputs);
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

    public void AddEffect(ValueEffect effect)
    {
        if (GroupNames.TryGetValue(effect.GroupName, out var group)) {
            group.AddEffect(effect);
        }
        else
        {
            throw new ArgumentException($"Group '{effect.GroupName}' not found in pipeline");
        }
    }

    public bool RemoveEffect(ValueEffect effect)
    {
        var group = GroupNames[effect.GroupName];
        return group.RemoveEffect(effect);
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

    public Pipeline Copy()
    {
        Pipeline copy = new();
        var kvpCopies = GroupNames.ToArray().Select(x => new KeyValuePair<string, PipelineGroup>(x.Key, x.Value.Copy()));
        copy.GroupNames = new Dictionary<string, PipelineGroup>(kvpCopies);
       
        return copy;
    }


}