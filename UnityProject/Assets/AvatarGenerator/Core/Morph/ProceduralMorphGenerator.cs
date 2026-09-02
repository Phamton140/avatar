using System.Collections.Generic;
using AvatarGenerator.Core.Parameters;
using AvatarGenerator.Core.Skeleton;
using UnityEngine;

namespace AvatarGenerator.Core.Morph
{
    public class ProceduralMorphGenerator
    {
        private readonly CanonModel _canon;

        public ProceduralMorphGenerator(CanonModel canon)
        {
            _canon = canon;
        }

        public MorphDefinition[] GenerateBodyMorphs(int estimatedVertexCount)
        {
            var morphs = new List<MorphDefinition>();

            morphs.Add(GenerateSexDimorphism(estimatedVertexCount));
            morphs.Add(GenerateMuscularMorph(estimatedVertexCount));
            morphs.Add(GenerateHeavyMorph(estimatedVertexCount));
            morphs.Add(GenerateThinMorph(estimatedVertexCount));
            morphs.Add(GenerateLongLimbsMorph(estimatedVertexCount));
            morphs.Add(GenerateShortLimbsMorph(estimatedVertexCount));
            morphs.Add(GenerateWideBodyMorph(estimatedVertexCount));
            morphs.Add(GenerateNarrowBodyMorph(estimatedVertexCount));

            return morphs.ToArray();
        }

        public MorphDefinition[] GenerateFaceMorphs(int estimatedVertexCount)
        {
            var morphs = new List<MorphDefinition>();

            morphs.Add(GenerateJawWidthMorph(estimatedVertexCount));
            morphs.Add(GenerateEyeSizeMorph(estimatedVertexCount));
            morphs.Add(GenerateNoseSizeMorph(estimatedVertexCount));
            morphs.Add(GenerateFaceWidthMorph(estimatedVertexCount));

            return morphs.ToArray();
        }

        private MorphDefinition GenerateSexDimorphism(int vertexCount)
        {
            var affected = GetRegionVertexIndices(vertexCount, new[] { "Chest", "Hip", "Waist", "Shoulder" });
            var deltas = new Vector3[affected.Length];

            for (int i = 0; i < affected.Length; i++)
            {
                float y = (float)affected[i] / vertexCount * _canon.BaseHeight;
                float x = Mathf.Sin((float)affected[i] * 0.1f) * 0.2f;
                float z = Mathf.Cos((float)affected[i] * 0.1f) * 0.2f;

                float hipFactor = Mathf.InverseLerp(0.8f, 1.0f, y / _canon.BaseHeight);
                float chestFactor = Mathf.InverseLerp(1.2f, 1.4f, y / _canon.BaseHeight);

                deltas[i] = new Vector3(
                    x * hipFactor * 0.08f,
                    0,
                    z * chestFactor * 0.03f
                );
            }

            return new MorphDefinition
            {
                Id = "SexDimorphism",
                DisplayName = "Male/Female",
                Category = MorphCategory.Identity,
                DrivenParameters = new[] { "body.sex" },
                MinWeight = 0f,
                MaxWeight = 1f,
                AffectedVertexIndices = affected,
                VertexDeltas = deltas
            };
        }

        private MorphDefinition GenerateMuscularMorph(int vertexCount)
        {
            var affected = GetRegionVertexIndices(vertexCount, new[] { "UpperArm", "Forearm", "Thigh", "Calf", "Chest", "Abdomen" });
            var deltas = new Vector3[affected.Length];

            for (int i = 0; i < affected.Length; i++)
            {
                float y = (float)affected[i] / vertexCount * _canon.BaseHeight;
                float distFromCenter = Mathf.Abs(Mathf.Sin((float)affected[i] * 0.1f)) * 0.2f;
                float bulge = Mathf.Max(0f, 1f - distFromCenter * 5f);
                bulge = bulge * bulge;

                deltas[i] = new Vector3(distFromCenter, 0, distFromCenter).normalized * bulge * 0.04f;
            }

            return new MorphDefinition
            {
                Id = "Muscular",
                DisplayName = "Muscular Definition",
                Category = MorphCategory.BodyType,
                DrivenParameters = new[] { "body.muscleMass" },
                MinWeight = 0f,
                MaxWeight = 1f,
                AffectedVertexIndices = affected,
                VertexDeltas = deltas
            };
        }

        private MorphDefinition GenerateHeavyMorph(int vertexCount)
        {
            var affected = GetRegionVertexIndices(vertexCount, new[] { "Abdomen", "Hip", "Thigh", "UpperArm", "Chest" });
            var deltas = new Vector3[affected.Length];

            for (int i = 0; i < affected.Length; i++)
            {
                float y = (float)affected[i] / vertexCount * _canon.BaseHeight;
                float x = Mathf.Sin((float)affected[i] * 0.1f) * 0.2f;
                float z = Mathf.Cos((float)affected[i] * 0.1f) * 0.2f;
                float hang = Mathf.Max(0f, (1.2f - y / _canon.BaseHeight) * 0.5f);

                deltas[i] = new Vector3(
                    x * hang * 0.15f,
                    -hang * 0.05f,
                    z * hang * 0.15f
                );
            }

            return new MorphDefinition
            {
                Id = "Heavy",
                DisplayName = "Body Fat Distribution",
                Category = MorphCategory.BodyType,
                DrivenParameters = new[] { "body.bodyFat" },
                MinWeight = 0f,
                MaxWeight = 1f,
                AffectedVertexIndices = affected,
                VertexDeltas = deltas
            };
        }

