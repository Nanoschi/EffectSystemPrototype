using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Pipeline
{
    public PipelineGroup[] EffectGroups => GroupNames.Values.ToArray();
    public Dictionary<string, PipelineGroup> GroupNames { get; private set; } = new();

    public Pipeline()
    {
        
    }
    public int EffectCount { get => EffectGroups.Aggregate(0, (acc, g) => acc + g.Effects.Count); }

    public double Calculate(double startValue, Dictionary<string, object> inputs)
    {
        foreach (PipelineGroup group in EffectGroups)
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

