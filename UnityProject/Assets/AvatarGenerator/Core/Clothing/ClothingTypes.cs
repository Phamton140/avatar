using System.Collections.Generic;
using UnityEngine;

namespace AvatarGenerator.Core.Clothing
{
    public enum ClothingSlot
    {
        Shirt,
        Pants,
        Shoes,
        Jacket,
        Hat,
        Gloves,
        Accessory
    }

    public enum ClothingType
    {
        UpperBody,
        LowerBody,
        FullBody,
        Feet,
        Head,
        Hands,
        Accessory
    }

    [System.Serializable]
    public struct ClothingCapsule
    {
        public string BoneName;
        public Vector3 LocalCenter;
        public float Radius;
        public float Height;
        public CapsuleDirection Direction;
    }

    public enum CapsuleDirection
    {
        AlongBone,
        Perpendicular,
        Custom
    }

    [System.Serializable]
    public struct ClothingAsset
    {
        public string Id;
        public string DisplayName;
        public ClothingSlot Slot;
        public ClothingType Type;
        public Mesh BaseMesh;
        public ClothingCapsule[] Capsules;
        public Dictionary<string, float> ParameterOverrides;
        public string[] CompatibleBodyTypes;
        public Material Material;
    }

    [System.Serializable]
    public struct ClothingInstance
    {
        public string AssetId;
        public ClothingAsset Asset;
        public Color Color;
        public float[] MorphWeights;
        public Matrix4x4[] BoneMatrices;
    }

    public struct ClothingDeformContext
    {
        public SkeletonDefinition Skeleton;
        public ClothingCapsule[] BodyCapsules;
        public RegionDeformResult[] BodyRegions;
        public Matrix4x4[] BoneWorldMatrices;
    }

    public interface IClothingDeformer
    {
        Mesh DeformClothing(Mesh baseClothing, ClothingDeformContext context, ClothingAsset asset);
        ClothingCapsule[] GenerateBodyCapsules(SkeletonDefinition skeleton, RegionDeformResult[] regions);
    }
}