namespace EffectSystemPrototype;

public interface IPipelineGroup
{
    public int Count { get; }
}

public class PipelineGroup : IPipelineGroup
{
    private readonly List<ValueEffect> _permanentEffects = new();
    private readonly List<ValueEffect> _generatedEffects = new();
    public int Count => _permanentEffects.Count + _generatedEffects.Count;

    public EffectOp BaseOperator { get; }
    public EffectOp EffectOperator { get; }

    public PipelineGroup(EffectOp baseOp, EffectOp effectOp)
    {
        BaseOperator = baseOp;
        EffectOperator = effectOp;
    }

    public double Calculate(InputVector inputsVector)
    {
        if (EffectOperator == EffectOp.Add)
        {
            return _permanentEffects.Concat(_generatedEffects).Aggregate(0.0, (acc, e) => acc + e.GetValue(inputsVector));
        }
        else if (EffectOperator == EffectOp.Mul)
        {
            return _permanentEffects.Concat(_generatedEffects).Aggregate(1.0, (acc, e) => acc * e.GetValue(inputsVector));
        }
        return 0;
    }

    internal void AddPermanentEffect(ValueEffect effect)
    {
        _permanentEffects.Add(effect);
    }

    internal void AddGeneratedEffect(ValueEffect effect)
    {
        _generatedEffects.Add(effect);
    }

    internal bool RemovePermanentEffect(ValueEffect effect)
    {
        return _permanentEffects.Remove(effect);
    }

    internal void ClearGeneratedEffects()
    {
        _generatedEffects.Clear();
    }
}