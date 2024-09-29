using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PipelineGroup
{
    public LinkedList<ValueEffect> Effects = new();
    public EffectOp BaseOperator;
    public EffectOp EffectOperator;

    public PipelineGroup(EffectOp baseOp, EffectOp effectOp)
    {
        BaseOperator = baseOp;
        EffectOperator = effectOp;
    }

    public double Calculate(Dictionary<string, object> inputs)
    {
        if (EffectOperator == EffectOp.Add)
        {
            return Effects.Aggregate(0.0, (acc, e) => acc + e.GetValue(inputs));
        }
        else if (EffectOperator == EffectOp.Mul)
        {
            return Effects.Aggregate(1.0, (acc, e) => acc * e.GetValue(inputs));
        }
        return 0;
    }

    public void AddEffect(ValueEffect effect)
    {
        Effects.AddLast(effect);
    }

    public bool RemoveEffect(ValueEffect effect)
    {
        return Effects.Remove(effect);
    }

    public PipelineGroup Copy()
    {
        PipelineGroup newList = new(BaseOperator, EffectOperator);
        foreach (var effect in Effects)
        {
            newList.Effects.AddLast(effect);
        }
        return newList;
    }
}

public class Pipeline
{
    public List<PipelineGroup> EffectGroups = new();
    public Dictionary<string, PipelineGroup> GroupNames = new();

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
        EffectGroups.Add(newGroup);
    }

    public Pipeline Copy()
    {
        Pipeline copy = new();
        foreach (PipelineGroup group in EffectGroups)
        {
            copy.EffectGroups.Add(group);
        }
        copy.GroupNames = GroupNames;
        return copy;
    }


}

