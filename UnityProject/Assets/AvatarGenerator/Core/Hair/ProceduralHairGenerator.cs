using System.Collections.Generic;
using AvatarGenerator.Core.Hair;
using AvatarGenerator.Core.Landmarks;
using AvatarGenerator.Core.Skeleton;
using UnityEngine;

namespace AvatarGenerator.Core.Hair
{
    public static class ProceduralHairGenerator
    {
        private static readonly Dictionary<HairStyle, HairStyleConfig> _styleConfigs = new Dictionary<HairStyle, HairStyleConfig>
        {
            [HairStyle.Bald] = new HairStyleConfig { BaseVertices = 0, Layers = 0, Volume = 0f },
            [HairStyle.BuzzCut] = new HairStyleConfig { BaseVertices = 500, Layers = 1, Volume = 0.02f, Length = 0.03f },
            [HairStyle.Short] = new HairStyleConfig { BaseVertices = 1200, Layers = 2, Volume = 0.05f, Length = 0.08f },
            [HairStyle.Medium] = new HairStyleConfig { BaseVertices = 2500, Layers = 3, Volume = 0.08f, Length = 0.15f },
            [HairStyle.Long] = new HairStyleConfig { BaseVertices = 4000, Layers = 4, Volume = 0.12f, Length = 0.25f },
            [HairStyle.VeryLong] = new HairStyleConfig { BaseVertices = 6000, Layers = 5, Volume = 0.15f, Length = 0.35f },
            [HairStyle.Ponytail] = new HairStyleConfig { BaseVertices = 3000, Layers = 3, Volume = 0.1f, Length = 0.3f, HasPonytail = true },
            [HairStyle.Bun] = new HairStyleConfig { BaseVertices = 2000, Layers = 2, Volume = 0.08f, Length = 0.1f, HasBun = true },
            [HairStyle.Afro] = new HairStyleConfig { BaseVertices = 5000, Layers = 4, Volume = 0.2f, Length = 0.12f },
            [HairStyle.CurlyShort] = new HairStyleConfig { BaseVertices = 2000, Layers = 2, Volume = 0.1f, Length = 0.08f, IsCurly = true },
            [HairStyle.CurlyLong] = new HairStyleConfig { BaseVertices = 4500, Layers = 4, Volume = 0.15f, Length = 0.25f, IsCurly = true },
            [HairStyle.Undercut] = new HairStyleConfig { BaseVertices = 1500, Layers = 2, Volume = 0.06f, Length = 0.1f, HasUndercut = true },
            [HairStyle.Bob] = new HairStyleConfig { BaseVertices = 2500, Layers = 3, Volume = 0.08f, Length = 0.12f },
            [HairStyle.Pixie] = new HairStyleConfig { BaseVertices = 1000, Layers = 2, Volume = 0.04f, Length = 0.05f },
            [HairStyle.Braids] = new HairStyleConfig { BaseVertices = 3500, Layers = 3, Volume = 0.1f, Length = 0.25f, HasBraids = true },
            [HairStyle.Dreadlocks] = new HairStyleConfig { BaseVertices = 4000, Layers = 4, Volume = 0.12f, Length = 0.3f, HasDreadlocks = true }
        };

        public static HairAsset GenerateHair(HairStyle style, HairLength length, Color color)
        {
            if (!_styleConfigs.TryGetValue(style, out var config))
            {
                config = _styleConfigs[HairStyle.Short];
            }

            var mesh = CreateHairMesh(style, config, length);
            var attachments = GenerateAttachments(style);
            var morphs = GenerateHairMorphs(style, config);
            var material = CreateHairMaterial(color);

            return new HairAsset
            {
                Id = $"hair_{style}_{length}",
                DisplayName = $"{style} {length}",
                Style = style,
                Length = length,
                LODs = new[] { mesh },
                Attachments = attachments,
                Morphs = morphs,
                Material = material,
                Mass = config.Volume * 0.5f,
                Stiffness = style == HairStyle.Afro ? 0.8f : 0.5f,
                Damping = 0.1f
            };
        }

