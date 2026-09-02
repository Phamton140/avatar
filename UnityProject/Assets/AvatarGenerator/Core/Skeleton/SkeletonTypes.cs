using System;
using UnityEngine;

namespace AvatarGenerator.Core.Skeleton
{
    [Serializable]
    public struct BoneDefinition
    {
        public string Name;
        public string ParentName;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;
        public ParameterDriver[] Drivers;
        public LandmarkId[] ControlledLandmarks;
        public bool HasIK;
        public IKChainData IKData;
    }

    [Serializable]
    public struct ParameterDriver
    {
        public string ParameterId;
        public DriverType Type;
        public float Multiplier;
        public string TargetProperty;
    }

    public enum DriverType
    {
        ScaleLength,
        ScaleWidth,
        PositionOffset,
        RotationOffset
    }

    [Serializable]
    public struct IKChainData
    {
        public string EffectorBone;
        public LandmarkId TargetLandmark;
        public LandmarkId PoleLandmark;
        public int ChainLength;
        public float Weight;
    }

    [Serializable]
    public struct SkeletonDefinition
    {
        public BoneDefinition[] Bones;
        public Matrix4x4[] BindPoses;
        public Matrix4x4[] InverseBindPoses;

        public int GetBoneIndex(string name)
        {
            for (int i = 0; i < Bones.Length; i++)
            {
                if (Bones[i].Name == name) return i;
            }
            return -1;
        }

        public Vector3 GetBoneWorldPosition(int index, Matrix4x4[] worldMatrices)
        {
            if (index >= 0 && index < worldMatrices.Length)
            {
                return worldMatrices[index].GetColumn(3);
            }
            return Vector3.zero;
        }

        public Matrix4x4 GetBoneWorldMatrix(int index, Matrix4x4[] worldMatrices)
        {
            return index >= 0 && index < worldMatrices.Length ? worldMatrices[index] : Matrix4x4.identity;
        }
    }
}