using System.Collections.Generic;
using System.Linq;
using AvatarGenerator.Core.Clothing;
using AvatarGenerator.Core.Regions;
using AvatarGenerator.Core.Skeleton;
using UnityEngine;

namespace AvatarGenerator.Core.Clothing
{
    public class ClothingDeformer : IClothingDeformer
    {
        public Mesh DeformClothing(Mesh baseClothing, ClothingDeformContext context, ClothingAsset asset)
        {
            if (baseClothing == null || context.BoneWorldMatrices == null)
                return baseClothing;

            var vertices = baseClothing.vertices;
            var normals = baseClothing.normals;
            var deformedVertices = new Vector3[vertices.Length];
            var deformedNormals = new Vector3[normals.Length];

            var boneWeights = baseClothing.boneWeights;
            var bindPoses = baseClothing.bindposes;

            for (int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                var n = normals[i];
                var bw = boneWeights[i];

                Vector3 deformedPos = Vector3.zero;
                Vector3 deformedNor = Vector3.zero;
                float totalWeight = 0f;

                var weights = new[] { bw.weight0, bw.weight1, bw.weight2, bw.weight3 };
                var indices = new[] { bw.boneIndex0, bw.boneIndex1, bw.boneIndex2, bw.boneIndex3 };

                for (int w = 0; w < 4; w++)
                {
                    if (weights[w] <= 0f) continue;
                    int boneIdx = indices[w];
                    if (boneIdx < 0 || boneIdx >= context.BoneWorldMatrices.Length) continue;

                    var boneMatrix = context.BoneWorldMatrices[boneIdx];
                    var bindPose = bindPoses[boneIdx];

                    var localPos = bindPose.MultiplyPoint(v);
                    var worldPos = boneMatrix.MultiplyPoint(localPos);
                    var worldNor = boneMatrix.MultiplyVector(bindPose.MultiplyVector(n)).normalized;

                    deformedPos += worldPos * weights[w];
                    deformedNor += worldNor * weights[w];
                    totalWeight += weights[w];
                }

                if (totalWeight > 0f)
                {
                    deformedPos /= totalWeight;
                    deformedNor = deformedNor.normalized;
                }
                else
                {
                    deformedPos = v;
                    deformedNor = n;
                }

                deformedVertices[i] = deformedPos;
                deformedNormals[i] = deformedNor;
            }

            var clothCapsules = asset.Capsules;
            var bodyCapsules = context.BodyCapsules;

            for (int i = 0; i < deformedVertices.Length; i++)
            {
                var vert = deformedVertices[i];

                foreach (var capsule in clothCapsules)
                {
                    var bodyCapsule = FindMatchingBodyCapsule(capsule, bodyCapsules, context);
                    if (bodyCapsule == null) continue;

                    float dist = DistancePointCapsule(vert, bodyCapsule.Value, context.BoneWorldMatrices);
                    float combinedRadius = capsule.Radius + bodyCapsule.Value.Radius;

                    if (dist < combinedRadius)
                    {
                        var pushDir = (vert - GetCapsuleClosestPoint(vert, bodyCapsule.Value, context.BoneWorldMatrices)).normalized;
                        float pushStrength = 1f - dist / combinedRadius;
                        deformedVertices[i] += pushDir * pushStrength * combinedRadius * 0.5f;
                    }
                }
            }

            var result = new Mesh();
            result.name = baseClothing.name + "_Deformed";
            result.vertices = deformedVertices;
            result.normals = deformedNormals;
            result.triangles = baseClothing.triangles;
            result.uv = baseClothing.uv;
            result.uv2 = baseClothing.uv2;
            result.uv3 = baseClothing.uv3;
            result.uv4 = baseClothing.uv4;
            result.boneWeights = baseClothing.boneWeights;
            result.bindposes = baseClothing.bindposes;
            result.subMeshCount = baseClothing.subMeshCount;

            for (int s = 0; s < baseClothing.subMeshCount; s++)
            {
                result.SetTriangles(baseClothing.GetTriangles(s), s);
            }

            result.RecalculateBounds();
            result.RecalculateTangents();

            return result;
        }

