using System.Collections.Generic;

namespace AvatarGenerator.Core.Resolution
{
    public struct ParameterOverrides
    {
        private readonly Dictionary<string, (float value, ValueSource source)> _overrides;

        public ParameterOverrides(int capacity = 16)
        {
            _overrides = new Dictionary<string, (float, ValueSource)>(capacity);
        }

        public void Set(string paramId, float value, ValueSource source)
        {
            if (_overrides.TryGetValue(paramId, out var existing))
            {
                if (source >= existing.source)
                {
                    _overrides[paramId] = (value, source);
                }
            }
            else
            {
                _overrides[paramId] = (value, source);
            }
        }

        public bool TryGet(string paramId, out float value)
        {
            if (_overrides.TryGetValue(paramId, out var v))
            {
                value = v.value;
                return true;
            }
            value = 0f;
            return false;
        }

        public ValueSource GetSource(string paramId)
        {
            return _overrides.TryGetValue(paramId, out var v) ? v.source : ValueSource.Default;
        }

        public IEnumerable<string> Keys => _overrides.Keys;
        public int Count => _overrides.Count;
        public void Clear() => _overrides.Clear();
    }
}