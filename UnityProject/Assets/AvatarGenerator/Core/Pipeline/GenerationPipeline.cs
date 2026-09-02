using System.Collections.Generic;
using AvatarGenerator.Core.Parameters;
using AvatarGenerator.Core.Dependencies;
using AvatarGenerator.Core.Resolution;
using AvatarGenerator.Core.Landmarks;
using AvatarGenerator.Core.Skeleton;
using AvatarGenerator.Core.Regions;
using AvatarGenerator.Core.Morph;
using AvatarGenerator.Core.Pipeline;
using UnityEngine;

namespace AvatarGenerator.Core.Pipeline
{
    public class GenerationPipeline : IGenerationPipeline
    {
        private readonly CanonModel _canon;
        private readonly IRuleEngine _ruleEngine;
        private readonly IDependencyGraph _dependencyGraph;
        private readonly IRegionDeformer _regionDeformer;
        private readonly IExpressionEvaluator _expressionEvaluator;
        private readonly MorphBlender _morphBlender;
        private readonly MorphDefinition[] _bodyMorphs;
        private readonly MorphDefinition[] _faceMorphs;

        public GenerationPipeline(
            CanonModel canon,
            IRuleEngine ruleEngine,
            IDependencyGraph dependencyGraph,
            IRegionDeformer regionDeformer,
            IExpressionEvaluator expressionEvaluator)
        {
            _canon = canon;
            _ruleEngine = ruleEngine;
            _dependencyGraph = dependencyGraph;
            _regionDeformer = regionDeformer;
            _expressionEvaluator = expressionEvaluator;

            var morphGen = new ProceduralMorphGenerator(canon);
            int estimatedVerts = 5000;
            _bodyMorphs = morphGen.GenerateBodyMorphs(estimatedVerts);
            _faceMorphs = morphGen.GenerateFaceMorphs(estimatedVerts);
            _morphBlender = new MorphBlender(_bodyMorphs);
        }

        public GeneratedCharacter Generate(CharacterDefinition definition)
        {
            var cache = new PipelineCache();
            return GenerateIncremental(definition, cache, null);
        }

        public GeneratedCharacter GenerateIncremental(CharacterDefinition definition, PipelineCache cache, HashSet<string> changedParams)
        {
            var bag = definition.ToParameterBag();

            if (changedParams != null && changedParams.Count > 0)
            {
                foreach (var p in changedParams)
                    bag.MarkDirty(p);
            }

            var resolved = PriorityResolver.Resolve(bag, _canon, _ruleEngine, _dependencyGraph);

            var paramsHash = resolved.ComputeHash();
            if (cache.ParamsHash == paramsHash && cache.FinalCharacter.FinalMesh != null)
            {
                return cache.FinalCharacter;
            }

            cache.ParamsHash = paramsHash;

            var skeleton = SkeletonBuilderFK.BuildFromCanon(_canon, resolved);
            var skeletonHash = Hash(skeleton, resolved);
            cache.SkeletonHash = skeletonHash;
            cache.Skeleton = skeleton;

            var landmarks = LandmarkTargetGenerator.Generate(resolved, _canon);
            var landmarkHash = Hash(landmarks);
            cache.GeometryHash = landmarkHash;

            IKSolver.SolveAll(skeleton, landmarks);

            var regions = CreateRegionDefinitions();
            var regionResults = new RegionDeformResult[regions.Length];
            for (int i = 0; i < regions.Length; i++)
            {
                regionResults[i] = _regionDeformer.Deform(regions[i], skeleton, resolved);
            }

            var regionHash = Hash(regionResults);
            cache.MorphHash = regionHash;

            var bodyMorphWeights = _morphBlender.ComputeWeights(resolved, _canon);
            var bodyMorphDeltas = _morphBlender.Blend(bodyMorphWeights, _bodyMorphs);

            var faceMorphBlender = new MorphBlender(_faceMorphs);
            var faceMorphWeights = faceMorphBlender.ComputeWeights(resolved, _canon);
            var faceMorphDeltas = faceMorphBlender.Blend(faceMorphWeights, _faceMorphs);

            var finalMesh = GenerateFinalMesh(skeleton, regionResults, resolved, bodyMorphDeltas, faceMorphDeltas);
            var finalHash = Hash(finalMesh);

            var validation = Validate(skeleton, finalMesh, resolved);

            var character = new GeneratedCharacter
            {
                FinalMesh = finalMesh,
                Skeleton = skeleton,
                Materials = new MaterialPropertyBlock(),
                Validation = validation,
                ContentHash = finalHash
            };

            cache.FinalHash = finalHash;
            cache.FinalCharacter = character;

            return character;
        }

