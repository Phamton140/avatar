using System.Collections.Generic;
using AvatarGenerator.Core.Parameters;

namespace AvatarGenerator.Core.Serialization
{
    [Serializable]
    public class PresetDefinition
    {
        public string Id;
        public string DisplayName;
        public string Category;
        public int Priority;
        public string[] CompatibleModules;
        public Dictionary<string, ParameterIntent> Parameters = new Dictionary<string, ParameterIntent>();
        public string[] Dependencies;
        public bool IsAdditive;
    }

    [Serializable]
    public class PresetLibrary
    {
        public string Version = "1.0.0";
        public List<PresetDefinition> Presets = new List<PresetDefinition>();

        public PresetDefinition Get(string id)
        {
            return Presets.Find(p => p.Id == id);
        }

        public IEnumerable<PresetDefinition> GetByCategory(string category)
        {
            foreach (var p in Presets)
            {
                if (p.Category == category)
                    yield return p;
            }
        }

        public static PresetLibrary CreateDefault()
        {
            var lib = new PresetLibrary();

            lib.Presets.Add(new PresetDefinition
            {
                Id = "HUMAN_REALISTIC",
                DisplayName = "Human Realistic",
                Category = "Style",
                Priority = 10,
                CompatibleModules = new[] { "Body", "Face", "Head" },
                Parameters = new Dictionary<string, ParameterIntent>
                {
                    ["body.height"] = ParameterIntent.Direct(1.75f),
                    ["body.headScale"] = ParameterIntent.Direct(1.0f),
                    ["body.torsoScale"] = ParameterIntent.Direct(1.0f),
                    ["body.legLength"] = ParameterIntent.Direct(1.0f),
                    ["body.armLength"] = ParameterIntent.Direct(1.0f),
                    ["body.shoulderWidth"] = ParameterIntent.Direct(1.0f),
                    ["body.chestWidth"] = ParameterIntent.Direct(1.0f),
                    ["body.hipWidth"] = ParameterIntent.Direct(1.0f),
                    ["body.muscleMass"] = ParameterIntent.Direct(0.5f),
                    ["body.bodyFat"] = ParameterIntent.Direct(0.2f),
                    ["face.faceWidth"] = ParameterIntent.Direct(1.0f),
                    ["face.jawWidth"] = ParameterIntent.Direct(1.0f),
                    ["face.eyeSize"] = ParameterIntent.Direct(1.0f),
                    ["face.noseSize"] = ParameterIntent.Direct(1.0f)
                }
            });

            lib.Presets.Add(new PresetDefinition
            {
                Id = "ANIME",
                DisplayName = "Anime Style",
                Category = "Style",
                Priority = 20,
                CompatibleModules = new[] { "Body", "Face", "Head" },
                IsAdditive = true,
                Parameters = new Dictionary<string, ParameterIntent>
                {
                    ["body.headScale"] = ParameterIntent.Direct(1.25f),
                    ["body.legLength"] = ParameterIntent.Direct(1.15f),
                    ["body.armLength"] = ParameterIntent.Direct(1.1f),
                    ["face.eyeSize"] = ParameterIntent.Direct(1.4f),
                    ["face.faceWidth"] = ParameterIntent.Direct(0.9f),
                    ["face.jawWidth"] = ParameterIntent.Direct(0.8f),
                    ["face.noseSize"] = ParameterIntent.Direct(0.7f)
                }
            });

            lib.Presets.Add(new PresetDefinition
            {
                Id = "CHIBI",
                DisplayName = "Chibi",
                Category = "Style",
                Priority = 30,
                CompatibleModules = new[] { "Body", "Face", "Head" },
                Parameters = new Dictionary<string, ParameterIntent>
                {
                    ["body.height"] = ParameterIntent.Direct(1.2f),
                    ["body.headScale"] = ParameterIntent.Direct(1.8f),
                    ["body.torsoScale"] = ParameterIntent.Direct(0.7f),
                    ["body.legLength"] = ParameterIntent.Direct(0.5f),
                    ["body.armLength"] = ParameterIntent.Direct(0.7f),
                    ["body.shoulderWidth"] = ParameterIntent.Direct(0.8f),
                    ["face.eyeSize"] = ParameterIntent.Direct(1.8f),
                    ["face.faceWidth"] = ParameterIntent.Direct(1.2f)
                }
            });

            lib.Presets.Add(new PresetDefinition
            {
                Id = "HEROIC",
                DisplayName = "Heroic Proportions",
                Category = "Style",
                Priority = 15,
                CompatibleModules = new[] { "Body" },
                IsAdditive = true,
                Parameters = new Dictionary<string, ParameterIntent>
                {
                    ["body.height"] = ParameterIntent.Direct(1.9f),
                    ["body.shoulderWidth"] = ParameterIntent.Direct(1.2f),
                    ["body.chestWidth"] = ParameterIntent.Direct(1.15f),
                    ["body.legLength"] = ParameterIntent.Direct(1.1f),
                    ["body.muscleMass"] = ParameterIntent.Direct(0.8f),
                    ["body.bodyFat"] = ParameterIntent.Direct(0.1f)
                }
            });

            lib.Presets.Add(new PresetDefinition
            {
                Id = "ATHLETIC_BUILD",
                DisplayName = "Athletic Build",
                Category = "BodyType",
                Priority = 50,
                CompatibleModules = new[] { "Body" },
                IsAdditive = true,
                Parameters = new Dictionary<string, ParameterIntent>
                {
                    ["body.muscleMass"] = ParameterIntent.Direct(0.75f),
                    ["body.bodyFat"] = ParameterIntent.Direct(0.1f),
                    ["body.shoulderWidth"] = ParameterIntent.Direct(1.1f),
                    ["body.chestWidth"] = ParameterIntent.Direct(1.05f)
                }
            });

            lib.Presets.Add(new PresetDefinition
            {
                Id = "HEAVY_BUILD",
                DisplayName = "Heavy Build",
                Category = "BodyType",
                Priority = 50,
                CompatibleModules = new[] { "Body" },
                IsAdditive = true,
                Parameters = new Dictionary<string, ParameterIntent>
                {
                    ["body.muscleMass"] = ParameterIntent.Direct(0.3f),
                    ["body.bodyFat"] = ParameterIntent.Direct(0.6f),
                    ["body.chestWidth"] = ParameterIntent.Direct(1.2f),
                    ["body.hipWidth"] = ParameterIntent.Direct(1.15f),
                    ["body.shoulderWidth"] = ParameterIntent.Direct(1.05f)
                }
            });

            lib.Presets.Add(new PresetDefinition
            {
                Id = "THIN_BUILD",
                DisplayName = "Thin Build",
                Category = "BodyType",
                Priority = 50,
                CompatibleModules = new[] { "Body" },
                IsAdditive = true,
                Parameters = new Dictionary<string, ParameterIntent>
                {
                    ["body.muscleMass"] = ParameterIntent.Direct(0.2f),
                    ["body.bodyFat"] = ParameterIntent.Direct(0.05f),
                    ["body.shoulderWidth"] = ParameterIntent.Direct(0.9f),
                    ["body.chestWidth"] = ParameterIntent.Direct(0.9f),
                    ["body.hipWidth"] = ParameterIntent.Direct(0.9f)
                }
            });

            lib.Presets.Add(new PresetDefinition
            {
                Id = "ELDERLY",
                DisplayName = "Elderly",
                Category = "Age",
                Priority = 40,
                CompatibleModules = new[] { "Body", "Face" },
                IsAdditive = true,
                Parameters = new Dictionary<string, ParameterIntent>
                {
                    ["body.height"] = ParameterIntent.Direct(1.65f),
                    ["body.headScale"] = ParameterIntent.Direct(1.05f),
                    ["body.muscleMass"] = ParameterIntent.Direct(0.2f),
                    ["body.bodyFat"] = ParameterIntent.Direct(0.25f),
                    ["face.eyeSize"] = ParameterIntent.Direct(0.9f),
                    ["face.jawWidth"] = ParameterIntent.Direct(1.1f)
                }
            });

            lib.Presets.Add(new PresetDefinition
            {
                Id = "CHILD",
                DisplayName = "Child",
                Category = "Age",
                Priority = 40,
                CompatibleModules = new[] { "Body", "Face", "Head" },
                Parameters = new Dictionary<string, ParameterIntent>
                {
                    ["body.height"] = ParameterIntent.Direct(1.2f),
                    ["body.headScale"] = ParameterIntent.Direct(1.3f),
                    ["body.torsoScale"] = ParameterIntent.Direct(0.9f),
                    ["body.legLength"] = ParameterIntent.Direct(0.8f),
                    ["body.armLength"] = ParameterIntent.Direct(0.85f),
                    ["body.muscleMass"] = ParameterIntent.Direct(0.15f),
                    ["body.bodyFat"] = ParameterIntent.Direct(0.15f),
                    ["face.eyeSize"] = ParameterIntent.Direct(1.2f),
                    ["face.faceWidth"] = ParameterIntent.Direct(1.1f),
                    ["face.jawWidth"] = ParameterIntent.Direct(0.7f)
                }
            });

            return lib;
        }
    }
}