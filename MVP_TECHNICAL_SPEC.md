# MVP Technical Specification — Procedural Character Generator

## 1. Objetivo del MVP (v0.1)

Demostrar que el **núcleo del sistema** funciona end-to-end:

```
CharacterDefinition (JSON)
    ↓ Parameter Resolution (Intent → Resolved)
    ↓ Dependency Graph + Rule Engine
    ↓ Landmark Targets Generation
    ↓ Skeleton Solver (FK + IK)
    ↓ Region Deformers (Scale + Volume)
    ↓ Morph Blending + Correctives
    ↓ GPU Skinning
    ↓ Unity Editor Preview (sliders tiempo real)
    ↓ Save/Load .character.json
    ↓ GLTF Export
```

**Alcance v0.1:**
- ✅ 1 cuerpo base (procedural primitives)
- ✅ 1 cabeza base (procedural primitives)
- ✅ Esqueleto humanoide básico (18 huesos)
- ✅ 6 parámetros corporales + 4 faciales
- ✅ 3 morph targets: SexDimorphism, BodyType, HeadShape
- ✅ Preview en Unity Editor con sliders
- ✅ Serialización JSON + Schema v1
- ✅ Export GLTF básico (mesh + skeleton + morphs)

**Fuera de alcance v0.1:**
- ❌ Ropa, cabello, accesorios
- ❌ Morph targets faciales detallados (ARKit)
- ❌ Base mesh autorada en Blender (usar primitivas)
- ❌ Undo/Redo system
- ❌ Preset system complejo (solo defaults)

---

## 2. Estructura de Carpetas (Unity Project)

```
Assets/
├── AvatarGenerator/
│   ├── Core/                           # Runtime - Sin dependencias Unity
│   │   ├── Parameters/
│   │   │   ├── ParameterDefinition.cs
│   │   │   ├── ParameterSchema.cs
│   │   │   ├── ParameterValue.cs
│   │   │   ├── ResolvedParameters.cs
│   │   │   ├── ParameterSpace.cs
│   │   │   └── CanonModel.cs
│   │   ├── Resolution/
│   │   │   ├── PriorityResolver.cs
│   │   │   ├── RuleEngine.cs
│   │   │   ├── ICharacterRule.cs
│   │   │   └── ValidationResult.cs
│   │   ├── Dependencies/
│   │   │   ├── DependencyGraph.cs
│   │   │   ├── ExpressionEvaluator.cs
│   │   │   └── ParameterRelationship.cs
│   │   ├── Landmarks/
│   │   │   ├── LandmarkId.cs
│   │   │   ├── LandmarkDefinition.cs
│   │   │   ├── LandmarkTarget.cs
│   │   │   └── LandmarkTargetGenerator.cs
│   │   ├── Skeleton/
│   │   │   ├── BoneDefinition.cs
│   │   │   ├── SkeletonDefinition.cs
│   │   │   ├── SkeletonBuilderFK.cs
│   │   │   ├── IKSolver.cs
│   │   │   └── SkeletonSolver.cs
│   │   ├── Regions/
│   │   │   ├── RegionDefinition.cs
│   │   │   ├── RegionDeformer.cs
│   │   │   └── RegionDeformResult.cs
│   │   ├── Morph/
│   │   │   ├── MorphDefinition.cs
│   │   │   ├── MorphBlender.cs
│   │   │   └── CorrectiveShape.cs
///   │   ├── Pipeline/
///   │   │   ├── GenerationPipeline.cs
///   │   │   ├── PipelineContext.cs
///   │   │   ├── PipelineStage.cs
///   │   │   └── PipelineCache.cs
///   │   ├── Serialization/
///   │   │   ├── CharacterDefinition.cs
///   │   │   ├── CharacterSerializer.cs
///   │   │   ├── SchemaVersion.cs
///   │   │   └── MigrationPipeline.cs
///   │   └── Interfaces/
///   │       ├── ICharacterModule.cs
///   │       ├── IParameterDriver.cs
///   │       └── IGeometryGenerator.cs
///   │
///   ├── Unity/                          # Unity-specific (Editor + Runtime)
///   │   ├── Generation/
///   │   │   ├── ProceduralMeshGenerator.cs
///   │   │   ├── BaseMeshFactory.cs
///   │   │   ├── MeshComposer.cs
///   │   │   └── GLTFExporter.cs
///   │   ├── Rendering/
///   │   │   ├── CharacterRenderer.cs
///   │   │   ├── MorphShader.cs
///   │   │   └── SkinningComputeShader.compute
///   │   ├── Editor/
///   │   │   ├── CharacterEditorWindow.cs
///   │   │   ├── ParameterInspector.cs
///   │   │   ├── PreviewViewport.cs
///   │   │   ├── PresetBrowser.cs
///   │   │   └── ValidationPanel.cs
///   │   └── Components/
///   │       ├── AvatarGeneratorComponent.cs
///   │       └── AvatarPreviewComponent.cs
///   │
///   ├── Data/                           # ScriptableObjects + Assets
///   │   ├── Canons/
///   │   │   └── HumanRealistic_v1.asset
///   │   ├── Morphs/
///   │   │   ├── SexDimorphism.asset
///   │   │   ├── BodyType.asset
///   │   │   └── HeadShape.asset
///   │   ├── Presets/
///   │   │   └── DefaultPreset.asset
///   │   └── Shaders/
///   │       ├── AvatarMorph.shader
///   │       └── AvatarSkinning.compute
///   │
///   └── Tests/                          # EditMode + PlayMode Tests
///       ├── Core/
///       │   ├── ParameterResolutionTests.cs
///       │   ├── DependencyGraphTests.cs
///       │   ├── RuleEngineTests.cs
///       │   └── SkeletonSolverTests.cs
///       └── Integration/
///           ├── GenerationPipelineTests.cs
///           └── SerializationRoundtripTests.cs
///
/// Packages/
/// ├── manifest.json                     # Unity packages: Burst, Mathematics, JSON, GLTF
/// └── packages-lock.json
///
/// ProjectSettings/
/// └── ProjectVersion.txt                # Unity 2022.3 LTS
```

