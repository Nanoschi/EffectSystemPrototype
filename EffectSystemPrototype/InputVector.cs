using System.Diagnostics;

namespace EffectSystemPrototype
{
    public class InputVector
    {
        private readonly Dictionary<string, object> _inputs = new();
        
        public object this[string key]
        {
            get => _inputs[key];
            set => _inputs[key] = value;
        }
        
        public bool Contains(string key)
        {
            return _inputs.ContainsKey(key);
        }

        public bool Remove(string name)
        {
            return _inputs.Remove(name);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _inputs.TryGetValue(key, out value);
        }

        public (string Name, object Value)[] Inputs => _inputs.Select(x => (x.Key, x.Value)).ToArray();
    }
}
