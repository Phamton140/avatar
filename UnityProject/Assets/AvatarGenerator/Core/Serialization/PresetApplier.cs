using System.Collections.Generic;
using System.Linq;
using AvatarGenerator.Core.Parameters;

namespace AvatarGenerator.Core.Serialization
{
    public class PresetApplier
    {
        private readonly PresetLibrary _library;
        private readonly ParameterSchema _schema;

        public PresetApplier(PresetLibrary library, ParameterSchema schema)
        {
            _library = library;
            _schema = schema;
        }

        public void ApplyPreset(ParameterBag bag, string presetId, bool additive = false)
        {
            var preset = _library.Get(presetId);
            if (preset == null)
                return;

            ApplyPreset(bag, preset, additive);
        }

        public void ApplyPreset(ParameterBag bag, PresetDefinition preset, bool additive = false)
        {
            var effectiveAdditive = additive || preset.IsAdditive;

            foreach (var kvp in preset.Parameters)
            {
                var paramId = kvp.Key;
                var intent = kvp.Value;

                if (!_schema.Definitions.ContainsKey(paramId))
                    continue;

                if (effectiveAdditive && bag.GetIntent(paramId).State == ResolutionState.Overridden)
                {
                    continue;
                }

                if (intent.Value.HasValue)
                {
                    var source = effectiveAdditive ? ValueSource.Preset : ValueSource.Preset;
                    bag.SetValue(paramId, intent.Value.Value, source);
                }

                bag.SetIntent(paramId, intent);
            }
        }

        public void ApplyPresetStack(ParameterBag bag, string[] presetIds)
        {
            var sorted = presetIds
                .Select(id => _library.Get(id))
                .Where(p => p != null)
                .OrderBy(p => p.Priority)
                .ToArray();

            foreach (var preset in sorted)
            {
                ApplyPreset(bag, preset, preset.IsAdditive);
            }
        }

        public void RemovePreset(ParameterBag bag, string presetId)
        {
            var preset = _library.Get(presetId);
            if (preset == null)
                return;

            foreach (var kvp in preset.Parameters)
            {
                var paramId = kvp.Key;
                var currentIntent = bag.GetIntent(paramId);

                if (currentIntent.State == ResolutionState.Overridden && !kvp.Value.State.HasFlag(ResolutionState.Overridden))
                {
                    continue;
                }

                bag.SetIntent(paramId, ParameterIntent.Auto());
            }

            bag.ClearDirty();
        }

        public string[] GetActivePresets(ParameterBag bag)
        {
            var active = new List<string>();

            foreach (var preset in _library.Presets)
            {
                bool matches = true;
                foreach (var kvp in preset.Parameters)
                {
                    var intent = bag.GetIntent(kvp.Key);
                    if (intent.State != ResolutionState.Overridden && intent.State != ResolutionState.Derived)
                    {
                        matches = false;
                        break;
                    }
                    if (intent.Value.HasValue && kvp.Value.Value.HasValue)
                    {
                        if (!Mathf.Approximately(intent.Value.Value, kvp.Value.Value.Value))
                        {
                            matches = false;
                            break;
                        }
                    }
                }
                if (matches)
                    active.Add(preset.Id);
            }

            return active.ToArray();
        }
    }
}