---

## 3. Interfaces C# Clave (Core - Sin Unity)

### 3.1 Parameter System

```csharp
// Core/Parameters/ParameterSpace.cs
namespace AvatarGenerator.Core.Parameters
{
    public enum ParameterSpace { Canonical, Parametric, Absolute }
    public enum ValueSource { Default = 0, Preset = 10, Procedural = 20, UserOverride = 100 }
    public enum ResolutionState { Auto, Derived, Overridden, Locked, Driven }

    public struct ParameterValue
    {
        public object Value;
        public ValueSource Source;
        public ResolutionState State;
        public bool IsDirty;
    }

    public struct ParameterDefinition
    {
        public string Id;
        public ParameterType Type;
        public string DisplayName;
        public string Category;
        public float? MinSuggested, MaxSuggested;
        public float DefaultValue;
        public string Unit;
        public ParameterFlags Flags;
        public string[] DependsOn;
        public string[] Drives;
        public string DerivationExpression; // Null = direct value
    }
}

// Core/Parameters/ResolvedParameters.cs
public interface IResolvedParameters : IReadOnlyDictionary<string, float>
{
    float GetFloat(string id);
    bool HasUserOverride(string id);
    Hash128 ComputeHash();
    IEnumerable<string> GetChangedParams(IResolvedParameters previous);
}
```

### 3.2 Rule Engine