        private RegionDefinition[] CreateRegionDefinitions()
        {
            return new[]
            {
                new RegionDefinition
                {
                    Id = "Head",
                    SemanticRegion = BodyRegion.Head,
                    Bones = new[] { "Head" },
                    PrimaryParams = new[] { "body.headScale" },
                    Deformers = new[] { DeformerType.ScaleLength, DeformerType.ScaleRadius, DeformerType.ScaleVolume }
                },
                new RegionDefinition
                {
                    Id = "Torso",
                    SemanticRegion = BodyRegion.Torso,
                    Bones = new[] { "Spine1", "Spine2", "Spine3" },
                    PrimaryParams = new[] { "body.torsoScale", "body.chestWidth", "body.bodyFat", "body.muscleMass" },
                    Deformers = new[] { DeformerType.ScaleLength, DeformerType.ScaleRadius, DeformerType.ScaleVolume, DeformerType.MorphBlend }
                },
                new RegionDefinition
                {
                    Id = "LeftArm",
                    SemanticRegion = BodyRegion.LeftUpperArm,
                    Bones = new[] { "LeftUpperArm", "LeftForearm", "LeftHand" },
                    PrimaryParams = new[] { "body.armLength" },
                    SecondaryParams = new[] { "body.muscleMass", "body.bodyFat" },
                    Deformers = new[] { DeformerType.ScaleLength, DeformerType.ScaleRadius, DeformerType.ScaleVolume }
                },
                new RegionDefinition
                {
                    Id = "RightArm",
                    SemanticRegion = BodyRegion.RightUpperArm,
                    Bones = new[] { "RightUpperArm", "RightForearm", "RightHand" },
                    PrimaryParams = new[] { "body.armLength" },
                    SecondaryParams = new[] { "body.muscleMass", "body.bodyFat" },
                    Deformers = new[] { DeformerType.ScaleLength, DeformerType.ScaleRadius, DeformerType.ScaleVolume }
                },
                new RegionDefinition
                {
                    Id = "LeftLeg",
                    SemanticRegion = BodyRegion.LeftThigh,
                    Bones = new[] { "LeftThigh", "LeftCalf", "LeftFoot" },
                    PrimaryParams = new[] { "body.legLength" },
                    SecondaryParams = new[] { "body.muscleMass", "body.bodyFat" },
                    Deformers = new[] { DeformerType.ScaleLength, DeformerType.ScaleRadius, DeformerType.ScaleVolume }
                },
                new RegionDefinition
                {
                    Id = "RightLeg",
                    SemanticRegion = BodyRegion.RightThigh,
                    Bones = new[] { "RightThigh", "RightCalf", "RightFoot" },
                    PrimaryParams = new[] { "body.legLength" },
                    SecondaryParams = new[] { "body.muscleMass", "body.bodyFat" },
                    Deformers = new[] { DeformerType.ScaleLength, DeformerType.ScaleRadius, DeformerType.ScaleVolume }
                }
            };
        }