        public static HairInstance CreateInstance(HairAsset asset, SkeletonDefinition skeleton, LandmarkTarget[] landmarks)
        {
            var attachmentTransforms = new Matrix4x4[asset.Attachments.Length];

            var landmarkMap = new Dictionary<LandmarkId, LandmarkTarget>();
            foreach (var lm in landmarks)
            {
                if (!landmarkMap.ContainsKey(lm.Landmark))
                    landmarkMap[lm.Landmark] = lm;
            }

            for (int i = 0; i < asset.Attachments.Length; i++)
            {
                var att = asset.Attachments[i];
                if (landmarkMap.TryGetValue(att.Landmark, out var target))
                {
                    var pos = target.TargetPosition + att.LocalOffset;
                    var rot = Quaternion.Euler(att.LocalRotation.eulerAngles);
                    attachmentTransforms[i] = Matrix4x4.TRS(pos, rot, Vector3.one);
                }
                else
                {
                    attachmentTransforms[i] = Matrix4x4.identity;
                }
            }

            return new HairInstance
            {
                AssetId = asset.Id,
                Asset = asset,
                Color = asset.Material.color,
                LengthScale = 1f,
                VolumeScale = 1f,
                AttachmentTransforms = attachmentTransforms,
                DeformedMesh = asset.LODs[0]
            };
        }

        public static Mesh DeformHair(HairInstance instance, SkeletonDefinition skeleton, LandmarkTarget[] landmarks)
        {
            var asset = instance.Asset;
            var mesh = asset.LODs[0];

            var deformed = new Mesh();
            deformed.name = mesh.name + "_Deformed";

            var verts = mesh.vertices;
            var norms = mesh.normals;
            var uvs = mesh.uv;
            var tris = mesh.triangles;

            var newVerts = new Vector3[verts.Length];
            var newNorms = new Vector3[norms.Length];

            var landmarkMap = new Dictionary<LandmarkId, LandmarkTarget>();
            foreach (var lm in landmarks)
            {
                if (!landmarkMap.ContainsKey(lm.Landmark))
                    landmarkMap[lm.Landmark] = lm;
            }

            Vector3 headTop = landmarkMap.TryGetValue(LandmarkId.HeadTop, out var ht) ? ht.TargetPosition : Vector3.up * 1.8f;
            Vector3 headCenter = (headTop + (landmarkMap.TryGetValue(LandmarkId.Nape, out var nk) ? nk.TargetPosition : Vector3.up * 1.5f)) * 0.5f;

            for (int i = 0; i < verts.Length; i++)
            {
                var v = verts[i];
                var n = norms[i];

                float heightFactor = Mathf.InverseLerp(headCenter.y - 0.15f, headTop.y, v.y);
                heightFactor = Mathf.Clamp01(heightFactor);

                Vector3 offset = Vector3.zero;

                if (heightFactor > 0.7f)
                {
                    float tipFactor = (heightFactor - 0.7f) / 0.3f;
                    offset += (v - headCenter).normalized * tipFactor * 0.02f * instance.LengthScale;
                }

                if (asset.Style == HairStyle.Ponytail && heightFactor < 0.4f)
                {
                    Vector3 toNape = (landmarkMap.TryGetValue(LandmarkId.Nape, out var nape) ? nape.TargetPosition : headCenter - Vector3.up * 0.15f) - v;
                    offset += toNape.normalized * (0.4f - heightFactor) * 0.05f * instance.VolumeScale;
                }

                if (asset.Style == HairStyle.Bun && heightFactor > 0.6f)
                {
                    Vector3 toCenter = (headCenter + Vector3.up * 0.08f) - v;
                    offset += toCenter.normalized * (heightFactor - 0.6f) * 0.03f * instance.VolumeScale;
                }

                if (asset.Style == HairStyle.Afro)
                {
                    offset += (v - headCenter).normalized * heightFactor * 0.04f * instance.VolumeScale;
                }

                newVerts[i] = v + offset;
                newNorms[i] = n;
            }

            deformed.vertices = newVerts;
            deformed.normals = newNorms;
            deformed.uv = uvs;
            deformed.triangles = tris;
            deformed.RecalculateBounds();
            deformed.RecalculateNormals();
            deformed.RecalculateTangents();

            instance.DeformedMesh = deformed;
            return deformed;
        }

