using System.Collections.Generic;
using UnityEngine;

namespace AvatarGenerator.Core.Face
{
    public enum ARKitBlendshape
    {
        // Eye
        EyeBlinkLeft,
        EyeBlinkRight,
        EyeLookDownLeft,
        EyeLookDownRight,
        EyeLookInLeft,
        EyeLookInRight,
        EyeLookOutLeft,
        EyeLookOutRight,
        EyeLookUpLeft,
        EyeLookUpRight,
        EyeSquintLeft,
        EyeSquintRight,
        EyeWideLeft,
        EyeWideRight,

        // Brow
        BrowDownLeft,
        BrowDownRight,
        BrowInnerUp,
        BrowOuterUpLeft,
        BrowOuterUpRight,

        // Mouth
        JawForward,
        JawLeft,
        JawRight,
        JawOpen,
        MouthClose,
        MouthFunnel,
        MouthPucker,
        MouthLeft,
        MouthRight,
        MouthSmileLeft,
        MouthSmileRight,
        MouthFrownLeft,
        MouthFrownRight,
        MouthDimpleLeft,
        MouthDimpleRight,
        MouthStretchLeft,
        MouthStretchRight,
        MouthRollLower,
        MouthRollUpper,
        MouthShrugLower,
        MouthShrugUpper,
        MouthPressLeft,
        MouthPressRight,
        MouthLowerDownLeft,
        MouthLowerDownRight,

        // Nose
        NoseSneerLeft,
        NoseSneerRight,

        // Cheek
        CheekPuff,
        CheekSquintLeft,
        CheekSquintRight,

        // Tongue
        TongueOut
    }

    [System.Serializable]
    public struct ARKitBlendshapeDefinition
    {
        public ARKitBlendshape Blendshape;
        public string DisplayName;
        public string[] DrivenParameters;
        public float MinWeight;
        public float MaxWeight;
        public BlendshapeCategory Category;
    }

    public enum BlendshapeCategory
    {
        Eye,
        Brow,
        Mouth,
        Jaw,
        Nose,
        Cheek,
        Tongue
    }

    [System.Serializable]
    public struct FaceRigDefinition
    {
        public string Id;
        public ARKitBlendshapeDefinition[] Blendshapes;
        public int[] VertexIndices;
        public Vector3[][] VertexDeltas;
        public float[] DefaultWeights;
    }

