using System.Collections.Generic;
using AvatarGenerator.Core.Landmarks;
using UnityEngine;

namespace AvatarGenerator.Core.Skeleton
{
    public static class IKSolver
    {
        public static void SolveAll(SkeletonDefinition skeleton, LandmarkTarget[] targets)
        {
            var worldMatrices = ComputeWorldMatrices(skeleton);
            var targetMap = new Dictionary<LandmarkId, LandmarkTarget>();

            foreach (var t in targets)
            {
                if (!targetMap.ContainsKey(t.Landmark) || t.Weight > targetMap[t.Landmark].Weight)
                {
                    targetMap[t.Landmark] = t;
                }
            }

            foreach (var bone in skeleton.Bones)
            {
                if (bone.HasIK)
                {
                    if (targetMap.TryGetValue(bone.IKData.TargetLandmark, out var target))
                    {
                        SolveChain(skeleton, worldMatrices, bone, target, targetMap);
                    }
                }
            }
        }

        private static void SolveChain(
            SkeletonDefinition skeleton,
            Matrix4x4[] worldMatrices,
            BoneDefinition rootBone,
            LandmarkTarget target,
            Dictionary<LandmarkId, LandmarkTarget> allTargets)
        {
            var chain = GetChain(skeleton, rootBone, rootBone.IKData.ChainLength);
            if (chain.Length < 2) return;

            var effectorIdx = chain[chain.Length - 1];
            var rootIdx = chain[0];

            var effectorPos = worldMatrices[effectorIdx].GetColumn(3);
            var targetPos = target.TargetPosition;
            var rootPos = worldMatrices[rootIdx].GetColumn(3);

            float totalLength = 0f;
            for (int i = 1; i < chain.Length; i++)
            {
                var parent = worldMatrices[chain[i - 1]].GetColumn(3);
                var child = worldMatrices[chain[i]].GetColumn(3);
                totalLength += Vector3.Distance(parent, child);
            }

            float targetDist = Vector3.Distance(rootPos, targetPos);

            if (targetDist > totalLength * 0.999f)
            {
                AlignChainToTarget(skeleton, worldMatrices, chain, targetPos);
            }
            else
            {
                var polePos = Vector3.zero;
                bool hasPole = false;

                if (rootBone.IKData.PoleLandmark != default && allTargets.TryGetValue(rootBone.IKData.PoleLandmark, out var poleTarget))
                {
                    polePos = poleTarget.TargetPosition;
                    hasPole = true;
                }

                FabrikSolve(skeleton, worldMatrices, chain, targetPos, hasPole ? polePos : rootPos + Vector3.up, rootBone.IKData.Weight);
            }
        }

        private static int[] GetChain(SkeletonDefinition skeleton, BoneDefinition rootBone, int length)
        {
            var chain = new List<int> { skeleton.GetBoneIndex(rootBone.Name) };
            var nameToIndex = new Dictionary<string, int>();
            for (int i = 0; i < skeleton.Bones.Length; i++)
                nameToIndex[skeleton.Bones[i].Name] = i;

            var current = rootBone;
            for (int i = 1; i < length; i++)
            {
                var children = new List<BoneDefinition>();
                foreach (var b in skeleton.Bones)
                {
                    if (b.ParentName == current.Name)
                        children.Add(b);
                }

                if (children.Count == 0) break;

                var next = children[0];
                chain.Add(nameToIndex[next.Name]);
                current = next;
            }

            return chain.ToArray();
        }

        private static void AlignChainToTarget(
            SkeletonDefinition skeleton,
            Matrix4x4[] worldMatrices,
            int[] chain,
            Vector3 targetPos)
        {
            var rootPos = worldMatrices[chain[0]].GetColumn(3);
            var dir = (targetPos - rootPos).normalized;

            for (int i = 1; i < chain.Length; i++)
            {
                var parentIdx = chain[i - 1];
                var childIdx = chain[i];

                var parentWorld = worldMatrices[parentIdx];
                var parentPos = parentWorld.GetColumn(3);

                var bone = skeleton.Bones[childIdx];
                var boneLen = bone.LocalPosition.magnitude;

                var childPos = parentPos + dir * boneLen;
                var childRot = Quaternion.LookRotation(dir, Vector3.up);
                var childScale = bone.LocalScale;

                var localPos = parentWorld.inverse.MultiplyPoint(childPos);
                var localRot = Quaternion.Inverse(parentWorld.rotation) * childRot;

                skeleton.Bones[childIdx] = skeleton.Bones[childIdx].WithLocal(localPos, localRot, childScale);
            }
        }

        private static void FabrikSolve(
            SkeletonDefinition skeleton,
            Matrix4x4[] worldMatrices,
            int[] chain,
            Vector3 targetPos,
            Vector3 polePos,
            float weight)
        {
            var positions = new Vector3[chain.Length];
            for (int i = 0; i < chain.Length; i++)
            {
                positions[i] = worldMatrices[chain[i]].GetColumn(3);
            }

            var lengths = new float[chain.Length - 1];
            for (int i = 0; i < lengths.Length; i++)
            {
                lengths[i] = Vector3.Distance(positions[i], positions[i + 1]);
            }

            var rootPos = positions[0];

            for (int iter = 0; iter < 10; iter++)
            {
                positions[chain.Length - 1] = targetPos;

                for (int i = chain.Length - 2; i >= 0; i--)
                {
                    var dir = (positions[i] - positions[i + 1]).normalized;
                    positions[i] = positions[i + 1] + dir * lengths[i];
                }

                positions[0] = rootPos;

                for (int i = 1; i < chain.Length; i++)
                {
                    var dir = (positions[i] - positions[i - 1]).normalized;
                    positions[i] = positions[i - 1] + dir * lengths[i];
                }

                if (Vector3.Distance(positions[chain.Length - 1], targetPos) < 0.001f)
                    break;
            }

            for (int i = 1; i < chain.Length; i++)
            {
                var parentIdx = chain[i - 1];
                var childIdx = chain[i];

                var parentWorld = worldMatrices[parentIdx];
                var childPos = positions[i];

                var dir = (childPos - positions[i - 1]).normalized;
                var childRot = Quaternion.LookRotation(dir, Vector3.up);

                var localPos = parentWorld.inverse.MultiplyPoint(childPos);
                var localRot = Quaternion.Inverse(parentWorld.rotation) * childRot;

                var bone = skeleton.Bones[childIdx];
                skeleton.Bones[childIdx] = bone.WithLocal(localPos, localRot, bone.LocalScale);
            }
        }

        private static Matrix4x4[] ComputeWorldMatrices(SkeletonDefinition skeleton)
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
    }

    public static class BoneDefinitionExtensions2
    {
        public static BoneDefinition WithLocal(this BoneDefinition bone, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            bone.LocalPosition = pos;
            bone.LocalRotation = rot;
            bone.LocalScale = scale;
            return bone;
        }
    }
}