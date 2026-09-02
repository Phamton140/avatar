using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using AvatarGenerator.Core.Parameters;

namespace AvatarGenerator.Core.Serialization
{
    public class CharacterSerializer : ICharacterSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public CharacterSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new ParameterIntentConverter(),
                    new HashSetConverter()
                }
            };
        }

        public string Serialize(CharacterDefinition def, SerializationOptions options = null)
        {
            options ??= new SerializationOptions();

            if (options.IncludeMetadata)
            {
                def.Metadata.Modified = DateTime.UtcNow.ToString("O");
                if (string.IsNullOrEmpty(def.Metadata.Created))
                    def.Metadata.Created = def.Metadata.Modified;
            }

            return JsonConvert.SerializeObject(def, _settings);
        }

        public CharacterDefinition Deserialize(string json)
        {
            var def = JsonConvert.DeserializeObject<CharacterDefinition>(json, _settings);
            return MigrateToCurrent(def);
        }

        public MigrationResult Migrate(CharacterDefinition def, int targetVersion)
        {
            var current = def;
            int fromVersion = def.FormatVersion;

            for (int v = fromVersion; v < targetVersion; v++)
            {
                current = MigrateVersion(current, v, v + 1);
            }

            return new MigrationResult
            {
                Success = true,
                Message = $"Migrated from v{fromVersion} to v{targetVersion}",
                MigratedDefinition = current
            };
        }

        private CharacterDefinition MigrateToCurrent(CharacterDefinition def)
        {
            if (def.FormatVersion < 1)
            {
                return MigrateVersion(def, 0, 1);
            }
            return def;
        }

        private CharacterDefinition MigrateVersion(CharacterDefinition def, int from, int to)
        {
            var migrated = def;
            migrated.FormatVersion = to;

            if (from == 0 && to == 1)
            {
                if (migrated.Parameters.ContainsKey("bodyType"))
                {
                    var bodyType = migrated.Parameters["bodyType"];
                    migrated.Parameters["body.build"] = new ParameterIntent
                    {
                        Space = ParameterSpace.Parametric,
                        Value = MapBodyType(bodyType.AsString()),
                        State = ResolutionState.Overridden
                    };
                    migrated.Parameters.Remove("bodyType");
                }
            }

            return migrated;
        }

        private float MapBodyType(string bodyType)
        {
            switch (bodyType?.ToLower())
            {
                case "thin": return 0.2f;
                case "average": return 0.5f;
                case "athletic": return 0.7f;
                case "heavy": return 0.9f;
                default: return 0.5f;
            }
        }

        [Serializable]
        public struct SerializationOptions
        {
            public bool IncludeMetadata;
            public bool PrettyPrint;
        }
    }

    public interface ICharacterSerializer
    {
        string Serialize(CharacterDefinition def, SerializationOptions options = null);
        CharacterDefinition Deserialize(string json);
        MigrationResult Migrate(CharacterDefinition def, int targetVersion);
    }
}