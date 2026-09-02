# Avatar Generator - Unity Project

Procedural character generation system with parametric control over proportions, morphing, and deformation.

## Structure

```
Assets/AvatarGenerator/
├── Core/                    # Runtime core (netstandard2.1, no Unity deps)
│   ├── Parameters/          # Parameter definitions, resolution, schemas
│   ├── Resolution/          # Rule engine, priority resolver
│   ├── Dependencies/        # Dependency graph, expression evaluator
│   ├── Landmarks/           # Semantic landmarks, target generation
│   ├── Skeleton/            # Skeleton builder, IK solver
│   ├── Regions/             # Region deformers, volumetric deformation
│   ├── Morph/               # Morph blending, correctives
│   ├── Pipeline/            # Generation pipeline, caching
│   ├── Serialization/       # JSON serialization, migration
│   └── Interfaces/          # Core interfaces
├── Unity/                   # Unity-specific implementations
│   ├── Generation/          # Procedural mesh, GLTF export
│   ├── Rendering/           # Shaders, compute shaders
│   ├── Editor/              # Editor window, inspectors
│   └── Components/          # Runtime components
├── Data/                    # ScriptableObjects, assets
│   ├── Canons/              # Anatomical canons
│   ├── Morphs/              # Morph target assets
│   ├── Presets/             # Character presets
│   └── Shaders/             # Shader assets
└── Tests/                   # Unit and integration tests
    ├── Core/
    └── Integration/
```

## Key Systems

### Parameter Resolution
- **Intent vs Resolved**: User intent stored separately from computed values
- **Priority System**: DEFAULT → PRESET → PROCEDURAL → USER_OVERRIDE
- **Dependency Graph**: DAG with cycle detection, dirty propagation

### Skeleton & IK
- **FK Builder**: Procedural skeleton from canon + parameters
- **Analytical IK**: 2-3 bone chains, no iteration needed
- **Landmark Targets**: Semantic constraints drive bone positions

### Deformation Pipeline
1. Global Transform
2. Skeleton FK (bone lengths)
3. Landmark Targets
4. Skeleton IK (joint positions)
5. Region Scales (length/radius/volume)
6. Vertex Deform (GPU)
7. Morph Blend (GPU)
8. Correctives (GPU)
9. Skinning (GPU)
10. Composition

### Serialization
- Versioned JSON with migration pipeline
- Parameter intent preserved across save/load
- Schema validation

## Running Tests

Open Unity Test Runner → Run "AvatarGenerator.Tests" assembly.

## Editor Window

`Window → Avatar Generator → Character Editor`

- Real-time parameter sliders
- Auto-regeneration on change
- Save/Load `.character` files
- Export GLTF/GLB

## MVP Scope (v0.1)

- ✅ Parameter system with overrides
- ✅ Dependency graph + expression evaluator
- ✅ Skeleton FK + analytical IK
- ✅ Landmark-driven deformation
- ✅ Region-based volumetric deformation
- ✅ Procedural mesh generation (capsules/spheres)
- ✅ JSON serialization with migration
- ✅ GLTF export (mesh + skeleton + skinning)
- ✅ Editor window with live preview

## Next Steps (v0.2+)

- Blender-authored base mesh + morph targets
- Clothing system with capsule collision
- Hair proxy meshes
- Facial morph targets (ARKit subset)
- Preset system
- Undo/Redo
- Advanced validation