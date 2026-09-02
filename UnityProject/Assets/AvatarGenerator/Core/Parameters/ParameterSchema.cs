using System.Collections.Generic;

namespace AvatarGenerator.Core.Parameters
{
    public class ParameterSchema
    {
        public string Version = "1.0.0";
        public readonly Dictionary<string, ParameterDefinition> Definitions = new Dictionary<string, ParameterDefinition>();

        public void Register(ParameterDefinition def)
        {
            Definitions[def.Id] = def;
        }

        public bool TryGet(string id, out ParameterDefinition def)
        {
            return Definitions.TryGetValue(id, out def);
        }

        public ParameterDefinition Get(string id)
        {
            return Definitions[id];
        }

        public IEnumerable<ParameterDefinition> GetByCategory(string category)
        {
            foreach (var def in Definitions.Values)
            {
                if (def.Category == category)
                    yield return def;
            }
        }

        public static ParameterSchema CreateDefault()
        {
            var schema = new ParameterSchema();

            // Body proportions
            schema.Register(new ParameterDefinition
            {
                Id = "body.height",
                Type = ParameterType.Float,
                DisplayName = "Height",
                Category = "Body/Proportions",
                DefaultValue = 1.75f,
                MinSuggested = 0.5f,
                MaxSuggested = 3.0f,
                Unit = "m",
                Flags = ParameterFlags.DrivesGeometry | ParameterFlags.DrivesSkeleton
            });

            schema.Register(new ParameterDefinition
            {
                Id = "body.headScale",
                Type = ParameterType.Float,
                DisplayName = "Head Scale",
                Category = "Body/Proportions",
                DefaultValue = 1.0f,
                MinSuggested = 0.5f,
                MaxSuggested = 2.5f,
                Unit = "ratio",
                Flags = ParameterFlags.DrivesGeometry | ParameterFlags.DrivesSkeleton,
                DerivationExpression = "1.0"
            });

            schema.Register(new ParameterDefinition
            {
                Id = "body.torsoScale",
                Type = ParameterType.Float,
                DisplayName = "Torso Scale",
                Category = "Body/Proportions",
                DefaultValue = 1.0f,
                MinSuggested = 0.5f,
                MaxSuggested = 2.0f,
                Unit = "ratio",
                Flags = ParameterFlags.DrivesGeometry | ParameterFlags.DrivesSkeleton,
                DerivationExpression = "1.0"
            });

            schema.Register(new ParameterDefinition
            {
                Id = "body.legLength",
                Type = ParameterType.Float,
                DisplayName = "Leg Length",
                Category = "Body/Proportions",
                DefaultValue = 1.0f,
                MinSuggested = 0.3f,
                MaxSuggested = 2.0f,
                Unit = "ratio",
                Flags = ParameterFlags.DrivesGeometry | ParameterFlags.DrivesSkeleton,
                DerivationExpression = "1.0"
            });

            schema.Register(new ParameterDefinition
            {
                Id = "body.armLength",
                Type = ParameterType.Float,
                DisplayName = "Arm Length",
                Category = "Body/Proportions",
                DefaultValue = 1.0f,
                MinSuggested = 0.3f,
                MaxSuggested = 2.5f,
                Unit = "ratio",
                Flags = ParameterFlags.DrivesGeometry | ParameterFlags.DrivesSkeleton,
                DerivationExpression = "1.0"
            });

            schema.Register(new ParameterDefinition
            {
                Id = "body.shoulderWidth",
                Type = ParameterType.Float,
                DisplayName = "Shoulder Width",
                Category = "Body/Proportions",
                DefaultValue = 1.0f,
                MinSuggested = 0.5f,
                MaxSuggested = 3.0f,
                Unit = "ratio",
                Flags = ParameterFlags.DrivesGeometry | ParameterFlags.DrivesSkeleton | ParameterFlags.AffectsClothing,
                DerivationExpression = "1.0"
            });

            schema.Register(new ParameterDefinition
            {
                Id = "body.chestWidth",
                Type = ParameterType.Float,
                DisplayName = "Chest Width",
                Category = "Body/Proportions",
                DefaultValue = 1.0f,
                MinSuggested = 0.5f,
                MaxSuggested = 2.0f,
                Unit = "ratio",
                Flags = ParameterFlags.DrivesGeometry | ParameterFlags.AffectsClothing,
                DerivationExpression = "1.0"
            });

            schema.Register(new ParameterDefinition
            {
                Id = "body.hipWidth",
                Type = ParameterType.Float,
                DisplayName = "Hip Width",
                Category = "Body/Proportions",
                DefaultValue = 1.0f,
                MinSuggested = 0.5f,
                MaxSuggested = 2.0f,
                Unit = "ratio",
                Flags = ParameterFlags.DrivesGeometry | ParameterFlags.AffectsClothing,
                DerivationExpression = "1.0"
            });

            schema.Register(new ParameterDefinition
            {
                Id = "body.muscleMass",
                Type = ParameterType.Float,
                DisplayName = "Muscle Mass",
                Category = "Body/Build",
                DefaultValue = 0.5f,
                MinSuggested = 0.0f,
                MaxSuggested = 2.0f,
                Unit = "ratio",
                Flags = ParameterFlags.DrivesMorphs | ParameterFlags.DrivesGeometry
            });

            schema.Register(new ParameterDefinition
            {
                Id = "body.bodyFat",
                Type = ParameterType.Float,
                DisplayName = "Body Fat",
                Category = "Body/Build",
                DefaultValue = 0.2f,
                MinSuggested = 0.0f,
                MaxSuggested = 1.0f,
                Unit = "ratio",
                Flags = ParameterFlags.DrivesMorphs | ParameterFlags.DrivesGeometry
            });

            // Face parameters
            schema.Register(new ParameterDefinition
            {
                Id = "face.faceWidth",
                Type = ParameterType.Float,
                DisplayName = "Face Width",
                Category = "Face/Structure",
                DefaultValue = 1.0f,
                MinSuggested = 0.7f,
                MaxSuggested = 1.5f,
                Unit = "ratio",
                Flags = ParameterFlags.DrivesMorphs
            });

            schema.Register(new ParameterDefinition
            {
                Id = "face.jawWidth",
                Type = ParameterType.Float,
                DisplayName = "Jaw Width",
                Category = "Face/Structure",
                DefaultValue = 1.0f,
                MinSuggested = 0.5f,
                MaxSuggested = 2.0f,
                Unit = "ratio",
                Flags = ParameterFlags.DrivesMorphs
            });

            schema.Register(new ParameterDefinition
            {
                Id = "face.eyeSize",
                Type = ParameterType.Float,
                DisplayName = "Eye Size",
                Category = "Face/Features",
                DefaultValue = 1.0f,
                MinSuggested = 0.5f,
                MaxSuggested = 2.0f,
                Unit = "ratio",
                Flags = ParameterFlags.DrivesMorphs
            });

            schema.Register(new ParameterDefinition
            {
                Id = "face.noseSize",
                Type = ParameterType.Float,
                DisplayName = "Nose Size",
                Category = "Face/Features",
                DefaultValue = 1.0f,
                MinSuggested = 0.5f,
                MaxSuggested = 2.0f,
                Unit = "ratio",
                Flags = ParameterFlags.DrivesMorphs
            });

            return schema;
        }
    }
}