```csharp
// Core/Resolution/IRuleEngine.cs
namespace AvatarGenerator.Core.Resolution
{
    public interface ICharacterRule
    {
        int Priority { get; }
        RuleScope Scope { get; }
        IEnumerable<string> Reads { get; }
        IEnumerable<string> Writes { get; }
        void Evaluate(IResolvedParameters input, ref ParameterOverrides output);
        ValidationResult Validate(IResolvedParameters input);
    }

    public interface IRuleEngine
    {
        void RegisterRule(ICharacterRule rule);
        void UnregisterRule(string ruleId);
        ParameterOverrides EvaluateAll(IResolvedParameters input);
        ValidationResult ValidateAll(IResolvedParameters input);
    }

    public struct ParameterOverrides
    {
        private Dictionary<string, (float value, ValueSource source)> _overrides;
        public void Set(string paramId, float value, ValueSource source);
        public bool TryGet(string paramId, out float value);
    }
}
```

### 3.3 Dependency Graph

```csharp
// Core/Dependencies/DependencyGraph.cs
namespace AvatarGenerator.Core.Dependencies
{
    public interface IDependencyGraph
    {
        bool TryAddEdge(string from, string to, out CycleInfo cycle);
        void RemoveEdge(string from, string to);
        IEnumerable<string> GetEvaluationOrder();
        HashSet<string> GetAffectedParams(string changedParam, HashSet<string> excludeOverridden);
        bool HasCycle();
        CycleInfo FindCycle();
    }

    public struct CycleInfo
    {
        public string[] CyclePath;
        public string Message;
    }
}

// Core/Dependencies/ExpressionEvaluator.cs
public interface IExpressionEvaluator
{
    float Evaluate(string expression, IResolvedParameters context);
    bool TryParse(string expression, out ExpressionNode ast);
    HashSet<string> GetDependencies(string expression);
}
```

### 3.4 Landmarks & Skeleton

```csharp
// Core/Landmarks/LandmarkId.cs
public enum LandmarkId
{
    HeadTop, Chin, NeckBase, NeckTop,
    LeftShoulder, RightShoulder, LeftClavicleEnd, RightClavicleEnd,
    LeftElbow, RightElbow, LeftWrist, RightWrist,
    LeftHandRoot, RightHandRoot, LeftHandTip, RightHandTip,
    ChestCenter, Sternum, Navel, PelvisCenter,
    LeftHip, RightHip, LeftKnee, RightKnee,
    LeftAnkle, RightAnkle, LeftHeel, RightHeel, LeftToeTip, RightToeTip
}

// Core/Landmarks/LandmarkTarget.cs
public struct LandmarkTarget
{
    public LandmarkId Landmark;
    public Vector3 TargetPosition;     // Model space (metros)
    public float Weight;               // 1.0 = hard
    public ConstraintType Type;        // Position, Distance, Ratio, Angle
    public LandmarkId? RelativeTo;
}

// Core/Skeleton/BoneDefinition.cs
public struct BoneDefinition
{
    public string Name;
    public string ParentName;
    public Vector3 LocalPosition;
    public Quaternion LocalRotation;
    public Vector3 LocalScale;
    public ParameterDriver[] Drivers;
    public LandmarkId[] ControlledLandmarks;
    public bool HasIK;
    public IKChainData IKData;
}

public struct IKChainData
{
    public string EffectorBone;
    public LandmarkId TargetLandmark;
    public LandmarkId PoleLandmark;
    public int ChainLength;
    public float Weight;
}

// Core/Skeleton/SkeletonDefinition.cs
public struct SkeletonDefinition
{
    public BoneDefinition[] Bones;
    public Dictionary<string, int> BoneIndexMap;
    public Matrix4x4[] BindPoses;
    public Matrix4x4[] InverseBindPoses;
    public IKChainData[] IKChains;
    public Hash128 ComputeHash();
}
```

### 3.5 Regions & Deformation

```csharp
// Core/Regions/RegionDefinition.cs
namespace AvatarGenerator.Core.Regions
{
    public enum DeformerType { ScaleLength, ScaleRadius, ScaleVolume, MorphBlend, ProceduralOffset }

    public struct RegionDefinition
    {
        public string Id;
        public BodyRegion SemanticRegion;
        public string[] Bones;
        public LandmarkId[] BoundaryLandmarks;
        public string[] PrimaryParams;
        public string[] SecondaryParams;
        public DeformerType[] Deformers;
    }

    public struct RegionDeformResult
    {
        public Vector3 Scale;                    // length, radiusX, radiusY
        public float[] MorphWeights;
        public ComputeBuffer VertexOffsets;      // Optional procedural
        public Matrix4x4 RootTransform;
    }

    public interface IRegionDeformer
    {
        RegionDeformResult Deform(RegionDefinition region, SkeletonDefinition skeleton, IResolvedParameters resolved);
    }
}
```

