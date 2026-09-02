# Changelog - Avatar Generator

## [0.2.0] - 2026-09-02

### Core Systems
- **Parameter System**: Intent vs Resolved separation, priority resolution (DEFAULT → PRESET → PROCEDURAL → USER_OVERRIDE), validation with warnings/errors
- **Dependency Graph**: DAG with cycle detection, topological sort, dirty propagation excluding overridden params
- **Rule Engine**: Priority-based rules with validation, procedural overrides
- **Expression Evaluator**: Safe expression language (math, functions, parameter refs)

### Skeleton & Animation
- **Skeleton Builder FK**: Procedural skeleton from canon + parameters (19 bones)
- **Analytical IK**: 2-3 bone chains, no iteration, pole vector support
- **Landmark System**: 26 semantic landmarks (head, shoulders, elbows, wrists, hips, knees, ankles, facial)
- **Landmark Targets**: Position/Distance/Ratio/Angle constraints driving IK

### Deformation Pipeline (10 Stages)
1. Global Transform
2. Skeleton FK (bone lengths)
3. Landmark Targets
4. Skeleton IK (joint positions)
5. Region Scales (length/radius/volume)
6. Vertex Deform (procedural)
7. Morph Blend (procedural + corrective)
8. Correctives (joint-angle driven)
9. Skinning (GPU-ready)
10. Module Composition

### Morph System
- **Procedural Body Morphs**: Sex dimorphism, muscular, heavy, thin, long/short limbs, wide/narrow body
- **Procedural Face Morphs**: Jaw width, eye size, nose size, face width
- **Morph Blender**: Category-based weight computation (Identity, BodyType, Proportional, Facial, Corrective)
- **ARKit Facial Blendshapes**: 52 blendshapes (Eye: 14, Brow: 5, Mouth/Jaw: 23, Nose: 2, Cheek: 3, Tongue: 1)
- **Corrective Shapes**: Joint-angle driven (elbow, knee, shoulder)

### Clothing System
- **Capsule-Based Collision**: Body capsules + clothing capsules, push-out depenetration
- **Procedural Clothing**: Shirt, Pants, Shoes, Jacket with proper capsules
- **Skinning Transfer**: Bone weight transfer from body skeleton
- **Slot System**: Shirt, Pants, Shoes, Jacket, Hat, Gloves, Accessory

### Hair System
- **16 Hair Styles**: Bald, BuzzCut, Short, Medium, Long, VeryLong, Ponytail, Bun, Afro, CurlyShort, CurlyLong, Undercut, Bob, Pixie, Braids, Dreadlocks
- **LOD Support**: Multiple detail levels
- **Attachment Points**: Forehead, temples, nape, top, ponytail base, bun center
- **Procedural Deformation**: Length/volume scaling, wind, ponytail/bun physics

### Serialization & Presets
- **Versioned JSON**: Schema versioning with migration pipeline (v0→v1)
- **Parameter Intent Preservation**: User overrides survive save/load
- **Preset Library**: 9 presets (Style: Realistic, Anime, Chibi, Heroic; BodyType: Athletic, Heavy, Thin; Age: Elderly, Child)
- **Additive Preset Stacking**: Priority-ordered, user overrides protected

### Blender Integration
- **FBX Import Pipeline**: Base mesh, morph targets, clothing, hair
- **Metadata JSON**: Parameter mapping, version tracking
- **Automatic Capsule Generation**: From mesh bounds per slot
- **Attachment Point Generation**: From mesh bounds
- **Editor Window**: Drag-drop FBX, validate, import

### Testing
- **Unit Tests**: Parameter resolution, dependency graph, rule engine, skeleton solver, preset system
- **Extreme Proportion Tests**: 3m height, 2.5x head, 2x arms, 0.3x legs - all generate without errors

### Editor
- **Character Editor Window**: Real-time sliders, preset browser, save/load, GLTF export
- **Blender Import Window**: FBX validation, import types, parameter mapping
- **Override Indicators**: ★ for user overrides, ↩ to restore auto

---

## [0.1.0] - 2026-09-02 (Initial MVP)

### Core
- Parameter system with intent/resolved separation
- Dependency graph with cycle detection
- Rule engine with priority resolution
- Expression evaluator (safe subset)

### Skeleton
- FK builder from canon
- Analytical IK solver
- 26 landmark targets

### Deformation
- 10-stage pipeline
- Region-based volumetric deformation
- Volume preservation (squash/stretch)

### Serialization
- JSON with versioning
- Parameter intent preservation

### Editor
- Character editor window
- Real-time preview
- GLTF export

---

## Roadmap

### [0.3.0] - Polish & Production Ready
- [ ] Blender-authored base mesh integration (high-quality topology/UVs)
- [ ] Advanced corrective shapes (candy-wrapper, volume loss fixes)
- [ ] Undo/Redo system (Command pattern)
- [ ] Advanced validation (self-intersection, UV quality)
- [ ] Performance optimization (Burst/Jobs for morph blending)
- [ ] LOD system for clothing/hair
- [ ] Texture baking pipeline

### [0.4.0] - Animation & Runtime
- [ ] Animation rigging package integration
- [ ] Facial animation (ARKit → blendshapes)
- [ ] Runtime API for games
- [ ] Addressables integration
- [ ] Mobile optimization

### [1.0.0] - Production Release
- [ ] Documentation & tutorials
- [ ] Asset store package
- [ ] Example scenes
- [ ] Migration guides