using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AvatarGenerator.Core.Parameters;
using AvatarGenerator.Core.Serialization;
using AvatarGenerator.Core.Pipeline;
using AvatarGenerator.Core.Skeleton;
using AvatarGenerator.Core.Resolution;
using AvatarGenerator.Core.Dependencies;
using AvatarGenerator.Core.Landmarks;
using AvatarGenerator.Core.Regions;
using AvatarGenerator.Unity.Generation;

namespace AvatarGenerator.Unity.Editor
{
    public class CharacterEditorWindow : EditorWindow
    {
        private CharacterDefinition _currentDefinition;
        private ParameterBag _parameterBag;
        private ParameterSchema _schema;
        private CanonModel _canon;
        private GenerationPipeline _pipeline;
        private GeneratedCharacter _generatedCharacter;
        private PipelineCache _cache;

        private PresetLibrary _presetLibrary;
        private PresetApplier _presetApplier;

        private Vector2 _scrollPosition;
        private Vector2 _presetScrollPosition;
        private string _statusMessage = "Ready";
        private bool _showPresets = true;

        [MenuItem("Window/Avatar Generator/Character Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<CharacterEditorWindow>("Character Generator");
            window.minSize = new Vector2(400, 600);
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void Initialize()
        {
            _schema = ParameterSchema.CreateDefault();
            _canon = new CanonModel();

            var ruleEngine = new RuleEngine();
            var dependencyGraph = new DependencyGraph();
            SetupDependencies(dependencyGraph);

            var regionDeformer = new DefaultRegionDeformer(_canon);
            var expressionEvaluator = new ExpressionEvaluator();

            _pipeline = new GenerationPipeline(_canon, ruleEngine, dependencyGraph, regionDeformer, expressionEvaluator);

            _presetLibrary = PresetLibrary.CreateDefault();
            _presetApplier = new PresetApplier(_presetLibrary, _schema);

            _currentDefinition = new CharacterDefinition();
            _currentDefinition.Metadata.Name = "New Character";
            _currentDefinition.Metadata.GeneratorVersion = "0.1.0";
            _parameterBag = _currentDefinition.ToParameterBag(_schema);

            ApplyPreset("HUMAN_REALISTIC");
        }

        private void SetupDependencies(DependencyGraph graph)
        {
            graph.TryAddEdge("body.height", "body.legLength");
            graph.TryAddEdge("body.height", "body.armLength");
            graph.TryAddEdge("body.height", "body.headHeight");
            graph.TryAddEdge("body.height", "body.torsoHeight");
            graph.TryAddEdge("body.legLength", "body.thighLength");
            graph.TryAddEdge("body.legLength", "body.calfLength");
            graph.TryAddEdge("body.armLength", "body.upperArmLength");
            graph.TryAddEdge("body.armLength", "body.forearmLength");
            graph.TryAddEdge("body.headScale", "body.headHeight");
            graph.TryAddEdge("body.torsoScale", "body.torsoHeight");
            graph.TryAddEdge("body.shoulderWidth", "body.chestWidth");
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawParameters();
            DrawStatus();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Generate", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                GenerateCharacter();
            }

            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                SaveCharacter();
            }

            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                LoadCharacter();
            }

            if (GUILayout.Button("Export GLTF", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                ExportGLTF();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Randomize", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                RandomizeCharacter();
            }

            EditorGUILayout.EndHorizontal();

            DrawPresetBrowser();
        }