        private static Mesh CreateHairMesh(HairStyle style, HairStyleConfig config, HairLength length)
        {
            var mesh = new Mesh { name = $"Hair_{style}_{length}" };

            if (style == HairStyle.Bald)
            {
                return new Mesh { name = "Hair_Bald" };
            }

            float lengthScale = GetLengthScale(length);
            float volumeScale = config.Volume;

            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            int segments = Mathf.Max(16, 16 + config.Layers * 4);
            int rings = Mathf.Max(4, config.Layers * 2);

            for (int layer = 0; layer < config.Layers; layer++)
            {
                float layerHeight = (float)layer / Mathf.Max(1, config.Layers - 1) * config.Length * lengthScale;
                float layerRadius = 0.1f + layer * 0.02f * volumeScale * 5f;

                if (style == HairStyle.Afro)
                {
                    layerRadius = 0.12f + layer * 0.03f * volumeScale * 5f;
                }

                if (style == HairStyle.Ponytail && layer > config.Layers * 0.5f)
                {
                    layerRadius *= 0.3f;
                }

                int layerSegments = segments;
                int layerRings = Mathf.Max(2, rings - layer);

                for (int i = 0; i <= layerRings; i++)
                {
                    float v = (float)i / layerRings;
                    float y = layerHeight - v * (config.Length / config.Layers) * lengthScale;
                    float r = layerRadius * (1f - v * 0.3f);

                    if (config.IsCurly)
                    {
                        r *= 1f + Mathf.Sin(v * Mathf.PI * 4f) * 0.1f;
                    }

                    for (int j = 0; j < layerSegments; j++)
                    {
                        float u = (float)j / layerSegments;
                        float angle = u * Mathf.PI * 2f;
                        float x = Mathf.Cos(angle) * r;
                        float z = Mathf.Sin(angle) * r;

                        verts.Add(new Vector3(x, y, z));
                        norms.Add(new Vector3(x, y * 0.5f, z).normalized);
                        uvs.Add(new Vector2(u, v + layer * 0.2f));
                    }
                }
            }

            if (config.HasPonytail)
            {
                float ponytailY = config.Length * lengthScale * 0.3f;
                float ponytailRadius = 0.04f * volumeScale;

                for (int i = 0; i <= 6; i++)
                {
                    float v = (float)i / 6;
                    float y = ponytailY - v * config.Length * lengthScale * 0.6f;
                    float r = ponytailRadius * (1f - v * 0.5f);

                    for (int j = 0; j < 12; j++)
                    {
                        float u = (float)j / 12;
                        float angle = u * Mathf.PI * 2f;
                        verts.Add(new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
                        norms.Add(new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)));
                        uvs.Add(new Vector2(u, 0.9f + v * 0.1f));
                    }
                }
            }