### 3.6 Generation Pipeline

```csharp
// Core/Pipeline/GenerationPipeline.cs
namespace AvatarGenerator.Core.Pipeline
{
    public interface IGenerationPipeline
    {
        GeneratedCharacter Generate(CharacterDefinition definition);
        GeneratedCharacter GenerateIncremental(CharacterDefinition definition, PipelineCache cache, HashSet<string> changedParams);
    }

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

    public struct GeneratedCharacter
    {
        public Mesh FinalMesh;
        public SkeletonDefinition Skeleton;
        public MaterialPropertyBlock Materials;
        public ValidationResult Validation;
        public Hash128 ContentHash;
    }
}
```

### 3.7 Serialization

```csharp
// Core/Serialization/CharacterDefinition.cs
namespace AvatarGenerator.Core.Serialization
{
    [Serializable]
    public class CharacterDefinition
    {
        public string SchemaVersion = "1.0.0";
        public int FormatVersion = 1;
        public CharacterMetadata Metadata;
        public Dictionary<string, ParameterIntent> Parameters;
        public Dictionary<string, ModuleData> Modules;
        public Dictionary<string, float> Overrides;
        public RiggingConfig Rigging;
    }

    [Serializable]
    public struct ParameterIntent
    {
        public ParameterSpace Space;
        public float? Value;
        public string Expression;
        public ResolutionState State;
    }

    public interface ICharacterSerializer
    {
        string Serialize(CharacterDefinition def, SerializationOptions options);
        CharacterDefinition Deserialize(string json);
        MigrationResult Migrate(CharacterDefinition def, int targetVersion);
    }
}
```

---

## 4. Primeros 3 Prototipos a Implementar (Orden de Prioridad)

### Prototipo 1: Parameter System + Dependency Graph + Rule Engine (Día 1-2)

**Archivos:**
- `Core/Parameters/ParameterSchema.cs`
- `Core/Parameters/ResolvedParameters.cs`
- `Core/Resolution/PriorityResolver.cs`
- `Core/Resolution/RuleEngine.cs`
- `Core/Dependencies/DependencyGraph.cs`
- `Core/Dependencies/ExpressionEvaluator.cs` (simple, sin Roslyn)
- `Tests/Core/ParameterResolutionTests.cs`
- `Tests/Core/DependencyGraphTests.cs`

**Test de validación:**
```csharp
[Test]
public void HeightChange_PropagatesToLegLength_ButNotOverriddenHead()
{
    var schema = CreateTestSchema();
    var graph = new DependencyGraph();
    graph.TryAddEdge("body.height", "body.legLength");
    graph.TryAddEdge("body.height", "body.headHeight");
    
    var params = new ParameterBag(schema);
    params.Set("body.height", 1.80f, ValueSource.UserOverride);
    params.Set("body.headScale", 1.40f, ValueSource.UserOverride); // Override
    
    var resolved = PriorityResolver.Resolve(params, graph, ruleEngine);
    
    Assert.AreEqual(1.80f, resolved["body.height"]);
    Assert.AreEqual(0.882f, resolved["body.legLength"]); // 0.49 * 1.80
    Assert.AreEqual(1.40f, resolved["body.headScale"]);  // Unchanged!
}
```

### Prototipo 2: Skeleton Builder FK + IK Solver (Día 2-3)

**Archivos:**
- `Core/Skeleton/SkeletonBuilderFK.cs`
- `Core/Skeleton/IKSolver.cs` (Analítico 2-3 huesos)
- `Core/Skeleton/SkeletonSolver.cs`
- `Core/Landmarks/LandmarkTargetGenerator.cs`
- `Tests/Core/SkeletonSolverTests.cs`

