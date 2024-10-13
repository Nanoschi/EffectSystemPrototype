using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectSystemPrototype
{
    public class EffectSystemPropertyRanges
    {
        private readonly Dictionary<string, (double minValue, double maxValue)> _ranges = new();

        public void SetMaxValue(string property, double value)
        {
            var current = _ranges[property];
            _ranges[property] = (current.minValue, value);
        }

        public void SetMinValue(string property, double value)
        {
            var current = _ranges[property];
            _ranges[property] = (value, current.maxValue);
        }

        public void SetMinMaxValue(string property, double minValue, double maxValue)
        {
            _ranges[property] = (minValue, maxValue);
        }

        public (double min, double max) GetMinMaxValue(string property)
        {
            return _ranges[property];
        }

        public double ClampValue(string property, double value)
        {
            var range = _ranges[property];
            return Math.Clamp(value, range.minValue, range.maxValue);
        }

        internal void Add(string property)
        {
            Add(property, double.NegativeInfinity, double.PositiveInfinity);
        }

        internal void Add(string property, double min, double max)
        {
            _ranges.Add(property, (min, max));
        }

        internal bool Remove(string property)
        {
            return _ranges.Remove(property);
        }
    }
}
