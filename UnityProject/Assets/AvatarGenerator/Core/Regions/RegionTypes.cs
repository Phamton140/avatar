using UnityEngine;

namespace AvatarGenerator.Core.Regions
{
    public enum BodyRegion
    {
        Root,
        Head,
        Neck,
        Torso,
        Chest,
        Abdomen,
        Pelvis,
        LeftClavicle,
        LeftUpperArm,
        LeftForearm,
        LeftHand,
        RightClavicle,
        RightUpperArm,
        RightForearm,
        RightHand,
        LeftThigh,
        LeftCalf,
        LeftFoot,
        RightThigh,
        RightCalf,
        RightFoot
    }

    public enum DeformerType
    {
        ScaleLength,
        ScaleRadius,
        ScaleVolume,
        MorphBlend,
        ProceduralOffset
    }

    [Serializable]
    public struct RegionDefinition
    {
        public string Id;
        public BodyRegion SemanticRegion;
        public string[] Bones;
        public LandmarkId[] BoundaryLandmarks;
        public string[] PrimaryParams;
        public string[] SecondaryParams;
        public DeformerType[] Deformers;
    }

    public struct RegionDeformResult
    {
        public Vector3 Scale;
        public float[] MorphWeights;
        public ComputeBuffer VertexOffsets;
        public Matrix4x4 RootTransform;
    }

    public interface IRegionDeformer
    {
        RegionDeformResult Deform(RegionDefinition region, SkeletonDefinition skeleton, IResolvedParameters resolved);
    }
}