        private Mesh GenerateFinalMesh(SkeletonDefinition skeleton, RegionDeformResult[] regions, IResolvedParameters resolved, MorphDeltas bodyMorphs, MorphDeltas faceMorphs)
        {
            var mesh = new Mesh();
            mesh.name = "ProceduralCharacter";

            var vertices = new List<Vector3>();
            var boneWeights = new List<BoneWeight>();
            var bindPoses = new List<Matrix4x4>();
            var triangles = new List<int>();

            int vertexOffset = 0;
            var nameToIndex = new Dictionary<string, int>();
            for (int i = 0; i < skeleton.Bones.Length; i++)
                nameToIndex[skeleton.Bones[i].Name] = i;

            var morphDeltas = new Dictionary<int, Vector3>();
            if (bodyMorphs.VertexIndices != null)
            {
                for (int i = 0; i < bodyMorphs.VertexIndices.Length; i++)
                {
                    morphDeltas[bodyMorphs.VertexIndices[i]] = bodyMorphs.Deltas[i];
                }
            }
            if (faceMorphs.VertexIndices != null)
            {
                for (int i = 0; i < faceMorphs.VertexIndices.Length; i++)
                {
                    int idx = faceMorphs.VertexIndices[i];
                    if (morphDeltas.ContainsKey(idx))
                        morphDeltas[idx] += faceMorphs.Deltas[i];
                    else
                        morphDeltas[idx] = faceMorphs.Deltas[i];
                }
            }

            foreach (var region in regions)
            {
                var boneIdx = region.RootTransform != Matrix4x4.identity
                    ? FindBoneIndexForTransform(skeleton, region.RootTransform)
                    : -1;

                if (boneIdx < 0) continue;

                var bone = skeleton.Bones[boneIdx];
                var scale = region.Scale;

                var regionMesh = CreatePrimitiveForBone(bone, scale);
                int startVert = vertices.Count;

                for (int i = 0; i < regionMesh.vertices.Length; i++)
                {
                    var v = regionMesh.vertices[i];
                    int globalIdx = startVert + i;

                    if (morphDeltas.TryGetValue(globalIdx, out var delta))
                    {
                        v += delta;
                    }

                    vertices.Add(v);
                    var bw = new BoneWeight
                    {
                        boneIndex0 = boneIdx,
                        weight0 = 1f
                    };
                    boneWeights.Add(bw);
                }

                foreach (var t in regionMesh.triangles)
                {
                    triangles.Add(t + startVert);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.boneWeights = boneWeights.ToArray();
            mesh.bindposes = skeleton.BindPoses;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private int FindBoneIndexForTransform(SkeletonDefinition skeleton, Matrix4x4 transform)
        {
            for (int i = 0; i < skeleton.Bones.Length; i++)
            {
                var bone = skeleton.Bones[i];
                var boneWorld = skeleton.BindPoses[i];
                if (Vector3.Distance(boneWorld.GetColumn(3), transform.GetColumn(3)) < 0.01f)
                    return i;
            }
            return -1;
        }

        private Mesh CreatePrimitiveForBone(BoneDefinition bone, Vector3 scale)
        {
            float length = bone.LocalPosition.magnitude * scale.x;
            float radius = Mathf.Max(scale.y, scale.z) * 0.1f;

            if (length < 0.01f)
            {
                var sphere = CreateSphere(radius * 2f);
                return sphere;
            }

            return CreateCapsule(length, radius);
        }

        private Mesh CreateCapsule(float height, float radius)
        {
            var mesh = new Mesh();
            int rings = 8;
            int segments = 12;
            var verts = new List<Vector3>();
            var tris = new List<int>();

            for (int i = 0; i <= rings; i++)
            {
                float v = (float)i / rings;
                float y = Mathf.Lerp(-height * 0.5f, height * 0.5f, v);
                float r = radius;

                if (v < 0.5f)
                {
                    float t = v * 2f;
                    r = Mathf.Sqrt(radius * radius - (y + height * 0.5f) * (y + height * 0.5f));
                    r = Mathf.Max(r, 0.001f);
                }
                else
                {
                    float t = (v - 0.5f) * 2f;
                    r = Mathf.Sqrt(radius * radius - (y - height * 0.5f) * (y - height * 0.5f));
                    r = Mathf.Max(r, 0.001f);
                }

                for (int j = 0; j < segments; j++)
                {
                    float u = (float)j / segments;
                    float angle = u * Mathf.PI * 2f;
                    verts.Add(new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
                }
            }

            for (int i = 0; i < rings; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    int a = i * segments + j;
                    int b = i * segments + (j + 1) % segments;
                    int c = (i + 1) * segments + j;
                    int d = (i + 1) * segments + (j + 1) % segments;

                    tris.Add(a); tris.Add(c); tris.Add(b);
                    tris.Add(b); tris.Add(c); tris.Add(d);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            return mesh;
        }

        private Mesh CreateSphere(float radius)
        {
            var mesh = new Mesh();
            int rings = 8;
            int segments = 12;
            var verts = new List<Vector3>();
            var tris = new List<int>();

            for (int i = 0; i <= rings; i++)
            {
                float v = (float)i / rings;
                float phi = v * Mathf.PI;
                float y = Mathf.Cos(phi) * radius;
                float r = Mathf.Sin(phi) * radius;

                for (int j = 0; j < segments; j++)
                {
                    float u = (float)j / segments;
                    float theta = u * Mathf.PI * 2f;
                    verts.Add(new Vector3(Mathf.Cos(theta) * r, y, Mathf.Sin(theta) * r));
                }
            }

            for (int i = 0; i < rings; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    int a = i * segments + j;
                    int b = i * segments + (j + 1) % segments;
                    int c = (i + 1) * segments + j;
                    int d = (i + 1) * segments + (j + 1) % segments;

                    tris.Add(a); tris.Add(c); tris.Add(b);
                    tris.Add(b); tris.Add(c); tris.Add(d);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            return mesh;
        }

        private ValidationResult Validate(SkeletonDefinition skeleton, Mesh mesh, IResolvedParameters resolved)
        {
            var result = new ValidationResult();

            foreach (var bone in skeleton.Bones)
            {
                if (bone.LocalPosition.magnitude < 0.001f && !string.IsNullOrEmpty(bone.Name))
                {
                    result.AddIssue(new ValidationIssue
                    {
                        ParameterId = $"bone.{bone.Name}",
                        Severity = ValidationSeverity.Error,
                        Message = $"Bone {bone.Name} has near-zero length",
                        IsBlocking = true
                    });
                }
            }

            if (mesh.vertexCount == 0)
            {
                result.AddIssue(new ValidationIssue
                {
                    ParameterId = "mesh",
                    Severity = ValidationSeverity.Error,
                    Message = "Generated mesh has no vertices",
                    IsBlocking = true
                });
            }

            return result;
        }

        private Hash128 Hash(params object[] objects)
        {
            var hash = new Hash128();
            foreach (var obj in objects)
            {
                if (obj == null) continue;
                if (obj is IResolvedParameters rp)
                    hash.Append(rp.ComputeHash());
                else if (obj is SkeletonDefinition sk)
                {
                    foreach (var b in sk.Bones)
                    {
                        hash.Append(b.Name);
                        hash.Append(b.LocalPosition);
                        hash.Append(b.LocalRotation);
                    }
                }
                else if (obj is LandmarkTarget[] lts)
                {
                    foreach (var lt in lts)
                    {
                        hash.Append(lt.Landmark.ToString());
                        hash.Append(lt.TargetPosition);
                        hash.Append(lt.Weight);
                    }
                }
                else if (obj is RegionDeformResult[] rrs)
                {
                    foreach (var rr in rrs)
                    {
                        hash.Append(rr.Scale);
                    }
                }
                else if (obj is Mesh m)
                {
                    hash.Append(m.vertexCount);
                    hash.Append(m.triangles.Length);
                }
                else
                {
                    hash.Append(obj.GetHashCode());
                }
            }
            return hash;
        }
    }

    public interface IGenerationPipeline
    {
        GeneratedCharacter Generate(CharacterDefinition definition);
        GeneratedCharacter GenerateIncremental(CharacterDefinition definition, PipelineCache cache, HashSet<string> changedParams);
    }
}