        private void DrawPresetBrowser()
        {
            _showPresets = EditorGUILayout.Foldout(_showPresets, "Presets", true, EditorStyles.foldoutHeader);
            if (!_showPresets) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _presetScrollPosition = EditorGUILayout.BeginScrollView(_presetScrollPosition, GUILayout.MaxHeight(200));

            var categories = new HashSet<string>();
            foreach (var preset in _presetLibrary.Presets)
            {
                categories.Add(preset.Category);
            }

            foreach (var category in categories)
            {
                EditorGUILayout.LabelField(category, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                foreach (var preset in _presetLibrary.Presets)
                {
                    if (preset.Category != category) continue;

                    var isActive = IsPresetActive(preset.Id);
                    var label = preset.DisplayName + (isActive ? " ✓" : "");

                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button(label, EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(true)))
                    {
                        ApplyPreset(preset.Id);
                    }

                    if (isActive)
                    {
                        if (GUILayout.Button("✕", EditorStyles.miniButtonRight, GUILayout.Width(24)))
                        {
                            RemovePreset(preset.Id);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private bool IsPresetActive(string presetId)
        {
            var preset = _presetLibrary.Get(presetId);
            if (preset == null) return false;

            foreach (var kvp in preset.Parameters)
            {
                var intent = _parameterBag.GetIntent(kvp.Key);
                if (intent.State != ResolutionState.Overridden && intent.State != ResolutionState.Derived)
                    return false;
                if (intent.Value.HasValue && kvp.Value.Value.HasValue)
                {
                    if (!Mathf.Approximately(intent.Value.Value, kvp.Value.Value.Value))
                        return false;
                }
            }
            return true;
        }

        private void ApplyPreset(string presetId)
        {
            _presetApplier.ApplyPreset(_parameterBag, presetId);
            _statusMessage = $"Applied preset: {presetId}";
            GenerateCharacter();
        }

        private void RemovePreset(string presetId)
        {
            _presetApplier.RemovePreset(_parameterBag, presetId);
            _statusMessage = $"Removed preset: {presetId}";
            GenerateCharacter();
        }

        private void DrawParameters()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            var categories = new HashSet<string>();
            foreach (var def in _schema.Definitions.Values)
            {
                categories.Add(def.Category);
            }

            foreach (var category in categories)
            {
                EditorGUILayout.LabelField(category, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                foreach (var def in _schema.Definitions.Values)
                {
                    if (def.Category != category) continue;

                    DrawParameterSlider(def);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(4);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawParameterSlider(ParameterDefinition def)
        {
            var intent = _parameterBag.GetIntent(def.Id);
            var value = _parameterBag.GetValue(def.Id).AsFloat();

            bool isOverridden = intent.State == ResolutionState.Overridden;

            EditorGUILayout.BeginHorizontal();

            string label = def.DisplayName;
            if (isOverridden) label += " ★";

            float newValue = EditorGUILayout.Slider(label, value, def.MinSuggested ?? 0f, def.MaxSuggested ?? 3f);

            if (Mathf.Abs(newValue - value) > 0.001f)
            {
                _parameterBag.SetValue(def.Id, newValue, ValueSource.UserOverride);
                _parameterBag.SetIntent(def.Id, ParameterIntent.Direct(newValue));
                _statusMessage = $"Modified: {def.DisplayName} = {newValue:F2}";
                GenerateCharacter();
            }

            if (isOverridden)
            {
                if (GUILayout.Button("↩", GUILayout.Width(24)))
                {
                    _parameterBag.SetIntent(def.Id, ParameterIntent.Auto());
                    _statusMessage = $"Restored auto: {def.DisplayName}";
                    GenerateCharacter();
                }
            }

            EditorGUILayout.EndHorizontal();

            if (def.MinSuggested.HasValue && def.MaxSuggested.HasValue)
            {
                if (value < def.MinSuggested || value > def.MaxSuggested)
                {
                    EditorGUILayout.HelpBox($"Value {value:F2} outside suggested range [{def.MinSuggested:F1}, {def.MaxSuggested:F1}]", MessageType.Warning);
                }
            }
        }

        private void DrawStatus()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(_statusMessage, EditorStyles.miniLabel);

            if (_generatedCharacter != null)
            {
                EditorGUILayout.LabelField($"Vertices: {_generatedCharacter.FinalMesh?.vertexCount ?? 0}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Triangles: {(_generatedCharacter.FinalMesh?.triangles.Length / 3 ?? 0)}", EditorStyles.miniLabel);

                if (_generatedCharacter.Validation.HasErrors)
                {
                    EditorGUILayout.HelpBox("Validation Errors:\n" + string.Join("\n", _generatedCharacter.Validation.Errors), MessageType.Error);
                }
                else if (_generatedCharacter.Validation.HasWarnings)
                {
                    EditorGUILayout.HelpBox("Warnings:\n" + string.Join("\n", _generatedCharacter.Validation.Warnings), MessageType.Warning);
                }
            }
        }

        private void GenerateCharacter()
        {
            try
            {
                _generatedCharacter = _pipeline.GenerateIncremental(_currentDefinition, _cache, _parameterBag.GetDirtyParams());
                _currentDefinition = CharacterDefinition.FromParameterBag(_parameterBag);
                _parameterBag.ClearDirty();
                _statusMessage = "Generated successfully";
            }
            catch (System.Exception e)
            {
                _statusMessage = $"Error: {e.Message}";
                Debug.LogError(e);
            }
        }

        private void SaveCharacter()
        {
            var path = EditorUtility.SaveFilePanel("Save Character", "", _currentDefinition.Metadata.Name, "character");
            if (string.IsNullOrEmpty(path)) return;

            var serializer = new CharacterSerializer();
            var json = serializer.Serialize(_currentDefinition);
            System.IO.File.WriteAllText(path, json);
            _statusMessage = $"Saved to {path}";
        }

        private void LoadCharacter()
        {
            var path = EditorUtility.OpenFilePanel("Load Character", "", "character");
            if (string.IsNullOrEmpty(path)) return;

            var json = System.IO.File.ReadAllText(path);
            var serializer = new CharacterSerializer();
            _currentDefinition = serializer.Deserialize(json);
            _parameterBag = _currentDefinition.ToParameterBag(_schema);
            _statusMessage = $"Loaded from {path}";
            GenerateCharacter();
        }

        private void ExportGLTF()
        {
            if (_generatedCharacter?.FinalMesh == null)
            {
                _statusMessage = "No character to export";
                return;
            }

            var path = EditorUtility.SaveFilePanel("Export GLTF", "", _currentDefinition.Metadata.Name, "glb");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                var exporter = new GLTFExporter();
                exporter.Export(_generatedCharacter, path);
                _statusMessage = $"Exported to {path}";
            }
            catch (System.Exception e)
            {
                _statusMessage = $"Export failed: {e.Message}";
                Debug.LogError(e);
            }
        }

        private void RandomizeCharacter()
        {
            var rng = new System.Random();
            foreach (var def in _schema.Definitions.Values)
            {
                if (def.Type == ParameterType.Float)
                {
                    float min = def.MinSuggested ?? 0f;
                    float max = def.MaxSuggested ?? 1f;
                    float value = min + (float)rng.NextDouble() * (max - min);
                    _parameterBag.SetValue(def.Id, value, ValueSource.UserOverride);
                    _parameterBag.SetIntent(def.Id, ParameterIntent.Direct(value));
                }
            }
            _statusMessage = "Randomized";
            GenerateCharacter();
        }
    }
}