        private MorphDefinition GenerateThinMorph(int vertexCount)
        {
            var affected = GetRegionVertexIndices(vertexCount, new[] { "UpperArm", "Forearm", "Thigh", "Calf", "Abdomen", "Chest" });
            var deltas = new Vector3[affected.Length];

            for (int i = 0; i < affected.Length; i++)
            {
                float x = Mathf.Sin((float)affected[i] * 0.1f) * 0.2f;
                float z = Mathf.Cos((float)affected[i] * 0.1f) * 0.2f;

                deltas[i] = new Vector3(
                    -x * 0.1f,
                    0,
                    -z * 0.1f
                );
            }

            return new MorphDefinition
            {
                Id = "Thin",
                DisplayName = "Thin Build",
                Category = MorphCategory.BodyType,
                DrivenParameters = new[] { "body.muscleMass", "body.bodyFat" },
                MinWeight = -1f,
                MaxWeight = 0f,
                AffectedVertexIndices = affected,
                VertexDeltas = deltas
            };
        }

        private MorphDefinition GenerateLongLimbsMorph(int vertexCount)
        {
            var affected = GetRegionVertexIndices(vertexCount, new[] { "UpperArm", "Forearm", "Thigh", "Calf" });
            var deltas = new Vector3[affected.Length];

            for (int i = 0; i < affected.Length; i++)
            {
                float x = Mathf.Sin((float)affected[i] * 0.1f) * 0.2f;
                float z = Mathf.Cos((float)affected[i] * 0.1f) * 0.2f;

                deltas[i] = new Vector3(x, 0, z).normalized * 0.02f;
            }

            return new MorphDefinition
            {
                Id = "LongLimbs",
                DisplayName = "Long Limbs",
                Category = MorphCategory.Proportional,
                DrivenParameters = new[] { "body.armLength", "body.legLength" },
                MinWeight = 0f,
                MaxWeight = 1f,
                AffectedVertexIndices = affected,
                VertexDeltas = deltas
            };
        }

        private MorphDefinition GenerateShortLimbsMorph(int vertexCount)
        {
            var affected = GetRegionVertexIndices(vertexCount, new[] { "UpperArm", "Forearm", "Thigh", "Calf" });
            var deltas = new Vector3[affected.Length];

            for (int i = 0; i < affected.Length; i++)
            {
                float x = Mathf.Sin((float)affected[i] * 0.1f) * 0.2f;
                float z = Mathf.Cos((float)affected[i] * 0.1f) * 0.2f;

                deltas[i] = -new Vector3(x, 0, z).normalized * 0.02f;
            }

            return new MorphDefinition
            {
                Id = "ShortLimbs",
                DisplayName = "Short Limbs",
                Category = MorphCategory.Proportional,
                DrivenParameters = new[] { "body.armLength", "body.legLength" },
                MinWeight = -1f,
                MaxWeight = 0f,
                AffectedVertexIndices = affected,
                VertexDeltas = deltas
            };
        }

        private MorphDefinition GenerateWideBodyMorph(int vertexCount)
        {
            var affected = GetRegionVertexIndices(vertexCount, new[] { "Chest", "Shoulder", "Hip" });
            var deltas = new Vector3[affected.Length];

            for (int i = 0; i < affected.Length; i++)
            {
                float x = Mathf.Sin((float)affected[i] * 0.1f) * 0.2f;
                float z = Mathf.Cos((float)affected[i] * 0.1f) * 0.2f;

                deltas[i] = new Vector3(x * 0.15f, 0, z * 0.15f);
            }

            return new MorphDefinition
            {
                Id = "WideBody",
                DisplayName = "Wide Body",
                Category = MorphCategory.Proportional,
                DrivenParameters = new[] { "body.shoulderWidth", "body.chestWidth", "body.hipWidth" },
                MinWeight = 0f,
                MaxWeight = 1f,
                AffectedVertexIndices = affected,
                VertexDeltas = deltas
            };
        }

        private MorphDefinition GenerateNarrowBodyMorph(int vertexCount)
        {
            var affected = GetRegionVertexIndices(vertexCount, new[] { "Chest", "Shoulder", "Hip" });
            var deltas = new Vector3[affected.Length];

            for (int i = 0; i < affected.Length; i++)
            {
                float x = Mathf.Sin((float)affected[i] * 0.1f) * 0.2f;
                float z = Mathf.Cos((float)affected[i] * 0.1f) * 0.2f;

                deltas[i] = new Vector3(-x * 0.1f, 0, -z * 0.1f);
            }

            return new MorphDefinition
            {
                Id = "NarrowBody",
                DisplayName = "Narrow Body",
                Category = MorphCategory.Proportional,
                DrivenParameters = new[] { "body.shoulderWidth", "body.chestWidth", "body.hipWidth" },
                MinWeight = -1f,
                MaxWeight = 0f,
                AffectedVertexIndices = affected,
                VertexDeltas = deltas
            };
        }

