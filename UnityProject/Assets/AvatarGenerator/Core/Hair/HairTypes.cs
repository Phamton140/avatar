using System.Collections.Generic;
using UnityEngine;

namespace AvatarGenerator.Core.Hair
{
    public enum HairStyle
    {
        Bald,
        Short,
        Medium,
        Long,
        Ponytail,
        Bun,
        Afro,
        CurlyShort,
        CurlyLong,
        BuzzCut,
        Undercut,
        Bob,
        Pixie,
        Braids,
        Dreadlocks
    }

    public enum HairLength
    {
        Bald = 0,
        VeryShort = 1,
        Short = 2,
        Medium = 3,
        Long = 4,
        VeryLong = 5
    }

    [System.Serializable]
    public struct HairAsset
    {
        public string Id;
        public string DisplayName;
        public HairStyle Style;
        public HairLength Length;
        public Mesh[] LODs;
        public AttachmentPoint[] Attachments;
        public HairMorphTarget[] Morphs;
        public Material Material;
        public float Mass;
        public float Stiffness;
        public float Damping;
    }

    [System.Serializable]
    public struct AttachmentPoint
    {
        public string Name;
        public LandmarkId Landmark;
        public Vector3 LocalOffset;
        public Quaternion LocalRotation;
        public float InfluenceRadius;
    }

    [System.Serializable]
    public struct HairMorphTarget
    {
        public string Name;
        public int[] VertexIndices;
        public Vector3[] VertexDeltas;
    }

    [System.Serializable]
    public struct HairInstance
    {
        public string AssetId;
        public HairAsset Asset;
        public Color Color;
        public float LengthScale;
        public float VolumeScale;
        public Matrix4x4[] AttachmentTransforms;
        public Mesh DeformedMesh;
    }

    public enum LandmarkId
    {
        HeadTop,
        Forehead,
        LeftTemple,
        RightTemple,
        LeftEar,
        RightEar,
        Nape,
        BackHead
    }

    public interface IHairGenerator
    {
        HairAsset GenerateHair(HairStyle style, HairLength length, Color color);
        HairInstance CreateInstance(HairAsset asset, SkeletonDefinition skeleton, LandmarkTarget[] landmarks);
        Mesh DeformHair(HairInstance instance, SkeletonDefinition skeleton, LandmarkTarget[] landmarks);
    }
}