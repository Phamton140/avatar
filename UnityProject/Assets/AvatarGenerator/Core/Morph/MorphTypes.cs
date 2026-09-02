using System.Collections.Generic;
using UnityEngine;

namespace AvatarGenerator.Core.Morph
{
    public enum MorphCategory
    {
        Identity,
        BodyType,
        Proportional,
        Facial,
        Corrective
    }

    [System.Serializable]
    public struct MorphDefinition
    {
        public string Id;
        public string DisplayName;
        public MorphCategory Category;
        public string[] DrivenParameters;
        public float MinWeight;
        public float MaxWeight;
        public int[] AffectedVertexIndices;
        public Vector3[] VertexDeltas;
        public float[] VertexWeights;
    }

    [System.Serializable]
    public struct MorphTargetAsset
    {
        public string Id;
        public string DisplayName;
        public MorphCategory Category;
        public Dictionary<string, float> ParameterWeights = new Dictionary<string, float>();
        public MorphDefinition[] Morphs;
    }

    [System.Serializable]
    public struct CorrectiveShape
    {
        public string Name;
        public string[] DrivingBones;
        public float[] TriggerAngles;
        public int[] AffectedVertices;
        public Vector3[] VertexDeltas;
    }

    public interface IMorphBlender
    {
        float[] ComputeWeights(IResolvedParameters resolved, CanonModel canon);
        MorphDeltas Blend(float[] weights, MorphDefinition[] morphs);
    }

    public class MorphBlender : IMorphBlender
    {
        private readonly MorphDefinition[] _morphs;

        public MorphBlender(MorphDefinition[] morphs)
        {
            _morphs = morphs;
        }

        public float[] ComputeWeights(IResolvedParameters resolved, CanonModel canon)
        {
            var weights = new float[_morphs.Length];

            for (int i = 0; i < _morphs.Length; i++)
            {
                var morph = _morphs[i];
                float weight = 0f;

                switch (morph.Category)
                {
                    case MorphCategory.Identity:
                        weight = ComputeIdentityWeight(morph, resolved);
                        break;
                    case MorphCategory.BodyType:
                        weight = ComputeBodyTypeWeight(morph, resolved);
                        break;
                    case MorphCategory.Proportional:
                        weight = ComputeProportionalWeight(morph, resolved, canon);
                        break;
                    case MorphCategory.Facial:
                        weight = ComputeFacialWeight(morph, resolved);
                        break;
                    case MorphCategory.Corrective:
                        weight = ComputeCorrectiveWeight(morph, resolved);
                        break;
                }

                weights[i] = Mathf.Clamp(weight, morph.MinWeight, morph.MaxWeight);
            }

            return weights;
        }

        private float ComputeIdentityWeight(MorphDefinition morph, IResolvedParameters resolved)
        {
            if (morph.DrivenParameters.Length == 0) return 0f;
            return resolved.TryGetValue(morph.DrivenParameters[0], out var v) ? v : 0f;
        }

        private float ComputeBodyTypeWeight(MorphDefinition morph, IResolvedParameters resolved)
        {
            float muscle = resolved.TryGetValue("body.muscleMass", out var m) ? m : 0.5f;
            float fat = resolved.TryGetValue("body.bodyFat", out var f) ? f : 0.2f;

            if (morph.Id.Contains("Muscular")) return muscle;
            if (morph.Id.Contains("Heavy") || morph.Id.Contains("Fat")) return fat;
            if (morph.Id.Contains("Thin")) return 1f - Mathf.Max(muscle, fat);

            return 0f;
        }

        private float ComputeProportionalWeight(MorphDefinition morph, IResolvedParameters resolved, CanonModel canon)
        {
            if (morph.DrivenParameters.Length == 0) return 0f;

            var param = morph.DrivenParameters[0];
            if (!resolved.TryGetValue(param, out var value)) return 0f;

            float canonValue = 1f;
            if (param.Contains("Scale") || param.Contains("Length"))
            {
                canonValue = 1f;
            }

            return (value - canonValue) / (morph.MaxWeight - morph.MinWeight);
        }

        private float ComputeFacialWeight(MorphDefinition morph, IResolvedParameters resolved)
        {
            if (morph.DrivenParameters.Length == 0) return 0f;
            return resolved.TryGetValue(morph.DrivenParameters[0], out var v) ? v - 1f : 0f;
        }

        private float ComputeCorrectiveWeight(MorphDefinition morph, IResolvedParameters resolved)
        {
            return 0f;
        }

        public MorphDeltas Blend(float[] weights, MorphDefinition[] morphs)
        {
            var vertexMap = new Dictionary<int, Vector3>();

            for (int i = 0; i < morphs.Length; i++)
            {
                var morph = morphs[i];
                float weight = weights[i];

                if (Mathf.Abs(weight) < 0.001f) continue;

                if (morph.AffectedVertexIndices == null || morph.VertexDeltas == null) continue;

                for (int j = 0; j < morph.AffectedVertexIndices.Length; j++)
                {
                    int vertIdx = morph.AffectedVertexIndices[j];
                    var delta = morph.VertexDeltas[j] * weight;

                    if (vertexMap.ContainsKey(vertIdx))
                    {
                        vertexMap[vertIdx] += delta;
                    }
                    else
                    {
                        vertexMap[vertIdx] = delta;
                    }
                }
            }

            var indices = new int[vertexMap.Count];
            var deltas = new Vector3[vertexMap.Count];
            int idx = 0;
            foreach (var kvp in vertexMap)
            {
                indices[idx] = kvp.Key;
                deltas[idx] = kvp.Value;
                idx++;
            }

            return new MorphDeltas
            {
                VertexIndices = indices,
                Deltas = deltas
            };
        }
    }
}