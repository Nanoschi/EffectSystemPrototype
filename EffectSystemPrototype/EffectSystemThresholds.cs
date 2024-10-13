namespace EffectSystemPrototype;

public enum RemoveCondition
{
    Greater,
    Smaller,
    Equals,
    NotEquals,
    Func
}

internal struct EffectThreshold
{
    public Effect Effect { get; }
    public object Limit { get; }
    public RemoveCondition Condition { get; }

    public EffectThreshold(Effect effect, object thresholdValue, RemoveCondition direction)
    {
        Effect = effect;
        Limit = thresholdValue;
        Condition = direction;
    }

    public bool RemoveConditionMet(object currentValue)
    {
        switch (Condition)
        {
            case RemoveCondition.Smaller:
                return Convert.ToDouble(currentValue) < Convert.ToDouble(Limit);
            case RemoveCondition.Greater:
                return Convert.ToDouble(currentValue) > Convert.ToDouble(Limit);
            case RemoveCondition.Equals:
                return Limit == currentValue;
            case RemoveCondition.NotEquals:
                return Limit != currentValue;
            case RemoveCondition.Func:
                if (Limit is Func<object, bool> comp)
                    return comp(currentValue);
                else
                    throw new InvalidCastException("Limit cannot be cast to function.");
        }
        return false;
    }
}

public class EffectSystemThresholds
{
    private List<(string input, EffectThreshold threshold)> Thresholds { get; set; } = new();

    public int Count => Thresholds.Count;

    internal void RemoveOutOfThreshold(EffectSystem system, InputVector inputs)
    { 
        Thresholds = Thresholds.Where(
            t => { 
                var inputValue = inputs[t.input];
                if (t.threshold.RemoveConditionMet(inputValue))
                {
                    system.RemoveEffect(t.threshold.Effect);
                    return false;
                }
                return true; }).ToList();
    }

    public void AddEffect(Effect effect, string inputName, object thresholdValue, RemoveCondition condition)
    {
        if (condition == RemoveCondition.Func)
        {
            AddEffect(effect, inputName, (Func<object, bool>)thresholdValue);
        }
        EffectThreshold threshold = new(effect, thresholdValue, condition);
        Thresholds.Add((inputName, threshold));
    }

    public void AddEffect(Effect effect, string inputName, Func<object, bool> thresholdFunction)
    {
        EffectThreshold threshold = new(effect, thresholdFunction, RemoveCondition.Func);
        Thresholds.Add((inputName, threshold));
    }
}