using System.Collections.Generic;

namespace AvatarGenerator.Core.Parameters
{
    public class ParameterBag
    {
        private readonly Dictionary<string, ParameterValue> _values = new Dictionary<string, ParameterValue>();
        private readonly Dictionary<string, ParameterIntent> _intents = new Dictionary<string, ParameterIntent>();
        private readonly ParameterSchema _schema;

        public ParameterBag(ParameterSchema schema)
        {
            _schema = schema;
            InitializeDefaults();
        }

        private void InitializeDefaults()
        {
            foreach (var def in _schema.Definitions.Values)
            {
                _values[def.Id] = ParameterValue.CreateDefault(def.DefaultValue);
                _intents[def.Id] = ParameterIntent.Auto();
            }
        }

        public void SetValue(string paramId, float value, ValueSource source = ValueSource.UserOverride)
        {
            if (_values.TryGetValue(paramId, out var existing))
            {
                existing.Value = value;
                existing.Source = source;
                existing.State = source == ValueSource.UserOverride ? ResolutionState.Overridden : existing.State;
                existing.IsDirty = true;
                _values[paramId] = existing;
            }
        }

        public void SetIntent(string paramId, ParameterIntent intent)
        {
            _intents[paramId] = intent;
            if (_values.TryGetValue(paramId, out var existing))
            {
                existing.State = intent.State;
                existing.IsDirty = true;
                _values[paramId] = existing;
            }
        }

        public ParameterValue GetValue(string paramId)
        {
            return _values.TryGetValue(paramId, out var v) ? v : default;
        }

        public ParameterIntent GetIntent(string paramId)
        {
            return _intents.TryGetValue(paramId, out var i) ? i : ParameterIntent.Auto();
        }

        public bool HasUserOverride(string paramId)
        {
            return _values.TryGetValue(paramId, out var v) && v.Source == ValueSource.UserOverride;
        }

        public IReadOnlyDictionary<string, ParameterValue> Values => _values;
        public IReadOnlyDictionary<string, ParameterIntent> Intents => _intents;
        public ParameterSchema Schema => _schema;

        public void MarkDirty(string paramId)
        {
            if (_values.TryGetValue(paramId, out var v))
            {
                v.IsDirty = true;
                _values[paramId] = v;
            }
        }

        public HashSet<string> GetDirtyParams()
        {
            var dirty = new HashSet<string>();
            foreach (var kvp in _values)
            {
                if (kvp.Value.IsDirty)
                    dirty.Add(kvp.Key);
            }
            return dirty;
        }

        public void ClearDirty()
        {
            foreach (var key in _values.Keys)
            {
                var v = _values[key];
                v.IsDirty = false;
                _values[key] = v;
            }
        }
    }
}