        private ClothingCapsule? FindMatchingBodyCapsule(ClothingCapsule clothCapsule, ClothingCapsule[] bodyCapsules, ClothingDeformContext context)
        {
            foreach (var bodyCap in bodyCapsules)
            {
                if (bodyCap.BoneName == clothCapsule.BoneName)
                    return bodyCap;
            }

            foreach (var bone in context.Skeleton.Bones)
            {
                if (bone.Name.Contains(clothCapsule.BoneName) || clothCapsule.BoneName.Contains(bone.Name))
                {
                    foreach (var bodyCap in bodyCapsules)
                    {
                        if (bodyCap.BoneName == bone.Name)
                            return bodyCap;
                    }
                }
            }

            return null;
        }

        private float DistancePointCapsule(Vector3 point, ClothingCapsule capsule, Matrix4x4[] boneMatrices)
        {
            int boneIdx = System.Array.FindIndex(boneMatrices, m => false);

            for (int i = 0; i < boneMatrices.Length; i++)
            {
                // We'd need bone name mapping here - simplified for now
            }

            var capsuleStart = capsule.LocalCenter;
            var capsuleEnd = capsule.LocalCenter;

            if (capsule.Direction == CapsuleDirection.AlongBone)
            {
                capsuleEnd += new Vector3(0, capsule.Height, 0);
            }

            var dir = (capsuleEnd - capsuleStart).normalized;
            var len = Vector3.Distance(capsuleStart, capsuleEnd);

            var t = Vector3.Dot(point - capsuleStart, dir);
            t = Mathf.Clamp(t, 0, len);

            var closest = capsuleStart + dir * t;
            return Vector3.Distance(point, closest);
        }

        private Vector3 GetCapsuleClosestPoint(Vector3 point, ClothingCapsule capsule, Matrix4x4[] boneMatrices)
        {
            var capsuleStart = capsule.LocalCenter;
            var capsuleEnd = capsule.LocalCenter;

            if (capsule.Direction == CapsuleDirection.AlongBone)
            {
                capsuleEnd += new Vector3(0, capsule.Height, 0);
            }

            var dir = (capsuleEnd - capsuleStart).normalized;
            var len = Vector3.Distance(capsuleStart, capsuleEnd);

            var t = Vector3.Dot(point - capsuleStart, dir);
            t = Mathf.Clamp(t, 0, len);

            return capsuleStart + dir * t;
        }

        public ClothingCapsule[] GenerateBodyCapsules(SkeletonDefinition skeleton, RegionDeformResult[] regions)
        {
            var capsules = new List<ClothingCapsule>();
            var nameToIndex = new Dictionary<string, int>();

            for (int i = 0; i < skeleton.Bones.Length; i++)
                nameToIndex[skeleton.Bones[i].Name] = i;

            foreach (var region in regions)
            {
                if (region.RootTransform == Matrix4x4.identity) continue;

                int boneIdx = -1;
                for (int i = 0; i < skeleton.Bones.Length; i++)
                {
                    var boneWorld = skeleton.BindPoses[i];
                    if (Vector3.Distance(boneWorld.GetColumn(3), region.RootTransform.GetColumn(3)) < 0.02f)
                    {
                        boneIdx = i;
                        break;
                    }
                }

                if (boneIdx < 0) continue;

                var bone = skeleton.Bones[boneIdx];
                float radius = Mathf.Max(region.Scale.y, region.Scale.z) * 0.12f;
                float height = bone.LocalPosition.magnitude * region.Scale.x;

                if (height < 0.01f)
                {
                    capsules.Add(new ClothingCapsule
                    {
                        BoneName = bone.Name,
                        LocalCenter = Vector3.zero,
                        Radius = radius * 1.5f,
                        Height = radius * 2f,
                        Direction = CapsuleDirection.Perpendicular
                    });
                }
                else
                {
                    capsules.Add(new ClothingCapsule
                    {
                        BoneName = bone.Name,
                        LocalCenter = Vector3.up * (-height * 0.5f),
                        Radius = radius,
                        Height = height,
                        Direction = CapsuleDirection.AlongBone
                    });
                }
            }

            return capsules.ToArray();
        }
    }
}