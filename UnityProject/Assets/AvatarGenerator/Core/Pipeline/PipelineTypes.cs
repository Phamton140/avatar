using System.Collections.Generic;
using AvatarGenerator.Core.Parameters;
using AvatarGenerator.Core.Skeleton;
using AvatarGenerator.Core.Resolution;
using UnityEngine;

namespace AvatarGenerator.Core.Pipeline
{
    public struct PipelineCache
    {
        public Hash128 ParamsHash;
        public Hash128 SkeletonHash;
        public Hash128 GeometryHash;
        public Hash128 MorphHash;
        public Hash128 FinalHash;
        public SkeletonDefinition Skeleton;
        public Mesh BaseMesh;
        public float[] MorphWeights;
        public Dictionary<string, ModuleCache> ModuleCaches;
        public GeneratedCharacter FinalCharacter;
    }

    public struct ModuleCache
    {
        public Hash128 Hash;
        public Mesh Mesh;
        public MorphDeltas MorphDeltas;
    }

    public struct MorphDeltas
    {
        public int[] VertexIndices;
        public Vector3[] Deltas;
    }

    public struct GeneratedCharacter
    {
        public Mesh FinalMesh;
        public SkeletonDefinition Skeleton;
        public MaterialPropertyBlock Materials;
        public ValidationResult Validation;
        public Hash128 ContentHash;
    }

    public struct PipelineContext
    {
        public IResolvedParameters ResolvedParams;
        public CanonModel Canon;
        public SkeletonDefinition Skeleton;
        public Mesh BaseMesh;
        public LandmarkTarget[] LandmarkTargets;
        public RegionDeformResult[] RegionResults;
        public float[] MorphWeights;
    }

    public enum PipelineStage
    {
        ParameterResolution = 0,
        GlobalTransform = 1,
        SkeletonFK = 2,
        LandmarkTargets = 3,
        SkeletonIK = 4,
        RegionScales = 5,
        VertexDeform = 6,
        MorphBlend = 7,
        Correctives = 8,
        Skinning = 9,
        Composition = 10
    }
}