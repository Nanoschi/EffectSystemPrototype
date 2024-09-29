using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class PipelineGroup
{
    LinkedList<ValueEffect> Effects = new();
    public EffectOp BaseOperator;
    public EffectOp EffectOperator;

    public PipelineGroup(EffectOp baseOp, EffectOp effectOp)
    {
        BaseOperator = baseOp;
        EffectOperator = effectOp;
    }

    public double Calculate(double startValue, Dictionary<string, object> inputs)
    {
        if (EffectOperator == EffectOp.Add)
        {
            return Effects.Aggregate(startValue, (acc, e) => acc + e.GetValue(inputs));
        }
        else if (EffectOperator == EffectOp.Mul)
        {
            return Effects.Aggregate(startValue, (acc, e) => acc * e.GetValue(inputs));
        }
        return startValue;
    }

    public void AddEffect(ValueEffect effect)
    {
        Effects.AddLast(effect);
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
    List<PipelineGroup> EffectGroups = new();
    Dictionary<string, PipelineGroup> GroupNames = new();

    public Pipeline()
    {
    }

    public double Calculate(double startValue, Dictionary<string, object> inputs)
    {
        foreach (PipelineGroup group in EffectGroups)
        {
            double value = group.Calculate(startValue, inputs);
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

    public bool RemoveEffect(long effectId)
    {
        return true;
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

