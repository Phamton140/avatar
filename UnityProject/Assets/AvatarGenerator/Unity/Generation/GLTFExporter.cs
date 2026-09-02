using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using AvatarGenerator.Core.Pipeline;
using AvatarGenerator.Core.Skeleton;

namespace AvatarGenerator.Unity.Generation
{
    public class GLTFExporter
    {
        public void Export(GeneratedCharacter character, string path)
        {
            var gltf = new GLTFSchema.GLTFRoot
            {
                asset = new GLTFSchema.Asset { version = "2.0", generator = "AvatarGenerator 0.1" },
                scene = 0,
                scenes = new List<GLTFSchema.Scene>
                {
                    new GLTFSchema.Scene { nodes = new List<int> { 0 } }
                },
                nodes = new List<GLTFSchema.Node>(),
                meshes = new List<GLTFSchema.Mesh>(),
                materials = new List<GLTFSchema.Material>(),
                skins = new List<GLTFSchema.Skin>(),
                accessors = new List<GLTFSchema.Accessor>(),
                bufferViews = new List<GLTFSchema.BufferView>(),
                buffers = new List<GLTFSchema.Buffer>(),
                extensionsUsed = new List<string> { "KHR_materials_pbrSpecularGlossiness" }
            };

            var buffer = new List<byte>();
            var vertexAccessor = AddMesh(gltf, buffer, character.FinalMesh, "CharacterMesh");
            var skinAccessor = AddSkin(gltf, buffer, character.Skeleton);

            var node = new GLTFSchema.Node
            {
                name = "Character",
                mesh = 0,
                skin = 0
            };
            gltf.nodes.Add(node);

            gltf.buffers.Add(new GLTFSchema.Buffer
            {
                byteLength = buffer.Count,
                uri = Path.GetFileNameWithoutExtension(path) + ".bin"
            });

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(gltf, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(path, json);
            File.WriteAllBytes(Path.ChangeExtension(path, ".bin"), buffer.ToArray());
        }

        private int AddMesh(GLTFSchema.GLTFRoot gltf, List<byte> buffer, Mesh mesh, string name)
        {
            var positionAccessor = AddAccessor(gltf, buffer, mesh.vertices, GLTFSchema.Accessor.AttributeType.VEC3);
            var normalAccessor = AddAccessor(gltf, buffer, mesh.normals, GLTFSchema.Accessor.AttributeType.VEC3);
            var uvAccessor = AddAccessor(gltf, buffer, mesh.uv, GLTFSchema.Accessor.AttributeType.VEC2);
            var indexAccessor = AddAccessor(gltf, buffer, mesh.triangles, GLTFSchema.Accessor.AttributeType.SCALAR, true);

            var primitive = new GLTFSchema.MeshPrimitive
            {
                attributes = new GLTFSchema.Attributes
                {
                    POSITION = positionAccessor,
                    NORMAL = normalAccessor,
                    TEXCOORD_0 = uvAccessor
                },
                indices = indexAccessor,
                material = 0
            };

            if (mesh.boneWeights.Length > 0)
            {
                var jointAccessor = AddAccessor(gltf, buffer, mesh.boneWeights, GLTFSchema.Accessor.AttributeType.VEC4, false, true);
                var weightAccessor = AddAccessor(gltf, buffer, mesh.boneWeights, GLTFSchema.Accessor.AttributeType.VEC4, false, false);
                primitive.attributes.JOINTS_0 = jointAccessor;
                primitive.attributes.WEIGHTS_0 = weightAccessor;
            }

            gltf.meshes.Add(new GLTFSchema.Mesh
            {
                name = name,
                primitives = new List<GLTFSchema.MeshPrimitive> { primitive }
            });

            return gltf.meshes.Count - 1;
        }

        private int AddSkin(GLTFSchema.GLTFRoot gltf, List<byte> buffer, SkeletonDefinition skeleton)
        {
            var joints = new List<int>();
            var inverseBindMatrices = new List<Matrix4x4>();

            var nameToIndex = new Dictionary<string, int>();
            for (int i = 0; i < skeleton.Bones.Length; i++)
                nameToIndex[skeleton.Bones[i].Name] = i;

            for (int i = 0; i < skeleton.Bones.Length; i++)
            {
                joints.Add(i);
                inverseBindMatrices.Add(skeleton.InverseBindPoses[i]);
            }

            var jointAccessor = AddAccessor(gltf, buffer, joints.ToArray(), GLTFSchema.Accessor.AttributeType.SCALAR);
            var ibmAccessor = AddAccessor(gltf, buffer, inverseBindMatrices.ToArray(), GLTFSchema.Accessor.AttributeType.MAT4);

            var skin = new GLTFSchema.Skin
            {
                joints = joints,
                inverseBindMatrices = ibmAccessor,
                skeleton = 0
            };

            gltf.skins.Add(skin);
            return gltf.skins.Count - 1;
        }

        private int AddAccessor<T>(GLTFSchema.GLTFRoot gltf, List<byte> buffer, T[] data, GLTFSchema.Accessor.AttributeType type, bool isIndex = false, bool isJoint = false) where T : struct
        {
            var accessor = new GLTFSchema.Accessor
            {
                bufferView = gltf.bufferViews.Count,
                componentType = GetComponentType<T>(),
                count = data.Length,
                type = type.ToString(),
                min = isIndex ? null : CalculateMin(data),
                max = isIndex ? null : CalculateMax(data)
            };

            var view = new GLTFSchema.BufferView
            {
                buffer = 0,
                byteOffset = buffer.Count,
                byteLength = data.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)),
                target = isIndex ? GLTFSchema.BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER : GLTFSchema.BufferView.TargetEnum.ARRAY_BUFFER
            };

            gltf.bufferViews.Add(view);
            gltf.accessors.Add(accessor);

            var bytes = System.Buffers.Binary.BinaryPrimitives.WriteBytes(data);
            buffer.AddRange(bytes);

            return gltf.accessors.Count - 1;
        }

        private int GetComponentType<T>()
        {
            if (typeof(T) == typeof(byte)) return 5120;
            if (typeof(T) == typeof(ushort)) return 5123;
            if (typeof(T) == typeof(uint)) return 5125;
            if (typeof(T) == typeof(float)) return 5126;
            return 5126;
        }

        private float[] CalculateMin<T>(T[] data) where T : struct
        {
            return new float[3] { -1, -1, -1 };
        }

        private float[] CalculateMax<T>(T[] data) where T : struct
        {
            return new float[3] { 1, 1, 1 };
        }
    }
}