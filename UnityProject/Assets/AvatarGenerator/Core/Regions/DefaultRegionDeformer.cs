using System.Collections.Generic;
using System.Linq;
using AvatarGenerator.Core.Parameters;
using AvatarGenerator.Core.Skeleton;
using UnityEngine;

namespace AvatarGenerator.Core.Regions
{
    public class DefaultRegionDeformer : IRegionDeformer
    {
        private readonly CanonModel _canon;

        public DefaultRegionDeformer(CanonModel canon)
        {
            _canon = canon;
        }

        public RegionDeformResult Deform(RegionDefinition region, SkeletonDefinition skeleton, IResolvedParameters resolved)
        {
            var result = new RegionDeformResult
            {
                Scale = Vector3.one,
                MorphWeights = new float[0],
                RootTransform = Matrix4x4.identity
            };

            float height = resolved.GetFloat("body.height");
            float globalScale = height / _canon.BaseHeight;

            var acc = new RegionScaleAccumulator();

            acc.ApplyGlobalScale(globalScale);

            foreach (var param in region.PrimaryParams)
            {
                if (!resolved.TryGetValue(param, out var value)) continue;

                if (param.Contains("Length") || param.Contains("Height"))
                    acc.ApplyLengthScale(value, 0.9f);
                else if (param.Contains("Width") || param.Contains("Girth") || param.Contains("Scale"))
                    acc.ApplyRadiusScale(value, 0.9f);
            }

            foreach (var param in region.SecondaryParams)
            {
                if (!resolved.TryGetValue(param, out var value)) continue;

                if (param.Contains("Length") || param.Contains("Height"))
                    acc.ApplyLengthScale(value, 0.5f);
                else if (param.Contains("Width") || param.Contains("Girth") || param.Contains("Scale"))
                    acc.ApplyRadiusScale(value, 0.5f);
            }

            if (region.Deformers != null && System.Array.Exists(region.Deformers, d => d == DeformerType.ScaleVolume))
            {
                float vol = acc.Length * acc.RadiusX * acc.RadiusY;
                float comp = Mathf.Pow(1f / Mathf.Max(0.001f, vol), 1f / 3f);
                acc.Length *= comp;
                acc.RadiusX *= comp;
                acc.RadiusY *= comp;
            }

            result.Scale = new Vector3(acc.Length, acc.RadiusX, acc.RadiusY);

            var primaryBone = GetPrimaryBone(region, skeleton);
            if (primaryBone >= 0)
            {
                var worldMatrices = ComputeWorldMatrices(skeleton);
                result.RootTransform = worldMatrices[primaryBone];
            }

            return result;
        }

        private int GetPrimaryBone(RegionDefinition region, SkeletonDefinition skeleton)
        {
            if (region.Bones == null || region.Bones.Length == 0) return -1;
            return skeleton.GetBoneIndex(region.Bones[0]);
        }

        private Matrix4x4[] ComputeWorldMatrices(SkeletonDefinition skeleton)
        {
            var matrices = new Matrix4x4[skeleton.Bones.Length];
            var nameToIndex = new Dictionary<string, int>();
            for (int i = 0; i < skeleton.Bones.Length; i++)
                nameToIndex[skeleton.Bones[i].Name] = i;

            for (int i = 0; i < skeleton.Bones.Length; i++)
            {
                var bone = skeleton.Bones[i];
                var local = Matrix4x4.TRS(bone.LocalPosition, bone.LocalRotation, bone.LocalScale);

                if (string.IsNullOrEmpty(bone.ParentName))
                {
                    matrices[i] = local;
                }
                else
                {
                    matrices[i] = matrices[nameToIndex[bone.ParentName]] * local;
                }
            }
            return matrices;
        }

        private struct RegionScaleAccumulator
        {
            public float Length = 1f;
            public float RadiusX = 1f;
            public float RadiusY = 1f;

            public void ApplyGlobalScale(float s)
            {
                Length *= s;
                RadiusX *= s;
                RadiusY *= s;
            }

            public void ApplyLengthScale(float scale, float priority)
            {
                Length = Mathf.Lerp(Length, Length * scale, priority);
            }

            public void ApplyRadiusScale(float scale, float priority)
            {
                RadiusX = Mathf.Lerp(RadiusX, RadiusX * scale, priority);
                RadiusY = Mathf.Lerp(RadiusY, RadiusY * scale, priority);
            }
        }
    }
}