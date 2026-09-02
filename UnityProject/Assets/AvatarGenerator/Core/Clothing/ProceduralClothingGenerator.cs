using System.Collections.Generic;
using AvatarGenerator.Core.Clothing;
using AvatarGenerator.Core.Skeleton;
using AvatarGenerator.Core.Regions;
using UnityEngine;

namespace AvatarGenerator.Core.Clothing
{
    public static class ProceduralClothingGenerator
    {
        public static ClothingAsset GenerateShirt(string id, string displayName, Color color)
        {
            var mesh = CreateShirtMesh();
            var capsules = GenerateShirtCapsules();

            return new ClothingAsset
            {
                Id = id,
                DisplayName = displayName,
                Slot = ClothingSlot.Shirt,
                Type = ClothingType.UpperBody,
                BaseMesh = mesh,
                Capsules = capsules,
                Material = CreateClothingMaterial(color),
                CompatibleBodyTypes = new[] { "male", "female", "neutral" }
            };
        }

        public static ClothingAsset GeneratePants(string id, string displayName, Color color)
        {
            var mesh = CreatePantsMesh();
            var capsules = GeneratePantsCapsules();

            return new ClothingAsset
            {
                Id = id,
                DisplayName = displayName,
                Slot = ClothingSlot.Pants,
                Type = ClothingType.LowerBody,
                BaseMesh = mesh,
                Capsules = capsules,
                Material = CreateClothingMaterial(color),
                CompatibleBodyTypes = new[] { "male", "female", "neutral" }
            };
        }

        public static ClothingAsset GenerateShoes(string id, string displayName, Color color)
        {
            var mesh = CreateShoesMesh();
            var capsules = GenerateShoesCapsules();

            return new ClothingAsset
            {
                Id = id,
                DisplayName = displayName,
                Slot = ClothingSlot.Shoes,
                Type = ClothingType.Feet,
                BaseMesh = mesh,
                Capsules = capsules,
                Material = CreateClothingMaterial(color),
                CompatibleBodyTypes = new[] { "male", "female", "neutral" }
            };
        }

        public static ClothingAsset GenerateJacket(string id, string displayName, Color color)
        {
            var mesh = CreateJacketMesh();
            var capsules = GenerateJacketCapsules();

            return new ClothingAsset
            {
                Id = id,
                DisplayName = displayName,
                Slot = ClothingSlot.Jacket,
                Type = ClothingType.UpperBody,
                BaseMesh = mesh,
                Capsules = capsules,
                Material = CreateClothingMaterial(color),
                CompatibleBodyTypes = new[] { "male", "female", "neutral" }
            };
        }