**Test de validación:**
```csharp
[Test]
public void IKSolver_ArmReach_ResolvesElbowPosition()
{
    var skeleton = SkeletonBuilderFK.BuildFromCanon(canon, resolvedParams);
    var targets = LandmarkTargetGenerator.Generate(resolvedParams, canon);
    
    var solved = SkeletonSolver.Solve(skeleton, targets);
    
    var wrist = solved.GetBoneWorldPos("LeftHand");
    var target = targets.First(t => t.Landmark == LandmarkId.LeftWrist).TargetPosition;
    
    Assert.Less(Vector3.Distance(wrist, target), 0.01f); // Within 1cm
    Assert.Greater(solved.GetBone("LeftForearm").Length, 0);
}
```

### Prototipo 3: Pipeline Integration + Unity Editor Preview (Día 3-5)

**Archivos:**
- `Core/Pipeline/GenerationPipeline.cs`
- `Unity/Generation/ProceduralMeshGenerator.cs` (Capsules/Loft para cuerpo simple)
- `Unity/Rendering/SkinningComputeShader.compute`
- `Unity/Editor/CharacterEditorWindow.cs`
- `Unity/Editor/ParameterInspector.cs`
- `Unity/Editor/PreviewViewport.cs`
- `Unity/Generation/GLTFExporter.cs` (wrapper UnityGLTF)

**Test de validación manual:**
1. Abrir `CharacterEditorWindow` en Unity
2. Mover slider `Height` → personaje crece/encoge manteniendo proporciones
3. Mover slider `HeadScale` → cabeza cambia independientemente
4. Mover slider `LegLength` → piernas cambian, IK resuelve rodillas
5. Click "Save" → genera `.character.json`
6. Click "Load" → restaura personaje idéntico
7. Click "Export GLTF" → abre en gltf-viewer, se ve correcto

---

## 5. Dependencias Unity (Packages/manifest.json)

```json
{
  "dependencies": {
    "com.unity.mathematics": "1.2.6",
    "com.unity.burst": "1.8.4",
    "com.unity.collections": "2.1.4",
    "com.unity.nuget.newtonsoft-json": "3.2.1",
    "com.unity.addressables": "1.23.0",
    "com.unity.gltfast": "6.0.0",
    "com.unity.testtools.codecoverage": "1.2.0"
  }
}
```

---

## 6. Canon Anatómico v1 (Datos - ScriptableObject)

```csharp
// Data/Canons/HumanRealistic_v1.asset
[CreateAssetMenu(menuName = "AvatarGenerator/Canon Model")]
public class CanonModel : ScriptableObject
{
    public string Version = "1.0.0";
    public float BaseHeight = 1.75f;
    public Dictionary<string, float> Proportions = new()
    {
        ["headHeight"] = 0.225f,
        ["neckHeight"] = 0.07f,
        ["torsoHeight"] = 0.50f,
        ["pelvisHeight"] = 0.12f,
        ["thighLength"] = 0.43f,
        ["calfLength"] = 0.39f,
        ["footLength"] = 0.15f,
        ["upperArmLength"] = 0.33f,
        ["forearmLength"] = 0.27f,
        ["handLength"] = 0.19f,
        ["shoulderWidth"] = 0.42f,
        ["hipWidth"] = 0.35f,
        ["chestWidth"] = 0.30f,
        ["waistWidth"] = 0.25f
    };
    public LandmarkDefinition[] Landmarks;
    public RegionDefinition[] Regions;
    public BoneDefinition[] SkeletonTemplate;
}
```

---

## 7. Parámetros v0.1 (Lista Completa)

