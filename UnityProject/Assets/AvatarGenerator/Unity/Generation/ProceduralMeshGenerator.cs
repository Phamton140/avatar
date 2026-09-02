using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AvatarGenerator.Unity.Generation
{
    public static class ProceduralMeshGenerator
    {
        public static Mesh GenerateBodyMesh(SkeletonDefinition skeleton, RegionDeformResult[] regions)
        {
            var mesh = new Mesh { name = "ProceduralBody" };
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var boneWeights = new List<BoneWeight>();
            var bindPoses = new List<Matrix4x4>();
            var triangles = new List<int>();

            var nameToIndex = new Dictionary<string, int>();
            for (int i = 0; i < skeleton.Bones.Length; i++)
                nameToIndex[skeleton.Bones[i].Name] = i;

            int vertexOffset = 0;

            foreach (var region in regions)
            {
                if (region.RootTransform == Matrix4x4.identity) continue;

                int boneIdx = FindBoneForRegion(skeleton, region);
                if (boneIdx < 0) continue;

                var bone = skeleton.Bones[boneIdx];
                var scale = region.Scale;

                var primitive = CreatePrimitiveForBone(bone, scale);
                int startVert = vertices.Count;

                var worldMatrix = region.RootTransform;
                var invWorld = worldMatrix.inverse;

                for (int i = 0; i < primitive.vertices.Length; i++)
                {
                    var v = primitive.vertices[i];
                    var n = primitive.normals[i];
                    var uv = primitive.uv.Length > i ? primitive.uv[i] : Vector2.zero;

                    vertices.Add(worldMatrix.MultiplyPoint(v));
                    normals.Add(worldMatrix.MultiplyVector(n).normalized);
                    uvs.Add(uv);

                    var bw = new BoneWeight
                    {
                        boneIndex0 = boneIdx,
                        weight0 = 1f
                    };
                    boneWeights.Add(bw);
                }

                foreach (var t in primitive.triangles)
                {
                    triangles.Add(t + startVert);
                }

                vertexOffset += primitive.vertexCount;
            }

            if (vertices.Count == 0)
            {
                return CreateFallbackMesh();
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.boneWeights = boneWeights.ToArray();
            mesh.bindposes = skeleton.BindPoses;
            mesh.RecalculateBounds();

            return mesh;
        }

        private static int FindBoneForRegion(SkeletonDefinition skeleton, RegionDeformResult region)
        {
            for (int i = 0; i < skeleton.Bones.Length; i++)
            {
                var boneWorld = skeleton.BindPoses[i];
                if (Vector3.Distance(boneWorld.GetColumn(3), region.RootTransform.GetColumn(3)) < 0.02f)
                    return i;
            }
            return -1;
        }

        private static Mesh CreatePrimitiveForBone(BoneDefinition bone, Vector3 scale)
        {
            float length = bone.LocalPosition.magnitude * scale.x;
            float radius = Mathf.Max(scale.y, scale.z) * 0.1f;

            if (length < 0.01f || bone.Name == "Head")
            {
                return CreateSphere(radius * 2.5f);
            }

            return CreateCapsule(length, radius);
        }

        private static Mesh CreateCapsule(float height, float radius)
        {
            var mesh = new Mesh();
            int rings = 8;
            int segments = 12;
            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            for (int i = 0; i <= rings; i++)
            {
                float v = (float)i / rings;
                float y = Mathf.Lerp(-height * 0.5f, height * 0.5f, v);
                float r = radius;

                if (v < 0.5f)
                {
                    float t = v * 2f;
                    r = Mathf.Sqrt(Mathf.Max(0.001f, radius * radius - (y + height * 0.5f) * (y + height * 0.5f)));
                }
                else
                {
                    float t = (v - 0.5f) * 2f;
                    r = Mathf.Sqrt(Mathf.Max(0.001f, radius * radius - (y - height * 0.5f) * (y - height * 0.5f)));
                }

                for (int j = 0; j < segments; j++)
                {
                    float u = (float)j / segments;
                    float angle = u * Mathf.PI * 2f;
                    float x = Mathf.Cos(angle) * r;
                    float z = Mathf.Sin(angle) * r;

                    verts.Add(new Vector3(x, y, z));
                    norms.Add(new Vector3(x, 0, z).normalized);
                    uvs.Add(new Vector2(u, v));
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
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateSphere(float radius)
        {
            var mesh = new Mesh();
            int rings = 8;
            int segments = 12;
            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var uvs = new List<Vector2>();
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
                    float x = Mathf.Cos(theta) * r;
                    float z = Mathf.Sin(theta) * r;

                    verts.Add(new Vector3(x, y, z));
                    norms.Add(new Vector3(x, y, z).normalized);
                    uvs.Add(new Vector2(u, v));
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
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateFallbackMesh()
        {
            return CreateSphere(0.5f);
        }
    }
}