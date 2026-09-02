using System.Collections.Generic;
using UnityEngine;

namespace AvatarGenerator.Core.Parameters
{
    [Serializable]
    public class CanonModel
    {
        public string Version = "1.0.0";
        public float BaseHeight = 1.75f;
        public Dictionary<string, float> Proportions = new Dictionary<string, float>
        {
            ["headHeight"] = 0.225f,
            ["neckHeight"] = 0.07f,
            ["torsoHeight"] = 0.50f,
            ["pelvisHeight"] = 0.12f,
            ["thighLength"] = 0.43f,
            ["calfLength"] = 0.39f,
            ["footLength"] = 0.15f,
            ["upperArmLength"] = 0.33f,
            ["forearmLength"] = 0.27f,
            ["handLength"] = 0.19f,
            ["shoulderWidth"] = 0.42f,
            ["hipWidth"] = 0.35f,
            ["chestWidth"] = 0.30f,
            ["waistWidth"] = 0.25f,
            ["handWidth"] = 0.085f,
            ["footWidth"] = 0.10f
        };

        public Dictionary<string, string> Relationships = new Dictionary<string, string>
        {
            ["legLength"] = "thighLength + calfLength",
            ["armLength"] = "upperArmLength + forearmLength",
            ["torsoHeight"] = "chestHeight + abdomenHeight + pelvisHeight",
            ["headWidth"] = "headHeight * 0.85f",
            ["eyeSpacing"] = "faceWidth * 0.45f"
        };

        public float GetProportion(string key)
        {
            return Proportions.TryGetValue(key, out var value) ? value : 0f;
        }

        public float GetAbsolute(string key, float height)
        {
            return GetProportion(key) * height;
        }

        public bool TryGetProportion(string key, out float value)
        {
            return Proportions.TryGetValue(key, out value);
        }
    }
}