    public static class ARKitBlendshapeRegistry
    {
        private static readonly ARKitBlendshapeDefinition[] _definitions = new ARKitBlendshapeDefinition[]
        {
            // Eye
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.EyeBlinkLeft, DisplayName = "Eye Blink Left", Category = BlendshapeCategory.Eye, DrivenParameters = new[] { "face.eyeBlinkLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.EyeBlinkRight, DisplayName = "Eye Blink Right", Category = BlendshapeCategory.Eye, DrivenParameters = new[] { "face.eyeBlinkRight" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.EyeLookDownLeft, DisplayName = "Eye Look Down Left", Category = BlendshapeCategory.Eye, DrivenParameters = new[] { "face.eyeLookDownLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.EyeLookDownRight, DisplayName = "Eye Look Down Right", Category = BlendshapeCategory.Eye, DrivenParameters = new[] { "face.eyeLookDownRight" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.EyeLookInLeft, DisplayName = "Eye Look In Left", Category = BlendshapeCategory.Eye, DrivenParameters = new[] { "face.eyeLookInLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.EyeLookInRight, DisplayName = "Eye Look In Right", Category = BlendshapeCategory.Eye, DrivenParameters = new[] { "face.eyeLookInRight" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.EyeLookOutLeft, DisplayName = "Eye Look Out Left", Category = BlendshapeCategory.Eye, DrivenParameters = new[] { "face.eyeLookOutLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.EyeLookOutRight, DisplayName = "Eye Look Out Right", Category = BlendshapeCategory.Eye, DrivenParameters = new[] { "face.eyeLookOutRight" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.EyeLookUpLeft, DisplayName = "Eye Look Up Left", Category = BlendshapeCategory.Eye, DrivenParameters = new[] { "face.eyeLookUpLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.EyeLookUpRight, DisplayName = "Eye Look Up Right", Category = BlendshapeCategory.Eye, DrivenParameters = new[] { "face.eyeLookUpRight" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.EyeSquintLeft, DisplayName = "Eye Squint Left", Category = BlendshapeCategory.Eye, DrivenParameters = new[] { "face.eyeSquintLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.EyeSquintRight, DisplayName = "Eye Squint Right", Category = BlendshapeCategory.Eye, DrivenParameters = new[] { "face.eyeSquintRight" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.EyeWideLeft, DisplayName = "Eye Wide Left", Category = BlendshapeCategory.Eye, DrivenParameters = new[] { "face.eyeWideLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.EyeWideRight, DisplayName = "Eye Wide Right", Category = BlendshapeCategory.Eye, DrivenParameters = new[] { "face.eyeWideRight" }, MinWeight = 0f, MaxWeight = 1f },

            // Brow
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.BrowDownLeft, DisplayName = "Brow Down Left", Category = BlendshapeCategory.Brow, DrivenParameters = new[] { "face.browDownLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.BrowDownRight, DisplayName = "Brow Down Right", Category = BlendshapeCategory.Brow, DrivenParameters = new[] { "face.browDownRight" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.BrowInnerUp, DisplayName = "Brow Inner Up", Category = BlendshapeCategory.Brow, DrivenParameters = new[] { "face.browInnerUp" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.BrowOuterUpLeft, DisplayName = "Brow Outer Up Left", Category = BlendshapeCategory.Brow, DrivenParameters = new[] { "face.browOuterUpLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.BrowOuterUpRight, DisplayName = "Brow Outer Up Right", Category = BlendshapeCategory.Brow, DrivenParameters = new[] { "face.browOuterUpRight" }, MinWeight = 0f, MaxWeight = 1f },

            // Mouth
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.JawForward, DisplayName = "Jaw Forward", Category = BlendshapeCategory.Jaw, DrivenParameters = new[] { "face.jawForward" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.JawLeft, DisplayName = "Jaw Left", Category = BlendshapeCategory.Jaw, DrivenParameters = new[] { "face.jawLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.JawRight, DisplayName = "Jaw Right", Category = BlendshapeCategory.Jaw, DrivenParameters = new[] { "face.jawRight" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.JawOpen, DisplayName = "Jaw Open", Category = BlendshapeCategory.Jaw, DrivenParameters = new[] { "face.jawOpen" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthClose, DisplayName = "Mouth Close", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthClose" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthFunnel, DisplayName = "Mouth Funnel", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthFunnel" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthPucker, DisplayName = "Mouth Pucker", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthPucker" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthLeft, DisplayName = "Mouth Left", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthRight, DisplayName = "Mouth Right", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthRight" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthSmileLeft, DisplayName = "Mouth Smile Left", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthSmileLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthSmileRight, DisplayName = "Mouth Smile Right", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthSmileRight" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthFrownLeft, DisplayName = "Mouth Frown Left", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthFrownLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthFrownRight, DisplayName = "Mouth Frown Right", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthFrownRight" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthDimpleLeft, DisplayName = "Mouth Dimple Left", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthDimpleLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthDimpleRight, DisplayName = "Mouth Dimple Right", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthDimpleRight" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthStretchLeft, DisplayName = "Mouth Stretch Left", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthStretchLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthStretchRight, DisplayName = "Mouth Stretch Right", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthStretchRight" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthRollLower, DisplayName = "Mouth Roll Lower", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthRollLower" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthRollUpper, DisplayName = "Mouth Roll Upper", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthRollUpper" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthShrugLower, DisplayName = "Mouth Shrug Lower", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthShrugLower" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthShrugUpper, DisplayName = "Mouth Shrug Upper", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthShrugUpper" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthPressLeft, DisplayName = "Mouth Press Left", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthPressLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthPressRight, DisplayName = "Mouth Press Right", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthPressRight" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthLowerDownLeft, DisplayName = "Mouth Lower Down Left", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthLowerDownLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.MouthLowerDownRight, DisplayName = "Mouth Lower Down Right", Category = BlendshapeCategory.Mouth, DrivenParameters = new[] { "face.mouthLowerDownRight" }, MinWeight = 0f, MaxWeight = 1f },

            // Nose
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.NoseSneerLeft, DisplayName = "Nose Sneer Left", Category = BlendshapeCategory.Nose, DrivenParameters = new[] { "face.noseSneerLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.NoseSneerRight, DisplayName = "Nose Sneer Right", Category = BlendshapeCategory.Nose, DrivenParameters = new[] { "face.noseSneerRight" }, MinWeight = 0f, MaxWeight = 1f },

            // Cheek
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.CheekPuff, DisplayName = "Cheek Puff", Category = BlendshapeCategory.Cheek, DrivenParameters = new[] { "face.cheekPuff" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.CheekSquintLeft, DisplayName = "Cheek Squint Left", Category = BlendshapeCategory.Cheek, DrivenParameters = new[] { "face.cheekSquintLeft" }, MinWeight = 0f, MaxWeight = 1f },
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.CheekSquintRight, DisplayName = "Cheek Squint Right", Category = BlendshapeCategory.Cheek, DrivenParameters = new[] { "face.cheekSquintRight" }, MinWeight = 0f, MaxWeight = 1f },

            // Tongue
            new ARKitBlendshapeDefinition { Blendshape = ARKitBlendshape.TongueOut, DisplayName = "Tongue Out", Category = BlendshapeCategory.Tongue, DrivenParameters = new[] { "face.tongueOut" }, MinWeight = 0f, MaxWeight = 1f }
        };

        public static ARKitBlendshapeDefinition[] GetAllDefinitions()
        {
            return _definitions;
        }

        public static ARKitBlendshapeDefinition GetDefinition(ARKitBlendshape blendshape)
        {
            foreach (var def in _definitions)
            {
                if (def.Blendshape == blendshape)
                    return def;
            }
            return default;
        }

        public static ARKitBlendshapeDefinition[] GetByCategory(BlendshapeCategory category)
        {
            var result = new List<ARKitBlendshapeDefinition>();
            foreach (var def in _definitions)
            {
                if (def.Category == category)
                    result.Add(def);
            }
            return result.ToArray();
        }

        public static int GetBlendshapeCount()
        {
            return _definitions.Length;
        }

        public static string[] GetAllBlendshapeNames()
        {
            var names = new string[_definitions.Length];
            for (int i = 0; i < _definitions.Length; i++)
            {
                names[i] = _definitions[i].Blendshape.ToString();
            }
            return names;
        }
    }

    public class ARKitFaceRigGenerator
    {
        public FaceRigDefinition GenerateFaceRig(Mesh faceMesh, CanonModel canon)
        {
            var definitions = ARKitBlendshapeRegistry.GetAllDefinitions();
            int blendshapeCount = definitions.Length;

            var vertexIndices = new List<int>();
            var vertexDeltas = new List<Vector3[]>();
            var defaultWeights = new float[blendshapeCount];

            int vertexCount = faceMesh.vertexCount;

            for (int i = 0; i < blendshapeCount; i++)
            {
                var def = definitions[i];
                var deltas = GenerateProceduralBlendshape(faceMesh, def, canon);
                vertexDeltas.Add(deltas);

                if (def.MinWeight > 0f)
                    defaultWeights[i] = def.MinWeight;
            }

            for (int i = 0; i < vertexCount; i++)
            {
                vertexIndices.Add(i);
            }

            return new FaceRigDefinition
            {
                Id = "ARKitFaceRig_v1",
                Blendshapes = definitions,
                VertexIndices = vertexIndices.ToArray(),
                VertexDeltas = vertexDeltas.ToArray(),
                DefaultWeights = defaultWeights
            };
        }

        private Vector3[] GenerateProceduralBlendshape(Mesh faceMesh, ARKitBlendshapeDefinition def, CanonModel canon)
        {
            var deltas = new Vector3[faceMesh.vertexCount];
            var vertices = faceMesh.vertices;

            Vector3 faceCenter = GetFaceCenter(vertices);
            float faceWidth = GetFaceWidth(vertices);
            float faceHeight = GetFaceHeight(vertices);

            for (int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                Vector3 delta = Vector3.zero;

                switch (def.Category)
                {
                    case BlendshapeCategory.Eye:
                        delta = GenerateEyeBlendshape(v, def, faceCenter, faceWidth, faceHeight);
                        break;
                    case BlendshapeCategory.Brow:
                        delta = GenerateBrowBlendshape(v, def, faceCenter, faceWidth, faceHeight);
                        break;
                    case BlendshapeCategory.Mouth:
                    case BlendshapeCategory.Jaw:
                        delta = GenerateMouthBlendshape(v, def, faceCenter, faceWidth, faceHeight);
                        break;
                    case BlendshapeCategory.Nose:
                        delta = GenerateNoseBlendshape(v, def, faceCenter, faceWidth, faceHeight);
                        break;
                    case BlendshapeCategory.Cheek:
                        delta = GenerateCheekBlendshape(v, def, faceCenter, faceWidth, faceHeight);
                        break;
                    case BlendshapeCategory.Tongue:
                        delta = GenerateTongueBlendshape(v, def, faceCenter, faceWidth, faceHeight);
                        break;
                }

                deltas[i] = delta;
            }

            return deltas;
        }

        private Vector3 GenerateEyeBlendshape(Vector3 v, ARKitBlendshapeDefinition def, Vector3 faceCenter, float faceWidth, float faceHeight)
        {
            float eyeY = faceCenter.y + faceHeight * 0.15f;
            float eyeZ = faceCenter.z + faceHeight * 0.1f;
            float eyeX = faceWidth * 0.2f;

            bool isLeft = def.Blendshape.ToString().EndsWith("Left");
            bool isRight = def.Blendshape.ToString().EndsWith("Right");

            float distToEye = float.MaxValue;
            if (isLeft) distToEye = Vector3.Distance(new Vector3(v.x, v.y, v.z), new Vector3(-eyeX, eyeY, eyeZ));
            if (isRight) distToEye = Vector3.Distance(new Vector3(v.x, v.y, v.z), new Vector3(eyeX, eyeY, eyeZ));

            if (distToEye > eyeX * 1.5f) return Vector3.zero;

            float influence = 1f - Mathf.Clamp01(distToEye / (eyeX * 0.8f));
            influence = influence * influence;

            var delta = Vector3.zero;

            if (def.Blendshape.ToString().Contains("Blink"))
            {
                delta.y = -influence * 0.015f;
            }
            else if (def.Blendshape.ToString().Contains("Squint"))
            {
                delta.y = -influence * 0.008f;
                delta.z = influence * 0.005f;
            }
            else if (def.Blendshape.ToString().Contains("Wide"))
            {
                delta.y = influence * 0.01f;
            }
            else if (def.Blendshape.ToString().Contains("LookDown"))
            {
                delta.y = -influence * 0.01f;
                delta.z = influence * 0.005f;
            }
            else if (def.Blendshape.ToString().Contains("LookUp"))
            {
                delta.y = influence * 0.01f;
                delta.z = -influence * 0.005f;
            }
            else if (def.Blendshape.ToString().Contains("LookIn"))
            {
                delta.x = isLeft ? influence * 0.008f : -influence * 0.008f;
            }
            else if (def.Blendshape.ToString().Contains("LookOut"))
            {
                delta.x = isLeft ? -influence * 0.008f : influence * 0.008f;
            }

            return delta;
        }

        private Vector3 GenerateBrowBlendshape(Vector3 v, ARKitBlendshapeDefinition def, Vector3 faceCenter, float faceWidth, float faceHeight)
        {
            float browY = faceCenter.y + faceHeight * 0.25f;
            float browX = faceWidth * 0.25f;

            bool isLeft = def.Blendshape.ToString().EndsWith("Left");
            bool isRight = def.Blendshape.ToString().EndsWith("Right");

            float distToBrow = float.MaxValue;
            if (isLeft) distToBrow = Vector3.Distance(new Vector3(v.x, v.y, v.z), new Vector3(-browX, browY, faceCenter.z + faceHeight * 0.1f));
            if (isRight) distToBrow = Vector3.Distance(new Vector3(v.x, v.y, v.z), new Vector3(browX, browY, faceCenter.z + faceHeight * 0.1f));

            if (distToBrow > browX * 1.2f) return Vector3.zero;

            float influence = 1f - Mathf.Clamp01(distToBrow / (browX * 0.8f));
            influence = influence * influence;

            var delta = Vector3.zero;

            if (def.Blendshape.ToString().Contains("Down"))
            {
                delta.y = -influence * 0.012f;
            }
            else if (def.Blendshape.ToString().Contains("InnerUp"))
            {
                delta.y = influence * 0.015f;
            }
            else if (def.Blendshape.ToString().Contains("OuterUp"))
            {
                delta.y = influence * 0.012f;
            }

            return delta;
        }

        private Vector3 GenerateMouthBlendshape(Vector3 v, ARKitBlendshapeDefinition def, Vector3 faceCenter, float faceWidth, float faceHeight)
        {
            float mouthY = faceCenter.y - faceHeight * 0.15f;
            float mouthZ = faceCenter.z + faceHeight * 0.12f;
            float mouthX = faceWidth * 0.18f;

            float distToMouth = Vector3.Distance(new Vector3(v.x, v.y, v.z), new Vector3(0, mouthY, mouthZ));

            if (distToMouth > mouthX * 2f) return Vector3.zero;

            float influence = 1f - Mathf.Clamp01(distToMouth / (mouthX * 1.5f));
            influence = influence * influence;

            var delta = Vector3.zero;

            if (def.Blendshape.ToString().Contains("JawOpen"))
            {
                delta.y = -influence * 0.03f;
            }
            else if (def.Blendshape.ToString().Contains("JawForward"))
            {
                delta.z = influence * 0.015f;
            }
            else if (def.Blendshape.ToString().Contains("JawLeft"))
            {
                delta.x = -influence * 0.01f;
            }
            else if (def.Blendshape.ToString().Contains("JawRight"))
            {
                delta.x = influence * 0.01f;
            }
            else if (def.Blendshape.ToString().Contains("Smile"))
            {
                delta.x = influence * 0.015f;
                delta.y = influence * 0.008f;
            }
            else if (def.Blendshape.ToString().Contains("Frown"))
            {
                delta.y = -influence * 0.01f;
            }
            else if (def.Blendshape.ToString().Contains("Funnel") || def.Blendshape.ToString().Contains("Pucker"))
            {
                delta.z = influence * 0.01f;
                delta.y = -influence * 0.005f;
            }
            else if (def.Blendshape.ToString().Contains("Stretch"))
            {
                delta.x = influence * 0.02f;
            }
            else if (def.Blendshape.ToString().Contains("Left") && !def.Blendshape.ToString().Contains("Smile") && !def.Blendshape.ToString().Contains("Frown"))
            {
                delta.x = -influence * 0.01f;
            }
            else if (def.Blendshape.ToString().Contains("Right") && !def.Blendshape.ToString().Contains("Smile") && !def.Blendshape.ToString().Contains("Frown"))
            {
                delta.x = influence * 0.01f;
            }
            else if (def.Blendshape.ToString().Contains("Close"))
            {
                delta.y = influence * 0.01f;
            }
            else if (def.Blendshape.ToString().Contains("Dimple"))
            {
                delta.z = influence * 0.005f;
            }
            else if (def.Blendshape.ToString().Contains("Roll"))
            {
                delta.z = influence * 0.008f;
            }
            else if (def.Blendshape.ToString().Contains("Shrug"))
            {
                delta.y = influence * 0.005f;
            }
            else if (def.Blendshape.ToString().Contains("Press"))
            {
                delta.z = -influence * 0.005f;
            }
            else if (def.Blendshape.ToString().Contains("LowerDown"))
            {
                delta.y = -influence * 0.015f;
            }

            return delta;
        }

        private Vector3 GenerateNoseBlendshape(Vector3 v, ARKitBlendshapeDefinition def, Vector3 faceCenter, float faceWidth, float faceHeight)
        {
            float noseY = faceCenter.y;
            float noseZ = faceCenter.z + faceHeight * 0.12f;

            float distToNose = Vector3.Distance(new Vector3(v.x, v.y, v.z), new Vector3(0, noseY, noseZ));

            if (distToNose > faceWidth * 0.15f) return Vector3.zero;

            float influence = 1f - Mathf.Clamp01(distToNose / (faceWidth * 0.1f));
            influence = influence * influence;

            var delta = Vector3.zero;

            if (def.Blendshape.ToString().Contains("Sneer"))
            {
                delta.y = influence * 0.008f;
                delta.z = influence * 0.005f;
            }

            return delta;
        }

        private Vector3 GenerateCheekBlendshape(Vector3 v, ARKitBlendshapeDefinition def, Vector3 faceCenter, float faceWidth, float faceHeight)
        {
            float cheekY = faceCenter.y - faceHeight * 0.05f;
            float cheekX = faceWidth * 0.3f;
            float cheekZ = faceCenter.z + faceHeight * 0.05f;

            bool isLeft = def.Blendshape.ToString().EndsWith("Left");
            bool isRight = def.Blendshape.ToString().EndsWith("Right");

            float distToCheek = float.MaxValue;
            if (isLeft) distToCheek = Vector3.Distance(new Vector3(v.x, v.y, v.z), new Vector3(-cheekX, cheekY, cheekZ));
            if (isRight) distToCheek = Vector3.Distance(new Vector3(v.x, v.y, v.z), new Vector3(cheekX, cheekY, cheekZ));

            if (def.Blendshape == ARKitBlendshape.CheekPuff)
            {
                distToCheek = Mathf.Min(
                    Vector3.Distance(new Vector3(v.x, v.y, v.z), new Vector3(-cheekX, cheekY, cheekZ)),
                    Vector3.Distance(new Vector3(v.x, v.y, v.z), new Vector3(cheekX, cheekY, cheekZ))
                );
            }

            if (distToCheek > cheekX * 0.8f) return Vector3.zero;

            float influence = 1f - Mathf.Clamp01(distToCheek / (cheekX * 0.6f));
            influence = influence * influence;

            var delta = Vector3.zero;

            if (def.Blendshape == ARKitBlendshape.CheekPuff)
            {
                delta = (v - faceCenter).normalized * influence * 0.02f;
            }
            else if (def.Blendshape.ToString().Contains("Squint"))
            {
                delta.y = -influence * 0.005f;
                delta.z = influence * 0.003f;
            }

            return delta;
        }

        private Vector3 GenerateTongueBlendshape(Vector3 v, ARKitBlendshapeDefinition def, Vector3 faceCenter, float faceWidth, float faceHeight)
        {
            float tongueY = faceCenter.y - faceHeight * 0.2f;
            float tongueZ = faceCenter.z + faceHeight * 0.1f;

            float distToTongue = Vector3.Distance(new Vector3(v.x, v.y, v.z), new Vector3(0, tongueY, tongueZ));

            if (distToTongue > faceWidth * 0.15f) return Vector3.zero;

            float influence = 1f - Mathf.Clamp01(distToTongue / (faceWidth * 0.1f));
            influence = influence * influence;

            var delta = new Vector3(0, -influence * 0.03f, influence * 0.02f);

            return delta;
        }

        private Vector3 GetFaceCenter(Vector3[] vertices)
        {
            Vector3 center = Vector3.zero;
            int count = 0;

            foreach (var v in vertices)
            {
                if (v.y > 1.3f && v.y < 1.7f)
                {
                    center += v;
                    count++;
                }
            }

            return count > 0 ? center / count : Vector3.up * 1.5f;
        }

        private float GetFaceWidth(Vector3[] vertices)
        {
            float minX = float.MaxValue, maxX = float.MinValue;

            foreach (var v in vertices)
            {
                if (v.y > 1.3f && v.y < 1.7f)
                {
                    minX = Mathf.Min(minX, v.x);
                    maxX = Mathf.Max(maxX, v.x);
                }
            }

            return maxX - minX;
        }

        private float GetFaceHeight(Vector3[] vertices)
        {
            float minY = float.MaxValue, maxY = float.MinValue;

            foreach (var v in vertices)
            {
                if (v.y > 1.3f && v.y < 1.7f)
                {
                    minY = Mathf.Min(minY, v.y);
                    maxY = Mathf.Max(maxY, v.y);
                }
            }

            return maxY - minY;
        }
    }
}