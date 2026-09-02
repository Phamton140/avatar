using System.Collections.Generic;
using AvatarGenerator.Core.Parameters;
using UnityEngine;

namespace AvatarGenerator.Core.Skeleton
{
    public static class SkeletonBuilderFK
    {
        public static SkeletonDefinition BuildFromCanon(CanonModel canon, IResolvedParameters resolved)
        {
            float height = resolved.GetFloat("body.height");
            float headScale = resolved.GetFloat("body.headScale");
            float torsoScale = resolved.GetFloat("body.torsoScale");
            float legScale = resolved.GetFloat("body.legLength");
            float armScale = resolved.GetFloat("body.armLength");
            float shoulderWidth = canon.GetAbsolute("shoulderWidth", height) * resolved.GetFloat("body.shoulderWidth");
            float hipWidth = canon.GetAbsolute("hipWidth", height) * resolved.GetFloat("body.hipWidth");

            float headHeight = canon.GetAbsolute("headHeight", height) * headScale;
            float neckHeight = canon.GetAbsolute("neckHeight", height);
            float torsoHeight = canon.GetAbsolute("torsoHeight", height) * torsoScale;
            float pelvisHeight = canon.GetAbsolute("pelvisHeight", height);

            float thighLen = canon.GetAbsolute("thighLength", height) * legScale;
            float calfLen = canon.GetAbsolute("calfLength", height) * legScale;
            float footLen = canon.GetAbsolute("footLength", height) * legScale;

            float upperArmLen = canon.GetAbsolute("upperArmLength", height) * armScale;
            float foreArmLen = canon.GetAbsolute("forearmLength", height) * armScale;
            float handLen = canon.GetAbsolute("handLength", height) * armScale;

            var bones = new List<BoneDefinition>();

            // Root / Hips
            bones.Add(new BoneDefinition
            {
                Name = "Hips",
                ParentName = "",
                LocalPosition = new Vector3(0, height - headHeight - neckHeight - torsoHeight, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Spine
            bones.Add(new BoneDefinition
            {
                Name = "Spine1",
                ParentName = "Hips",
                LocalPosition = new Vector3(0, pelvisHeight + torsoHeight * 0.25f, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            bones.Add(new BoneDefinition
            {
                Name = "Spine2",
                ParentName = "Spine1",
                LocalPosition = new Vector3(0, torsoHeight * 0.25f, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            bones.Add(new BoneDefinition
            {
                Name = "Spine3",
                ParentName = "Spine2",
                LocalPosition = new Vector3(0, torsoHeight * 0.25f, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Neck
            bones.Add(new BoneDefinition
            {
                Name = "Neck",
                ParentName = "Spine3",
                LocalPosition = new Vector3(0, torsoHeight * 0.25f + neckHeight, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Head
            bones.Add(new BoneDefinition
            {
                Name = "Head",
                ParentName = "Neck",
                LocalPosition = new Vector3(0, headHeight * 0.5f, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Left Clavicle
            bones.Add(new BoneDefinition
            {
                Name = "LeftClavicle",
                ParentName = "Spine3",
                LocalPosition = new Vector3(-shoulderWidth * 0.5f, torsoHeight * 0.1f, 0),
                LocalRotation = Quaternion.Euler(0, 0, -15f),
                LocalScale = Vector3.one
            });

            // Left UpperArm
            bones.Add(new BoneDefinition
            {
                Name = "LeftUpperArm",
                ParentName = "LeftClavicle",
                LocalPosition = new Vector3(-upperArmLen, 0, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Left Forearm
            bones.Add(new BoneDefinition
            {
                Name = "LeftForearm",
                ParentName = "LeftUpperArm",
                LocalPosition = new Vector3(-foreArmLen, 0, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Left Hand
            bones.Add(new BoneDefinition
            {
                Name = "LeftHand",
                ParentName = "LeftForearm",
                LocalPosition = new Vector3(-handLen, 0, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Right Clavicle
            bones.Add(new BoneDefinition
            {
                Name = "RightClavicle",
                ParentName = "Spine3",
                LocalPosition = new Vector3(shoulderWidth * 0.5f, torsoHeight * 0.1f, 0),
                LocalRotation = Quaternion.Euler(0, 0, 15f),
                LocalScale = Vector3.one
            });

            // Right UpperArm
            bones.Add(new BoneDefinition
            {
                Name = "RightUpperArm",
                ParentName = "RightClavicle",
                LocalPosition = new Vector3(upperArmLen, 0, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Right Forearm
            bones.Add(new BoneDefinition
            {
                Name = "RightForearm",
                ParentName = "RightUpperArm",
                LocalPosition = new Vector3(foreArmLen, 0, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Right Hand
            bones.Add(new BoneDefinition
            {
                Name = "RightHand",
                ParentName = "RightForearm",
                LocalPosition = new Vector3(handLen, 0, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Left Thigh
            bones.Add(new BoneDefinition
            {
                Name = "LeftThigh",
                ParentName = "Hips",
                LocalPosition = new Vector3(-hipWidth * 0.5f, -thighLen, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Left Calf
            bones.Add(new BoneDefinition
            {
                Name = "LeftCalf",
                ParentName = "LeftThigh",
                LocalPosition = new Vector3(0, -calfLen, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Left Foot
            bones.Add(new BoneDefinition
            {
                Name = "LeftFoot",
                ParentName = "LeftCalf",
                LocalPosition = new Vector3(0, -footLen, footLen * 0.5f),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Right Thigh
            bones.Add(new BoneDefinition
            {
                Name = "RightThigh",
                ParentName = "Hips",
                LocalPosition = new Vector3(hipWidth * 0.5f, -thighLen, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Right Calf
            bones.Add(new BoneDefinition
            {
                Name = "RightCalf",
                ParentName = "RightThigh",
                LocalPosition = new Vector3(0, -calfLen, 0),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Right Foot
            bones.Add(new BoneDefinition
            {
                Name = "RightFoot",
                ParentName = "RightCalf",
                LocalPosition = new Vector3(0, -footLen, footLen * 0.5f),
                LocalRotation = Quaternion.identity,
                LocalScale = Vector3.one
            });

            // Add IK data
            int leftArmChainIdx = bones.FindIndex(b => b.Name == "LeftUpperArm");
            int rightArmChainIdx = bones.FindIndex(b => b.Name == "RightUpperArm");
            int leftLegChainIdx = bones.FindIndex(b => b.Name == "LeftThigh");
            int rightLegChainIdx = bones.FindIndex(b => b.Name == "RightThigh");

            bones[leftArmChainIdx] = bones[leftArmChainIdx].WithIK(
                "LeftHand", LandmarkId.LeftWrist, LandmarkId.LeftElbow, 3, 1.0f);
            bones[rightArmChainIdx] = bones[rightArmChainIdx].WithIK(
                "RightHand", LandmarkId.RightWrist, LandmarkId.RightElbow, 3, 1.0f);
            bones[leftLegChainIdx] = bones[leftLegChainIdx].WithIK(
                "LeftFoot", LandmarkId.LeftAnkle, LandmarkId.LeftKnee, 3, 1.0f);
            bones[rightLegChainIdx] = bones[rightLegChainIdx].WithIK(
                "RightFoot", LandmarkId.RightAnkle, LandmarkId.RightKnee, 3, 1.0f);

            // Compute bind poses
            var bindPoses = ComputeBindPoses(bones.ToArray());
            var inverseBindPoses = new Matrix4x4[bindPoses.Length];
            for (int i = 0; i < bindPoses.Length; i++)
            {
                inverseBindPoses[i] = bindPoses[i].inverse;
            }

            return new SkeletonDefinition
            {
                Bones = bones.ToArray(),
                BindPoses = bindPoses,
                InverseBindPoses = inverseBindPoses
            };
        }

        private static Matrix4x4[] ComputeBindPoses(BoneDefinition[] bones)
        {
            var worldMatrices = new Matrix4x4[bones.Length];
            var nameToIndex = new Dictionary<string, int>();

            for (int i = 0; i < bones.Length; i++)
            {
                nameToIndex[bones[i].Name] = i;
            }

            for (int i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];
                var localMatrix = Matrix4x4.TRS(bone.LocalPosition, bone.LocalRotation, bone.LocalScale);

                if (string.IsNullOrEmpty(bone.ParentName))
                {
                    worldMatrices[i] = localMatrix;
                }
                else
                {
                    int parentIdx = nameToIndex[bone.ParentName];
                    worldMatrices[i] = worldMatrices[parentIdx] * localMatrix;
                }
            }

            return worldMatrices;
        }
    }

    public static class BoneDefinitionExtensions
    {
        public static BoneDefinition WithIK(this BoneDefinition bone, string effector, LandmarkId target, LandmarkId pole, int length, float weight)
        {
            bone.HasIK = true;
            bone.IKData = new IKChainData
            {
                EffectorBone = effector,
                TargetLandmark = target,
                PoleLandmark = pole,
                ChainLength = length,
                Weight = weight
            };
            return bone;
        }
    }
}