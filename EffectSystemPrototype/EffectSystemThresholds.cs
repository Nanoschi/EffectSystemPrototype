using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum LimitDirection
{
    Min,
    Max
}

public struct EffectThreshold
{
    public Effect Effect;
    public double Limit;
    public LimitDirection Direction;

    public EffectThreshold(Effect effect, double limit, LimitDirection direction)
    {
        this.Effect = effect;
        this.Limit = limit;
        this.Direction = direction;
    }

    public bool IsOutOfLimit(double currentValue)
    {
        switch (Direction)
        {
            case LimitDirection.Max:
                return currentValue > Limit;
            case LimitDirection.Min:
                return currentValue < Limit;
        }
        return false;
    }
}

public class EffectSystemThresholds
{
    public Dictionary<string, (LinkedList<EffectThreshold> effects, double currentValue)> Thresholds = new();

    public void RemoveOutOfThreshold(EffectSystem system)
    {
        foreach (var kv in Thresholds)
        {
            foreach (EffectThreshold threshold in kv.Value.effects)
            {
                if (threshold.IsOutOfLimit(kv.Value.currentValue))
                {
                    system.RemoveEffect(threshold.Effect);
                }
            }
        }
    }

    public void AddEffect(Effect effect, string valueName, double threshold, LimitDirection direction)
    {
        Thresholds[valueName].effects.AddLast(new EffectThreshold(effect, threshold, direction));
    }

    public void AddValue(string name, double startValue = 0)
    {
        if (!Thresholds.TryAdd(name, (new(), startValue)))
        {
            throw new ArgumentException($"Value name {name} already  exists");
        }
    }

    public bool RemoveValue(string name)
    {
        return Thresholds.Remove(name);
    }

    public void SetValue(string name, double newValue)
    {
        if (Thresholds.TryGetValue(name, out var pair))
        {
            Thresholds[name] = (pair.effects, newValue);
        }
        else
        {
            throw new ArgumentException($"Value name {name} not found");
        }
    }

    public void IncValue(string name, double delta)
    {
        if (Thresholds.TryGetValue(name, out var pair))
        {
            Thresholds[name] = (pair.effects, pair.currentValue + delta);
        }
        else
        {
            throw new ArgumentException($"Value name {name} not found");
        }
    }
}