        private static Mesh CreateShirtMesh()
        {
            var mesh = new Mesh { name = "ProceduralShirt" };
            float torsoHeight = 0.5f;
            float chestRadius = 0.22f;
            float waistRadius = 0.18f;
            float shoulderWidth = 0.45f;
            float sleeveLength = 0.55f;
            float sleeveRadius = 0.07f;

            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            int rings = 12;
            int segments = 16;

            for (int i = 0; i <= rings; i++)
            {
                float v = (float)i / rings;
                float y = Mathf.Lerp(-torsoHeight * 0.5f, torsoHeight * 0.5f, v);
                float r = Mathf.Lerp(waistRadius, chestRadius, v);

                if (v > 0.7f)
                {
                    float t = (v - 0.7f) / 0.3f;
                    r = Mathf.Lerp(chestRadius, shoulderWidth * 0.5f, t);
                }

                for (int j = 0; j < segments; j++)
                {
                    float u = (float)j / segments;
                    float angle = u * Mathf.PI * 2f;
                    float x = Mathf.Cos(angle) * r;
                    float z = Mathf.Sin(angle) * r;

                    verts.Add(new Vector3(x, y + torsoHeight * 0.5f, z));
                    norms.Add(new Vector3(x, 0, z).normalized);
                    uvs.Add(new Vector2(u, v));
                }
            }

            int neckStart = verts.Count;
            for (int j = 0; j < segments; j++)
            {
                float u = (float)j / segments;
                float angle = u * Mathf.PI * 2f;
                float r = 0.08f;
                verts.Add(new Vector3(Mathf.Cos(angle) * r, torsoHeight * 0.5f + 0.05f, Mathf.Sin(angle) * r));
                norms.Add(Vector3.up);
                uvs.Add(new Vector2(u, 1f));
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

            for (int j = 0; j < segments; j++)
            {
                int a = rings * segments + j;
                int b = rings * segments + (j + 1) % segments;
                int c = neckStart + j;
                int d = neckStart + (j + 1) % segments;

                tris.Add(a); tris.Add(c); tris.Add(b);
                tris.Add(b); tris.Add(c); tris.Add(d);
            }

            int sleeveStart = verts.Count;
            for (int side = -1; side <= 1; side += 2)
            {
                float shoulderY = torsoHeight * 0.5f - 0.05f;
                float shoulderX = side * shoulderWidth * 0.5f;

                for (int i = 0; i <= 8; i++)
                {
                    float v = (float)i / 8;
                    float y = shoulderY - v * sleeveLength;
                    float r = Mathf.Lerp(sleeveRadius, sleeveRadius * 0.6f, v);

                    for (int j = 0; j < 10; j++)
                    {
                        float u = (float)j / 10;
                        float angle = u * Mathf.PI * 2f;
                        verts.Add(new Vector3(shoulderX + Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
                        norms.Add(new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)));
                        uvs.Add(new Vector2(u, v));
                    }
                }
            }

            for (int s = 0; s < 2; s++)
            {
                int baseIdx = sleeveStart + s * 9 * 10;
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        int a = baseIdx + i * 10 + j;
                        int b = baseIdx + i * 10 + (j + 1) % 10;
                        int c = baseIdx + (i + 1) * 10 + j;
                        int d = baseIdx + (i + 1) * 10 + (j + 1) % 10;

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
            mesh.RecalculateTangents();

            return mesh;
        }

        private static Mesh CreatePantsMesh()
        {
            var mesh = new Mesh { name = "ProceduralPants" };
            float legLength = 0.85f;
            float hipRadius = 0.2f;
            float kneeRadius = 0.1f;
            float ankleRadius = 0.08f;
            float waistHeight = 0.15f;

            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            int segments = 16;
            int legRings = 14;

            for (int leg = -1; leg <= 1; leg += 2)
            {
                float hipX = leg * hipRadius * 0.8f;
                int legBase = verts.Count;

                for (int i = 0; i <= legRings; i++)
                {
                    float v = (float)i / legRings;
                    float y = waistHeight - v * legLength;
                    float r = Mathf.Lerp(hipRadius, ankleRadius, v);

                    if (v > 0.45f && v < 0.55f)
                    {
                        r = kneeRadius;
                    }

                    for (int j = 0; j < segments; j++)
                    {
                        float u = (float)j / segments;
                        float angle = u * Mathf.PI * 2f;
                        verts.Add(new Vector3(hipX + Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
                        norms.Add(new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)));
                        uvs.Add(new Vector2(u, v));
                    }
                }

                for (int i = 0; i < legRings; i++)
                {
                    for (int j = 0; j < segments; j++)
                    {
                        int a = legBase + i * segments + j;
                        int b = legBase + i * segments + (j + 1) % segments;
                        int c = legBase + (i + 1) * segments + j;
                        int d = legBase + (i + 1) * segments + (j + 1) % segments;

                        tris.Add(a); tris.Add(c); tris.Add(b);
                        tris.Add(b); tris.Add(c); tris.Add(d);
                    }
                }
            }

            int waistRings = 4;
            int waistStart = verts.Count;
            for (int i = 0; i <= waistRings; i++)
            {
                float v = (float)i / waistRings;
                float y = waistHeight + v * 0.05f;
                float r = hipRadius * 1.1f;

                for (int j = 0; j < segments; j++)
                {
                    float u = (float)j / segments;
                    float angle = u * Mathf.PI * 2f;
                    verts.Add(new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
                    norms.Add(Vector3.up);
                    uvs.Add(new Vector2(u, 1f + v * 0.1f));
                }
            }

            for (int i = 0; i < waistRings; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    int a = waistStart + i * segments + j;
                    int b = waistStart + i * segments + (j + 1) % segments;
                    int c = waistStart + (i + 1) * segments + j;
                    int d = waistStart + (i + 1) * segments + (j + 1) % segments;

                    tris.Add(a); tris.Add(c); tris.Add(b);
                    tris.Add(b); tris.Add(c); tris.Add(d);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mesh;
        }

        private static Mesh CreateShoesMesh()
        {
            var mesh = new Mesh { name = "ProceduralShoes" };
            float footLength = 0.25f;
            float footWidth = 0.1f;
            float footHeight = 0.08f;

            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            for (int leg = -1; leg <= 1; leg += 2)
            {
                float xOffset = leg * 0.12f;
                int baseIdx = verts.Count;

                verts.Add(new Vector3(xOffset - footWidth * 0.5f, 0, -footLength * 0.5f));
                verts.Add(new Vector3(xOffset + footWidth * 0.5f, 0, -footLength * 0.5f));
                verts.Add(new Vector3(xOffset + footWidth * 0.5f, 0, footLength * 0.5f));
                verts.Add(new Vector3(xOffset - footWidth * 0.5f, 0, footLength * 0.5f));

                verts.Add(new Vector3(xOffset - footWidth * 0.5f, footHeight, -footLength * 0.5f));
                verts.Add(new Vector3(xOffset + footWidth * 0.5f, footHeight, -footLength * 0.5f));
                verts.Add(new Vector3(xOffset + footWidth * 0.5f, footHeight, footLength * 0.5f));
                verts.Add(new Vector3(xOffset - footWidth * 0.5f, footHeight, footLength * 0.5f));

                norms.Add(Vector3.down); norms.Add(Vector3.down); norms.Add(Vector3.down); norms.Add(Vector3.down);
                norms.Add(Vector3.up); norms.Add(Vector3.up); norms.Add(Vector3.up); norms.Add(Vector3.up);

                uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(1, 1)); uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(1, 1)); uvs.Add(new Vector2(0, 1));

                int b = baseIdx;
                tris.Add(b); tris.Add(b + 2); tris.Add(b + 1);
                tris.Add(b); tris.Add(b + 3); tris.Add(b + 2);

                tris.Add(b + 4); tris.Add(b + 5); tris.Add(b + 6);
                tris.Add(b + 4); tris.Add(b + 6); tris.Add(b + 7);

                for (int i = 0; i < 4; i++)
                {
                    int a = b + i;
                    int c = b + (i + 1) % 4;
                    int d = b + 4 + i;
                    int e = b + 4 + (i + 1) % 4;

                    tris.Add(a); tris.Add(d); tris.Add(c);
                    tris.Add(c); tris.Add(d); tris.Add(e);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mesh;
        }

        private static Mesh CreateJacketMesh()
        {
            var shirt = CreateShirtMesh();
            var mesh = new Mesh { name = "ProceduralJacket" };

            var verts = new List<Vector3>(shirt.vertices);
            var norms = new List<Vector3>(shirt.normals);
            var uvs = new List<Vector2>(shirt.uv);
            var tris = new List<int>(shirt.triangles);

            for (int i = 0; i < verts.Count; i++)
            {
                var v = verts[i];
                var n = norms[i];
                verts[i] = v + n * 0.02f;
            }

            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mesh;
        }

        private static ClothingCapsule[] GenerateShirtCapsules()
        {
            return new[]
            {
                new ClothingCapsule { BoneName = "Spine1", LocalCenter = new Vector3(0, 0.15f, 0), Radius = 0.2f, Height = 0.25f, Direction = CapsuleDirection.AlongBone },
                new ClothingCapsule { BoneName = "Spine2", LocalCenter = new Vector3(0, 0.1f, 0), Radius = 0.22f, Height = 0.2f, Direction = CapsuleDirection.AlongBone },
                new ClothingCapsule { BoneName = "Spine3", LocalCenter = new Vector3(0, 0.05f, 0.1f), Radius = 0.25f, Height = 0.2f, Direction = CapsuleDirection.AlongBone },
                new ClothingCapsule { BoneName = "LeftClavicle", LocalCenter = new Vector3(-0.2f, 0, 0), Radius = 0.08f, Height = 0.3f, Direction = CapsuleDirection.AlongBone },
                new ClothingCapsule { BoneName = "RightClavicle", LocalCenter = new Vector3(0.2f, 0, 0), Radius = 0.08f, Height = 0.3f, Direction = CapsuleDirection.AlongBone },
                new ClothingCapsule { BoneName = "LeftUpperArm", LocalCenter = Vector3.zero, Radius = 0.09f, Height = 0.35f, Direction = CapsuleDirection.AlongBone },
                new ClothingCapsule { BoneName = "RightUpperArm", LocalCenter = Vector3.zero, Radius = 0.09f, Height = 0.35f, Direction = CapsuleDirection.AlongBone },
                new ClothingCapsule { BoneName = "LeftForearm", LocalCenter = Vector3.zero, Radius = 0.08f, Height = 0.28f, Direction = CapsuleDirection.AlongBone },
                new ClothingCapsule { BoneName = "RightForearm", LocalCenter = Vector3.zero, Radius = 0.08f, Height = 0.28f, Direction = CapsuleDirection.AlongBone }
            };
        }

        private static ClothingCapsule[] GeneratePantsCapsules()
        {
            return new[]
            {
                new ClothingCapsule { BoneName = "Hips", LocalCenter = new Vector3(0, 0.05f, 0), Radius = 0.22f, Height = 0.15f, Direction = CapsuleDirection.Perpendicular },
                new ClothingCapsule { BoneName = "LeftThigh", LocalCenter = Vector3.zero, Radius = 0.18f, Height = 0.43f, Direction = CapsuleDirection.AlongBone },
                new ClothingCapsule { BoneName = "RightThigh", LocalCenter = Vector3.zero, Radius = 0.18f, Height = 0.43f, Direction = CapsuleDirection.AlongBone },
                new ClothingCapsule { BoneName = "LeftCalf", LocalCenter = Vector3.zero, Radius = 0.12f, Height = 0.39f, Direction = CapsuleDirection.AlongBone },
                new ClothingCapsule { BoneName = "RightCalf", LocalCenter = Vector3.zero, Radius = 0.12f, Height = 0.39f, Direction = CapsuleDirection.AlongBone }
            };
        }

        private static ClothingCapsule[] GenerateShoesCapsules()
        {
            return new[]
            {
                new ClothingCapsule { BoneName = "LeftFoot", LocalCenter = new Vector3(0, 0.02f, 0.05f), Radius = 0.1f, Height = 0.08f, Direction = CapsuleDirection.Perpendicular },
                new ClothingCapsule { BoneName = "RightFoot", LocalCenter = new Vector3(0, 0.02f, 0.05f), Radius = 0.1f, Height = 0.08f, Direction = CapsuleDirection.Perpendicular }
            };
        }

        private static ClothingCapsule[] GenerateJacketCapsules()
        {
            var shirtCaps = GenerateShirtCapsules();
            var list = new List<ClothingCapsule>(shirtCaps);

            list.Add(new ClothingCapsule { BoneName = "LeftForearm", LocalCenter = Vector3.zero, Radius = 0.1f, Height = 0.28f, Direction = CapsuleDirection.AlongBone });
            list.Add(new ClothingCapsule { BoneName = "RightForearm", LocalCenter = Vector3.zero, Radius = 0.1f, Height = 0.28f, Direction = CapsuleDirection.AlongBone });

            return list.ToArray();
        }

        private static Material CreateClothingMaterial(Color color)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Glossiness", 0.3f);
            mat.SetFloat("_Metallic", 0f);
            return mat;
        }
    }
}