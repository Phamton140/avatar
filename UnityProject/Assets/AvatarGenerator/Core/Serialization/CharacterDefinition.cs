using System.Collections.Generic;
using AvatarGenerator.Core.Parameters;

namespace AvatarGenerator.Core.Serialization
{
    [Serializable]
    public class CharacterDefinition
    {
        public string SchemaVersion = "1.0.0";
        public int FormatVersion = 1;
        public CharacterMetadata Metadata = new CharacterMetadata();
        public Dictionary<string, ParameterIntent> Parameters = new Dictionary<string, ParameterIntent>();
        public Dictionary<string, ModuleData> Modules = new Dictionary<string, ModuleData>();
        public Dictionary<string, float> Overrides = new Dictionary<string, float>();
        public RiggingConfig Rigging = new RiggingConfig();

        public ParameterBag ToParameterBag(ParameterSchema schema = null)
        {
            schema ??= ParameterSchema.CreateDefault();
            var bag = new ParameterBag(schema);

            foreach (var kvp in Parameters)
            {
                bag.SetIntent(kvp.Key, kvp.Value);
                if (kvp.Value.Value.HasValue)
                {
                    bag.SetValue(kvp.Key, kvp.Value.Value.Value, ValueSource.UserOverride);
                }
            }

            foreach (var kvp in Overrides)
            {
                bag.SetValue(kvp.Key, kvp.Value, ValueSource.UserOverride);
            }

            return bag;
        }

        public static CharacterDefinition FromParameterBag(ParameterBag bag)
        {
            var def = new CharacterDefinition();
            def.Metadata.GeneratorVersion = "0.1.0";

            foreach (var kvp in bag.Intents)
            {
                def.Parameters[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in bag.Values)
            {
                if (kvp.Value.Source == ValueSource.UserOverride)
                {
                    def.Overrides[kvp.Key] = kvp.Value.AsFloat();
                }
            }

            return def;
        }
    }

    [Serializable]
    public class CharacterMetadata
    {
        public string Id;
        public string Name;
        public int Seed;
        public string GeneratorVersion;
        public string Created;
        public string Modified;
        public string[] PresetsApplied;
    }

    [Serializable]
    public class ModuleData
    {
        public string ModuleId;
        public Dictionary<string, object> Data = new Dictionary<string, object>();
    }

    [Serializable]
    public class RiggingConfig
    {
        public string Template = "humanoid_v1";
        public Dictionary<string, float> CustomBoneScales = new Dictionary<string, float>();
    }

    [Serializable]
    public struct MigrationResult
    {
        public bool Success;
        public string Message;
        public CharacterDefinition MigratedDefinition;
    }
}