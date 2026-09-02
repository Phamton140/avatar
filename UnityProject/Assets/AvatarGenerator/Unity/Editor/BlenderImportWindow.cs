using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using AvatarGenerator.Unity.Import;

namespace AvatarGenerator.Unity.Editor
{
    public class BlenderImportWindow : EditorWindow
    {
        private BlenderImportPipeline _pipeline;
        private string _fbxPath;
        private string _assetId;
        private string _displayName;
        private AssetImportType _importType;
        private ClothingSlot _clothingSlot;

        private Vector2 _scrollPosition;
        private string _statusMessage;
        private bool _showAdvanced;

        [MenuItem("Window/Avatar Generator/Blender Import")]
        public static void ShowWindow()
        {
            var window = GetWindow<BlenderImportWindow>("Blender Import");
            window.minSize = new Vector2(500, 600);
        }

        private void OnEnable()
        {
            _pipeline = new BlenderImportPipeline("Assets/AvatarGenerator/Data");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Blender Asset Import", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            DrawFileSelection();
            DrawAssetSettings();
            DrawAdvancedOptions();
            DrawImportButton();
            DrawStatus();
        }

        private void DrawFileSelection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("FBX File", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _fbxPath = EditorGUILayout.TextField("Path", _fbxPath);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                var path = EditorUtility.OpenFilePanel("Select FBX", "Assets", "fbx");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        _fbxPath = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        _statusMessage = "FBX must be inside Assets folder";
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_fbxPath))
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(_fbxPath);
                if (obj != null)
                {
                    EditorGUILayout.ObjectField("Preview", obj, typeof(GameObject), false);
                }
                else
                {
                    EditorGUILayout.HelpBox("Invalid FBX path", MessageType.Warning);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAssetSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Asset Settings", EditorStyles.boldLabel);

            _assetId = EditorGUILayout.TextField("Asset ID", _assetId);
            _displayName = EditorGUILayout.TextField("Display Name", _displayName);
            _importType = (AssetImportType)EditorGUILayout.EnumPopup("Import Type", _importType);

            if (_importType == AssetImportType.Clothing)
            {
                _clothingSlot = (ClothingSlot)EditorGUILayout.EnumPopup("Clothing Slot", _clothingSlot);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedOptions()
        {
            _showAdvanced = EditorGUILayout.Foldout(_showAdvanced, "Advanced Options");
            if (!_showAdvanced) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Auto-Detected from FBX/JSON:");
            EditorGUILayout.LabelField("  • Base Mesh");
            EditorGUILayout.LabelField("  • Morph Targets (if any)");
            EditorGUILayout.LabelField("  • Materials");
            EditorGUILayout.LabelField("  • Parameter Mapping (from .json)");

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Validate FBX Structure"))
            {
                ValidateFBX();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawImportButton()
        {
            EditorGUILayout.Space(8);

            GUI.enabled = CanImport();
            if (GUILayout.Button("Import Asset", GUILayout.Height(40)))
            {
                ImportAsset();
            }
            GUI.enabled = true;
        }

        private void DrawStatus()
        {
            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }
        }

        private bool CanImport()
        {
            return !string.IsNullOrEmpty(_fbxPath) &&
                   !string.IsNullOrEmpty(_assetId) &&
                   !string.IsNullOrEmpty(_displayName);
        }

        private void ImportAsset()
        {
            _statusMessage = "Importing...";

            try
            {
                BlenderImportResult result = null;

                switch (_importType)
                {
                    case AssetImportType.BaseMesh:
                        result = _pipeline.ImportBaseMesh(_fbxPath, _assetId);
                        break;

                    case AssetImportType.Clothing:
                        result = _pipeline.ImportClothing(_fbxPath, _assetId, _clothingSlot);
                        break;

                    case AssetImportType.Hair:
                        result = _pipeline.ImportHair(_fbxPath, _assetId);
                        break;

                    default:
                        _statusMessage = $"Import type {_importType} not implemented";
                        return;
                }

                if (result.Success)
                {
                    _statusMessage = $"Successfully imported: {result.AssetPath}";
                    AssetDatabase.Refresh();
                }
                else
                {
                    _statusMessage = $"Import failed: {result.ErrorMessage}";
                }
            }
            catch (System.Exception e)
            {
                _statusMessage = $"Import error: {e.Message}";
                Debug.LogError(e);
            }
        }

        private void ValidateFBX()
        {
            if (string.IsNullOrEmpty(_fbxPath))
            {
                _statusMessage = "No FBX selected";
                return;
            }

            var objects = AssetDatabase.LoadAllAssetsAtPath(_fbxPath);
            int meshCount = 0;
            int morphCount = 0;
            int materialCount = 0;

            foreach (var obj in objects)
            {
                if (obj is Mesh mesh)
                {
                    meshCount++;
                    morphCount += mesh.blendShapeCount;
                }
                else if (obj is Material)
                {
                    materialCount++;
                }
            }

            _statusMessage = $"FBX Validation: {meshCount} mesh(es), {morphCount} morph target(s), {materialCount} material(s)";
        }
    }
}