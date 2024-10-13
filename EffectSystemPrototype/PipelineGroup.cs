namespace EffectSystemPrototype;

public interface IPipelineGroup
{
    public int Count { get; }
}

public class PipelineGroup : IPipelineGroup
{
    private readonly List<ValueEffect> PermanentEffects = new();
    private readonly List<ValueEffect> GeneratedEffects = new();
    public int Count => PermanentEffects.Count + GeneratedEffects.Count;

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
            return PermanentEffects.Concat(GeneratedEffects).Aggregate(0.0, (acc, e) => acc + e.GetValue(inputsVector));
        }
        else if (EffectOperator == EffectOp.Mul)
        {
            return PermanentEffects.Concat(GeneratedEffects).Aggregate(1.0, (acc, e) => acc * e.GetValue(inputsVector));
        }
        return 0;
    }

    internal void AddPermanentEffect(ValueEffect effect)
    {
        PermanentEffects.Add(effect);
    }

    internal void AddGeneratedEffect(ValueEffect effect)
    {
        GeneratedEffects.Add(effect);
    }

    internal bool RemovePermanentEffect(ValueEffect effect)
    {
        return PermanentEffects.Remove(effect);
    }

    internal void ClearGeneratedEffects()
    {
        GeneratedEffects.Clear();
    }
}