| ID | Space | Default | Rango Sugerido | Descripción |
|----|-------|---------|----------------|-------------|
| `body.height` | Absolute | 1.75 | 0.5 - 3.0 | Altura total (m) |
| `body.headScale` | Parametric | 1.0 | 0.5 - 2.5 | Ratio vs canon headHeight |
| `body.torsoScale` | Parametric | 1.0 | 0.5 - 2.0 | Ratio vs canon torsoHeight |
| `body.legLength` | Parametric | 1.0 | 0.3 - 2.0 | Ratio vs canon legLength |
| `body.armLength` | Parametric | 1.0 | 0.3 - 2.5 | Ratio vs canon armLength |
| `body.shoulderWidth` | Parametric | 1.0 | 0.5 - 3.0 | Ratio vs canon shoulderWidth |
| `body.chestWidth` | Parametric | 1.0 | 0.5 - 2.0 | Ratio vs canon chestWidth |
| `body.hipWidth` | Parametric | 1.0 | 0.5 - 2.0 | Ratio vs canon hipWidth |
| `body.muscleMass` | Parametric | 0.5 | 0.0 - 2.0 | 0=thin, 1=athletic, 2=hulk |
| `body.bodyFat` | Parametric | 0.2 | 0.0 - 1.0 | Distribución grasa |
| `face.faceWidth` | Parametric | 1.0 | 0.7 - 1.5 | Ancho rostro |
| `face.jawWidth` | Parametric | 1.0 | 0.5 - 2.0 | Mandíbula |
| `face.eyeSize` | Parametric | 1.0 | 0.5 - 2.0 | Tamaño ojos |
| `face.noseSize` | Parametric | 1.0 | 0.5 - 2.0 | Tamaño nariz |

---

## 8. Criterios de Aceptación MVP

| # | Criterio | Verificación |
|---|----------|--------------|
| 1 | Parameter resolution respeta prioridades | Unit tests passing |
| 2 | Dependency graph detecta ciclos | Unit test con ciclo → error |
| 3 | Override usuario bloquea regla procedural | Test: headScale=1.4 no cambia al cambiar height |
| 4 | Skeleton FK genera longitudes correctas | Test: legLength=1.2 → femur+calf = canon.leg*1.2 |
| 5 | IK resuelve wrist/ankle a targets | Test: distancia < 1cm |
| 6 | Region scales componen multiplicativamente | Test: global×1.5 + muscle×1.3 = 1.95 length |
| 7 | Volume preservation mantiene volumen aprox | Test: volume ratio ∈ [0.95, 1.05] |
| 8 | Morph blending funciona en GPU | Visual: muscleMass 0→1 transiciona suave |
| 9 | Editor sliders actualizan preview <16ms | Profile: GenerationPipeline < 10ms |
| 10 | Serialización round-trip idéntica | Test: Save→Load → hash igual |
| 11 | GLTF export abre en viewer estándar | Manual: drag .glb a https://gltf-viewer.donmccurdy.com/ |

---

## 9. Próximos Pasos Inmediatos

1. **Crear proyecto Unity 2022.3 LTS** en `C:\Users\Admin\Downloads\personajes\UnityProject`
2. **Configurar Assembly Definitions** para separar Core (netstandard2.1) de Unity
3. **Implementar Prototipo 1** (Parameter System) + Tests
4. **Commit & Push** a `Development` tras cada prototipo funcional
5. **Revisar arquitectura** antes de Prototipo 2

---

## 10. Notas de Implementación Críticas

- **Core assembly:** `AvatarGenerator.Core` → netstandard2.1, **cero referencias Unity**. Testable en .NET CLI.
- **Unity assembly:** `AvatarGenerator.Unity` → referencia Core + UnityEngine.
- **Burst/Jobs:** Usar en `ProceduralMeshGenerator`, `SkinningComputeShader`, `RegionDeformer`.
- **Mathematics:** `Unity.Mathematics` (float3, quaternion, matrix) en todo Core.
- **JSON:** Newtonsoft.Json con `ContractResolver` para serializar `Dictionary<string, ParameterIntent>`.
- **GLTF:** `UnityGLTF` (GLTFast) para export. Incluir morph targets + skinning.
- **Shaders:** Un solo shader `AvatarMorph` con `#pragma multi_compile` para morph count variable.