using Microsoft.VisualBasic.CompilerServices;

namespace EffectSystemPrototype
{
    public class InputVector
    {
        private readonly Dictionary<string, object> _inputs = new();
        private readonly EffectSystem _effectSystem;

        public InputVector(EffectSystem effectSystem)
        {
            _effectSystem = effectSystem;
        }
        
        public object this[string key]
        {
            get => _inputs[key];
            set => _inputs[key] = value;
        }

        public double PropertyValue(string property)
        {
            return _effectSystem.Results[property];
        }
        
        public bool Contains(string key)
        {
            return _inputs.ContainsKey(key);
        }

        public bool Remove(string name)
        {
            return _inputs.Remove(name);
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            if (_inputs.ContainsKey(key))
            {
                try
                {
                    value = (T)_inputs[key];
                    return true;
                }
                catch (Exception)
                {
                    value = default;
                    return false;
                }
            }
            value = default;
            return false;
        }

        public (string Name, object Value)[] Inputs => _inputs.Select(x => (x.Key, x.Value)).ToArray();
    }
}
