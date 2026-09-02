using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace AvatarGenerator.Unity.Import
{
    public enum AssetImportType
    {
        BaseMesh,
        MorphTargets,
        Clothing,
        Hair,
        Rig
    }

    [System.Serializable]
    public struct BlenderAssetMetadata
    {
        public string AssetId;
        public string DisplayName;
        public AssetImportType Type;
        public string BlenderVersion;
        public string ExportDate;
        public string[] MorphTargetNames;
        public Dictionary<string, string> ParameterMapping;
        public Vector3 Scale;
        public Vector3 Rotation;
        public Vector3 Position;
    }

    [System.Serializable]
    public struct ImportedMorphTarget
    {
        public string Name;
        public int[] VertexIndices;
        public Vector3[] VertexDeltas;
        public float MinWeight;
        public float MaxWeight;
    }

    public class BlenderImportPipeline
    {
        private readonly string _importRootPath;

        public BlenderImportPipeline(string importRootPath)
        {
            _importRootPath = importRootPath;
        }

        public BlenderImportResult ImportBaseMesh(string fbxPath, string assetId)
        {
            var result = new BlenderImportResult { Success = false };

            var mesh = LoadMeshFromFBX(fbxPath);
            if (mesh == null)
            {
                result.ErrorMessage = "Failed to load mesh from FBX";
                return result;
            }

            var morphs = LoadMorphTargetsFromFBX(fbxPath);
            var metadata = LoadMetadataFromJSON(Path.ChangeExtension(fbxPath, ".json"));

            var savePath = Path.Combine(_importRootPath, "BaseMeshes", assetId + ".asset");
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));

            var asset = ScriptableObject.CreateInstance<BaseMeshAsset>();
            asset.AssetId = assetId;
            asset.DisplayName = metadata.DisplayName;
            asset.BaseMesh = mesh;
            asset.MorphTargets = morphs;
            asset.ParameterMapping = metadata.ParameterMapping;
            asset.Scale = metadata.Scale;
            asset.Rotation = metadata.Rotation;
            asset.Position = metadata.Position;

            AssetDatabase.CreateAsset(asset, savePath);
            AssetDatabase.SaveAssets();

            result.Success = true;
            result.AssetPath = savePath;
            result.Asset = asset;

            return result;
        }

        public BlenderImportResult ImportClothing(string fbxPath, string assetId, ClothingSlot slot)
        {
            var result = new BlenderImportResult { Success = false };

            var mesh = LoadMeshFromFBX(fbxPath);
            if (mesh == null)
            {
                result.ErrorMessage = "Failed to load mesh from FBX";
                return result;
            }

            var metadata = LoadMetadataFromJSON(Path.ChangeExtension(fbxPath, ".json"));

            var capsules = GenerateCapsulesFromMesh(mesh, slot);
            var material = LoadMaterialFromFBX(fbxPath);

            var savePath = Path.Combine(_importRootPath, "Clothing", assetId + ".asset");
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));

            var asset = ScriptableObject.CreateInstance<ClothingAssetWrapper>();
            asset.AssetId = assetId;
            asset.DisplayName = metadata.DisplayName;
            asset.Slot = slot;
            asset.BaseMesh = mesh;
            asset.Capsules = capsules;
            asset.Material = material;
            asset.ParameterMapping = metadata.ParameterMapping;

            AssetDatabase.CreateAsset(asset, savePath);
            AssetDatabase.SaveAssets();

            result.Success = true;
            result.AssetPath = savePath;
            result.Asset = asset;

            return result;
        }

        public BlenderImportResult ImportHair(string fbxPath, string assetId)
        {
            var result = new BlenderImportResult { Success = false };

            var mesh = LoadMeshFromFBX(fbxPath);
            if (mesh == null)
            {
                result.ErrorMessage = "Failed to load mesh from FBX";
                return result;
            }

            var metadata = LoadMetadataFromJSON(Path.ChangeExtension(fbxPath, ".json"));

            var attachments = GenerateAttachmentsFromMesh(mesh);
            var material = LoadMaterialFromFBX(fbxPath);

            var savePath = Path.Combine(_importRootPath, "Hair", assetId + ".asset");
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));

            var asset = ScriptableObject.CreateInstance<HairAssetWrapper>();
            asset.AssetId = assetId;
            asset.DisplayName = metadata.DisplayName;
            asset.BaseMesh = mesh;
            asset.Attachments = attachments;
            asset.Material = material;
            asset.ParameterMapping = metadata.ParameterMapping;

            AssetDatabase.CreateAsset(asset, savePath);
            AssetDatabase.SaveAssets();

            result.Success = true;
            result.AssetPath = savePath;
            result.Asset = asset;

            return result;
        }

        private Mesh LoadMeshFromFBX(string fbxPath)
        {
            var objects = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            foreach (var obj in objects)
            {
                if (obj is Mesh mesh)
                    return mesh;
            }
            return null;
        }

        private ImportedMorphTarget[] LoadMorphTargetsFromFBX(string fbxPath)
        {
            var morphs = new List<ImportedMorphTarget>();

            var objects = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            foreach (var obj in objects)
            {
                if (obj is Mesh mesh && mesh.blendShapeCount > 0)
                {
                    for (int i = 0; i < mesh.blendShapeCount; i++)
                    {
                        string name = mesh.GetBlendShapeName(i);
                        int vertexCount = mesh.vertexCount;

                        var indices = new int[vertexCount];
                        var deltas = new Vector3[vertexCount];

                        for (int v = 0; v < vertexCount; v++)
                        {
                            indices[v] = v;
                            mesh.GetBlendShapeFrameVertices(i, 0, deltas, null, null);
                        }

                        morphs.Add(new ImportedMorphTarget
                        {
                            Name = name,
                            VertexIndices = indices,
                            VertexDeltas = deltas,
                            MinWeight = 0f,
                            MaxWeight = 1f
                        });
                    }
                    break;
                }
            }

            return morphs.ToArray();
        }

        private BlenderAssetMetadata LoadMetadataFromJSON(string jsonPath)
        {
            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                return JsonUtility.FromJson<BlenderAssetMetadata>(json);
            }

            return new BlenderAssetMetadata
            {
                DisplayName = "Imported Asset",
                ParameterMapping = new Dictionary<string, string>()
            };
        }

        private ClothingCapsule[] GenerateCapsulesFromMesh(Mesh mesh, ClothingSlot slot)
        {
            var capsules = new List<ClothingCapsule>();
            var bounds = mesh.bounds;

            switch (slot)
            {
                case ClothingSlot.Shirt:
                    capsules.Add(new ClothingCapsule { BoneName = "Spine1", LocalCenter = new Vector3(0, bounds.center.y * 0.5f, 0), Radius = bounds.extents.x * 0.9f, Height = bounds.size.y * 0.6f, Direction = CapsuleDirection.AlongBone });
                    capsules.Add(new ClothingCapsule { BoneName = "Spine2", LocalCenter = Vector3.zero, Radius = bounds.extents.x, Height = bounds.size.y * 0.4f, Direction = CapsuleDirection.AlongBone });
                    capsules.Add(new ClothingCapsule { BoneName = "LeftUpperArm", LocalCenter = Vector3.zero, Radius = bounds.extents.x * 0.4f, Height = bounds.size.y * 0.3f, Direction = CapsuleDirection.AlongBone });
                    capsules.Add(new ClothingCapsule { BoneName = "RightUpperArm", LocalCenter = Vector3.zero, Radius = bounds.extents.x * 0.4f, Height = bounds.size.y * 0.3f, Direction = CapsuleDirection.AlongBone });
                    break;

                case ClothingSlot.Pants:
                    capsules.Add(new ClothingCapsule { BoneName = "Hips", LocalCenter = new Vector3(0, bounds.center.y * 0.2f, 0), Radius = bounds.extents.x * 0.9f, Height = bounds.size.y * 0.2f, Direction = CapsuleDirection.Perpendicular });
                    capsules.Add(new ClothingCapsule { BoneName = "LeftThigh", LocalCenter = Vector3.zero, Radius = bounds.extents.x * 0.8f, Height = bounds.size.y * 0.5f, Direction = CapsuleDirection.AlongBone });
                    capsules.Add(new ClothingCapsule { BoneName = "RightThigh", LocalCenter = Vector3.zero, Radius = bounds.extents.x * 0.8f, Height = bounds.size.y * 0.5f, Direction = CapsuleDirection.AlongBone });
                    capsules.Add(new ClothingCapsule { BoneName = "LeftCalf", LocalCenter = Vector3.zero, Radius = bounds.extents.x * 0.5f, Height = bounds.size.y * 0.4f, Direction = CapsuleDirection.AlongBone });
                    capsules.Add(new ClothingCapsule { BoneName = "RightCalf", LocalCenter = Vector3.zero, Radius = bounds.extents.x * 0.5f, Height = bounds.size.y * 0.4f, Direction = CapsuleDirection.AlongBone });
                    break;
            }

            return capsules.ToArray();
        }

        private AttachmentPoint[] GenerateAttachmentsFromMesh(Mesh mesh)
        {
            var bounds = mesh.bounds;
            return new[]
            {
                new AttachmentPoint { Name = "Front", Landmark = LandmarkId.Forehead, LocalOffset = new Vector3(0, bounds.extents.y * 0.5f, bounds.extents.z), LocalRotation = Quaternion.identity, InfluenceRadius = 0.1f },
                new AttachmentPoint { Name = "Left", Landmark = LandmarkId.LeftTemple, LocalOffset = new Vector3(bounds.extents.x, 0, 0), LocalRotation = Quaternion.Euler(0, -90, 0), InfluenceRadius = 0.08f },
                new AttachmentPoint { Name = "Right", Landmark = LandmarkId.RightTemple, LocalOffset = new Vector3(-bounds.extents.x, 0, 0), LocalRotation = Quaternion.Euler(0, 90, 0), InfluenceRadius = 0.08f },
                new AttachmentPoint { Name = "Back", Landmark = LandmarkId.Nape, LocalOffset = new Vector3(0, bounds.extents.y * 0.3f, -bounds.extents.z), LocalRotation = Quaternion.Euler(180, 0, 0), InfluenceRadius = 0.1f },
                new AttachmentPoint { Name = "Top", Landmark = LandmarkId.HeadTop, LocalOffset = Vector3.up * bounds.extents.y, LocalRotation = Quaternion.identity, InfluenceRadius = 0.08f }
            };
        }

        private Material LoadMaterialFromFBX(string fbxPath)
        {
            var objects = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            foreach (var obj in objects)
            {
                if (obj is Material mat)
                    return mat;
            }
            return null;
        }
    }

    [System.Serializable]
    public struct BlenderImportResult
    {
        public bool Success;
        public string ErrorMessage;
        public string AssetPath;
        public ScriptableObject Asset;
    }

    [System.Serializable]
    public class BaseMeshAsset : ScriptableObject
    {
        public string AssetId;
        public string DisplayName;
        public Mesh BaseMesh;
        public ImportedMorphTarget[] MorphTargets;
        public Dictionary<string, string> ParameterMapping;
        public Vector3 Scale;
        public Vector3 Rotation;
        public Vector3 Position;
    }

    [System.Serializable]
    public class ClothingAssetWrapper : ScriptableObject
    {
        public string AssetId;
        public string DisplayName;
        public ClothingSlot Slot;
        public Mesh BaseMesh;
        public ClothingCapsule[] Capsules;
        public Material Material;
        public Dictionary<string, string> ParameterMapping;
    }

    [System.Serializable]
    public class HairAssetWrapper : ScriptableObject
    {
        public string AssetId;
        public string DisplayName;
        public Mesh BaseMesh;
        public AttachmentPoint[] Attachments;
        public Material Material;
        public Dictionary<string, string> ParameterMapping;
    }
}