            if (config.HasBun)
            {
                float bunY = config.Length * lengthScale * 0.15f;
                float bunRadius = 0.08f * volumeScale;

                for (int i = 0; i <= 4; i++)
                {
                    float v = (float)i / 4;
                    float y = bunY + v * 0.05f;
                    float r = bunRadius * (1f - v * 0.3f);

                    for (int j = 0; j < 16; j++)
                    {
                        float u = (float)j / 16;
                        float angle = u * Mathf.PI * 2f;
                        verts.Add(new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
                        norms.Add(new Vector3(Mathf.Cos(angle), Mathf.Sin(v * Mathf.PI), Mathf.Sin(angle)).normalized);
                        uvs.Add(new Vector2(u, 0.95f));
                    }
                }
            }

            int totalSegments = segments;
            int totalRings = rings * config.Layers;

            for (int layer = 0; layer < config.Layers; layer++)
            {
                int layerBase = layer * (rings + 1) * segments;
                int layerVerts = (rings + 1) * segments;

                for (int i = 0; i < rings; i++)
                {
                    for (int j = 0; j < segments; j++)
                    {
                        int a = layerBase + i * segments + j;
                        int b = layerBase + i * segments + (j + 1) % segments;
                        int c = layerBase + (i + 1) * segments + j;
                        int d = layerBase + (i + 1) * segments + (j + 1) % segments;

                        tris.Add(a); tris.Add(c); tris.Add(b);
                        tris.Add(b); tris.Add(c); tris.Add(d);
                    }
                }

                if (layer < config.Layers - 1)
                {
                    int nextBase = (layer + 1) * (rings + 1) * segments;
                    for (int j = 0; j < segments; j++)
                    {
                        int a = layerBase + rings * segments + j;
                        int b = layerBase + rings * segments + (j + 1) % segments;
                        int c = nextBase + j;
                        int d = nextBase + (j + 1) % segments;

                        tris.Add(a); tris.Add(c); tris.Add(b);
                        tris.Add(b); tris.Add(c); tris.Add(d);
                    }
                }
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }

        private static AttachmentPoint[] GenerateAttachments(HairStyle style)
        {
            var attachments = new List<AttachmentPoint>();

            attachments.Add(new AttachmentPoint
            {
                Name = "Front",
                Landmark = LandmarkId.Forehead,
                LocalOffset = new Vector3(0, 0.02f, 0.05f),
                LocalRotation = Quaternion.identity,
                InfluenceRadius = 0.1f
            });

            attachments.Add(new AttachmentPoint
            {
                Name = "LeftSide",
                Landmark = LandmarkId.LeftTemple,
                LocalOffset = new Vector3(0.02f, 0, 0),
                LocalRotation = Quaternion.Euler(0, -90, 0),
                InfluenceRadius = 0.08f
            });

            attachments.Add(new AttachmentPoint
            {
                Name = "RightSide",
                Landmark = LandmarkId.RightTemple,
                LocalOffset = new Vector3(-0.02f, 0, 0),
                LocalRotation = Quaternion.Euler(0, 90, 0),
                InfluenceRadius = 0.08f
            });

            attachments.Add(new AttachmentPoint
            {
                Name = "Back",
                Landmark = LandmarkId.Nape,
                LocalOffset = new Vector3(0, 0.01f, -0.03f),
                LocalRotation = Quaternion.Euler(180, 0, 0),
                InfluenceRadius = 0.1f
            });

            attachments.Add(new AttachmentPoint
            {
                Name = "Top",
                Landmark = LandmarkId.HeadTop,
                LocalOffset = Vector3.zero,
                LocalRotation = Quaternion.identity,
                InfluenceRadius = 0.08f
            });

            if (style == HairStyle.Ponytail || style == HairStyle.Braids || style == HairStyle.Dreadlocks)
            {
                attachments.Add(new AttachmentPoint
                {
                    Name = "PonytailBase",
                    Landmark = LandmarkId.Nape,
                    LocalOffset = new Vector3(0, 0.05f, -0.02f),
                    LocalRotation = Quaternion.identity,
                    InfluenceRadius = 0.06f
                });
            }

            if (style == HairStyle.Bun)
            {
                attachments.Add(new AttachmentPoint
                {
                    Name = "BunCenter",
                    Landmark = LandmarkId.HeadTop,
                    LocalOffset = new Vector3(0, 0.05f, 0),
                    LocalRotation = Quaternion.identity,
                    InfluenceRadius = 0.08f
                });
            }

            return attachments.ToArray();
        }

        private static HairMorphTarget[] GenerateHairMorphs(HairStyle style, HairStyleConfig config)
        {
            return new[]
            {
                new HairMorphTarget
                {
                    Name = "Length",
                    VertexIndices = new int[0],
                    VertexDeltas = new Vector3[0]
                },
                new HairMorphTarget
                {
                    Name = "Volume",
                    VertexIndices = new int[0],
                    VertexDeltas = new Vector3[0]
                },
                new HairMorphTarget
                {
                    Name = "Wind",
                    VertexIndices = new int[0],
                    VertexDeltas = new Vector3[0]
                }
            };
        }

        private static Material CreateHairMaterial(Color color)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Glossiness", 0.4f);
            mat.SetFloat("_Metallic", 0f);
            mat.EnableKeyword("_ALPHATEST_ON");
            mat.renderQueue = 2450;
            return mat;
        }

        private static float GetLengthScale(HairLength length)
        {
            switch (length)
            {
                case HairLength.Bald: return 0f;
                case HairLength.VeryShort: return 0.3f;
                case HairLength.Short: return 0.6f;
                case HairLength.Medium: return 1f;
                case HairLength.Long: return 1.5f;
                case HairLength.VeryLong: return 2f;
                default: return 1f;
            }
        }

        private class HairStyleConfig
        {
            public int BaseVertices;
            public int Layers;
            public float Volume;
            public float Length = 0.2f;
            public bool IsCurly;
            public bool HasPonytail;
            public bool HasBun;
            public bool HasUndercut;
            public bool HasBraids;
            public bool HasDreadlocks;
        }
    }
}