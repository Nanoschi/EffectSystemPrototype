namespace EffectSystemPrototype;

public interface IPipelineGroup
{
    ValueEffect[] Effects { get; }
}

public class PipelineGroup : IPipelineGroup
{
    private readonly List<ValueEffect> _effects = new();
    public ValueEffect[] Effects => _effects.ToArray();

    public EffectOp BaseOperator;
    public EffectOp EffectOperator;
    
    public PipelineGroup(EffectOp baseOp, EffectOp effectOp)
    {
        BaseOperator = baseOp;
        EffectOperator = effectOp;
    }

    public double Calculate(InputVector inputsVector)
    {
        if (EffectOperator == EffectOp.Add)
        {
            return Effects.Aggregate(0.0, (acc, e) => acc + e.GetValue(inputsVector));
        }
        else if (EffectOperator == EffectOp.Mul)
        {
            return Effects.Aggregate(1.0, (acc, e) => acc * e.GetValue(inputsVector));
        }
        return 0;
    }

    public void AddEffect(ValueEffect effect)
    {
        _effects.Add(effect);
    }

    public bool RemoveEffect(ValueEffect effect)
    {
        return _effects.Remove(effect);
    }

    public PipelineGroup Copy()
    {
        PipelineGroup group = new(BaseOperator, EffectOperator);
        foreach (var effect in Effects)
        {
            group._effects.Add(effect);
        }
        return group;
    }
}