        private MorphDefinition GenerateJawWidthMorph(int vertexCount)
        {
            var affected = GetRegionVertexIndices(vertexCount, new[] { "Jaw" });
            var deltas = new Vector3[affected.Length];

            for (int i = 0; i < affected.Length; i++)
            {
                float x = Mathf.Sin((float)affected[i] * 0.1f) * 0.15f;

                deltas[i] = new Vector3(x * 0.3f, 0, 0);
            }

            return new MorphDefinition
            {
                Id = "JawWidth",
                DisplayName = "Jaw Width",
                Category = MorphCategory.Facial,
                DrivenParameters = new[] { "face.jawWidth" },
                MinWeight = -0.5f,
                MaxWeight = 1f,
                AffectedVertexIndices = affected,
                VertexDeltas = deltas
            };
        }

        private MorphDefinition GenerateEyeSizeMorph(int vertexCount)
        {
            var affected = GetRegionVertexIndices(vertexCount, new[] { "Eye" });
            var deltas = new Vector3[affected.Length];

            for (int i = 0; i < affected.Length; i++)
            {
                float x = Mathf.Sin((float)affected[i] * 0.1f) * 0.05f;
                float z = Mathf.Cos((float)affected[i] * 0.1f) * 0.05f;

                deltas[i] = new Vector3(x, 0, z).normalized * 0.015f;
            }

            return new MorphDefinition
            {
                Id = "EyeSize",
                DisplayName = "Eye Size",
                Category = MorphCategory.Facial,
                DrivenParameters = new[] { "face.eyeSize" },
                MinWeight = -0.5f,
                MaxWeight = 1f,
                AffectedVertexIndices = affected,
                VertexDeltas = deltas
            };
        }

        private MorphDefinition GenerateNoseSizeMorph(int vertexCount)
        {
            var affected = GetRegionVertexIndices(vertexCount, new[] { "Nose" });
            var deltas = new Vector3[affected.Length];

            for (int i = 0; i < affected.Length; i++)
            {
                float x = Mathf.Sin((float)affected[i] * 0.1f) * 0.03f;
                float z = Mathf.Cos((float)affected[i] * 0.1f) * 0.03f;

                deltas[i] = new Vector3(x, 0, z).normalized * 0.02f;
            }

            return new MorphDefinition
            {
                Id = "NoseSize",
                DisplayName = "Nose Size",
                Category = MorphCategory.Facial,
                DrivenParameters = new[] { "face.noseSize" },
                MinWeight = -0.5f,
                MaxWeight = 1f,
                AffectedVertexIndices = affected,
                VertexDeltas = deltas
            };
        }

        private MorphDefinition GenerateFaceWidthMorph(int vertexCount)
        {
            var affected = GetRegionVertexIndices(vertexCount, new[] { "Face" });
            var deltas = new Vector3[affected.Length];

            for (int i = 0; i < affected.Length; i++)
            {
                float x = Mathf.Sin((float)affected[i] * 0.1f) * 0.1f;

                deltas[i] = new Vector3(x * 0.2f, 0, 0);
            }

            return new MorphDefinition
            {
                Id = "FaceWidth",
                DisplayName = "Face Width",
                Category = MorphCategory.Facial,
                DrivenParameters = new[] { "face.faceWidth" },
                MinWeight = -0.3f,
                MaxWeight = 0.5f,
                AffectedVertexIndices = affected,
                VertexDeltas = deltas
            };
        }

        private int[] GetRegionVertexIndices(int vertexCount, string[] regionNames)
        {
            var indices = new List<int>();

            for (int i = 0; i < vertexCount; i++)
            {
                string region = GetRegionForVertexIndex(i, vertexCount);

                foreach (var r in regionNames)
                {
                    if (region.Contains(r))
                    {
                        indices.Add(i);
                        break;
                    }
                }
            }

            return indices.ToArray();
        }

        private string GetRegionForVertexIndex(int vertexIndex, int vertexCount)
        {
            float normalizedY = (float)vertexIndex / vertexCount;
            float y = normalizedY * _canon.BaseHeight;
            float x = Mathf.Abs(Mathf.Sin((float)vertexIndex * 0.1f)) * 0.2f;

            if (y > 1.6f) return "Head";
            if (y > 1.4f) return x > 0.2f ? "Shoulder" : "Neck";
            if (y > 1.1f) return x > 0.15f ? "Chest" : "Spine";
            if (y > 0.9f) return "Abdomen";
            if (y > 0.7f) return x > 0.1f ? "Hip" : "Pelvis";
            if (y > 0.3f) return x > 0.1f ? "Thigh" : "Spine";
            if (y > 0.05f) return x > 0.08f ? "Calf" : "Foot";

            return "Foot";
        }
    }
}