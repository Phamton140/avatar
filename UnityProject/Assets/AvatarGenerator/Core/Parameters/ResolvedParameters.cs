using System.Collections.Generic;
using UnityEngine;

namespace AvatarGenerator.Core.Parameters
{
    public interface IResolvedParameters : IReadOnlyDictionary<string, float>
    {
        float GetFloat(string id);
        bool HasUserOverride(string id);
        Hash128 ComputeHash();
        IEnumerable<string> GetChangedParams(IResolvedParameters previous);
        bool TryGetValue(string key, out float value);
    }

    public class ResolvedParameters : IResolvedParameters
    {
        private readonly Dictionary<string, float> _values = new Dictionary<string, float>();
        private readonly HashSet<string> _userOverrides = new HashSet<string>();

        public float this[string key] => _values[key];
        public IEnumerable<string> Keys => _values.Keys;
        public IEnumerable<float> Values => _values.Values;
        public int Count => _values.Count;

        public void Set(string key, float value, bool isUserOverride = false)
        {
            _values[key] = value;
            if (isUserOverride)
                _userOverrides.Add(key);
            else
                _userOverrides.Remove(key);
        }

        public bool TryGetValue(string key, out float value)
        {
            return _values.TryGetValue(key, out value);
        }

        public float GetFloat(string id)
        {
            return _values.TryGetValue(id, out var value) ? value : 0f;
        }

        public bool HasUserOverride(string id)
        {
            return _userOverrides.Contains(id);
        }

        public Hash128 ComputeHash()
        {
            var hash = new Hash128();
            foreach (var kvp in _values)
            {
                hash.Append(kvp.Key);
                hash.Append(kvp.Value);
            }
            return hash;
        }

        public IEnumerable<string> GetChangedParams(IResolvedParameters previous)
        {
            var changed = new List<string>();
            foreach (var kvp in _values)
            {
                if (!previous.TryGetValue(kvp.Key, out var prevValue) || !Mathf.Approximately(kvp.Value, prevValue))
                {
                    changed.Add(kvp.Key);
                }
            }
            return changed;
        }

        public IEnumerator<KeyValuePair<string, float>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}