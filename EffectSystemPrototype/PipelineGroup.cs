public class PipelineGroup
{
    public List<ValueEffect> Effects { get; } = new();
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
        Effects.Add(effect);
    }

    public bool RemoveEffect(ValueEffect effect)
    {
        return Effects.Remove(effect);
    }

    public PipelineGroup Copy()
    {
        PipelineGroup group = new(BaseOperator, EffectOperator);
        foreach (var effect in Effects)
        {
            group.Effects.Add(effect);
        }
        return group;
    }
}