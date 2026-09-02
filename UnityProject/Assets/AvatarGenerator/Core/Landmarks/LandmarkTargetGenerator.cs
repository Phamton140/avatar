using System.Collections.Generic;
using AvatarGenerator.Core.Parameters;
using UnityEngine;

namespace AvatarGenerator.Core.Landmarks
{
    public static class LandmarkTargetGenerator
    {
        public static LandmarkTarget[] Generate(IResolvedParameters resolved, CanonModel canon)
        {
            var targets = new List<LandmarkTarget>();
            float height = resolved.GetFloat("body.height");
            float globalScale = height / canon.BaseHeight;

            float headHeight = canon.GetAbsolute("headHeight", height) * resolved.GetFloat("body.headScale");
            float legLength = canon.GetAbsolute("legLength", height) * resolved.GetFloat("body.legLength");
            float armLength = canon.GetAbsolute("armLength", height) * resolved.GetFloat("body.armLength");
            float shoulderWidth = canon.GetAbsolute("shoulderWidth", height) * resolved.GetFloat("body.shoulderWidth");
            float chestWidth = canon.GetAbsolute("chestWidth", height) * resolved.GetFloat("body.chestWidth");
            float hipWidth = canon.GetAbsolute("hipWidth", height) * resolved.GetFloat("body.hipWidth");

            float thighRatio = 0.55f;
            float thighLen = legLength * thighRatio;
            float calfLen = legLength * (1f - thighRatio);

            float upperArmRatio = 0.55f;
            float upperArmLen = armLength * upperArmRatio;
            float foreArmLen = armLength * (1f - upperArmRatio);

            // HEAD
            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.HeadTop,
                TargetPosition = new Vector3(0, height, 0),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.Chin,
                TargetPosition = new Vector3(0, height - headHeight, headHeight * 0.4f),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.NeckBase,
                TargetPosition = new Vector3(0, height - headHeight, 0),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            // SHOULDERS
            float shoulderY = height - headHeight - canon.GetAbsolute("neckHeight", height);
            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.LeftShoulder,
                TargetPosition = new Vector3(-shoulderWidth * 0.5f, shoulderY, 0),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.RightShoulder,
                TargetPosition = new Vector3(shoulderWidth * 0.5f, shoulderY, 0),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            // Distance constraint between shoulders
            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.LeftShoulder,
                TargetValue = shoulderWidth,
                Type = ConstraintType.Distance,
                Weight = 1.0f,
                RelativeTo = LandmarkId.RightShoulder
            });

            // ARMS
            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.LeftElbow,
                TargetPosition = new Vector3(-shoulderWidth * 0.5f - upperArmLen * 0.3f, shoulderY - upperArmLen * 0.7f, upperArmLen * 0.2f),
                Type = ConstraintType.Position,
                Weight = 0.8f
            });

            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.RightElbow,
                TargetPosition = new Vector3(shoulderWidth * 0.5f + upperArmLen * 0.3f, shoulderY - upperArmLen * 0.7f, upperArmLen * 0.2f),
                Type = ConstraintType.Position,
                Weight = 0.8f
            });

            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.LeftWrist,
                TargetPosition = new Vector3(-shoulderWidth * 0.5f - armLength * 0.3f, shoulderY - armLength * 0.95f, 0),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.RightWrist,
                TargetPosition = new Vector3(shoulderWidth * 0.5f + armLength * 0.3f, shoulderY - armLength * 0.95f, 0),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            // TORSO
            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.ChestCenter,
                TargetPosition = new Vector3(0, shoulderY - canon.GetAbsolute("torsoHeight", height) * 0.3f, chestWidth * 0.3f),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.PelvisCenter,
                TargetPosition = new Vector3(0, shoulderY - canon.GetAbsolute("torsoHeight", height), 0),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            // HIPS
            float hipY = shoulderY - canon.GetAbsolute("torsoHeight", height);
            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.LeftHip,
                TargetPosition = new Vector3(-hipWidth * 0.5f, hipY, 0),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.RightHip,
                TargetPosition = new Vector3(hipWidth * 0.5f, hipY, 0),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.LeftHip,
                TargetValue = hipWidth,
                Type = ConstraintType.Distance,
                Weight = 1.0f,
                RelativeTo = LandmarkId.RightHip
            });

            // LEGS
            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.LeftKnee,
                TargetPosition = new Vector3(-hipWidth * 0.5f, hipY - thighLen, 0),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.RightKnee,
                TargetPosition = new Vector3(hipWidth * 0.5f, hipY - thighLen, 0),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.LeftAnkle,
                TargetPosition = new Vector3(-hipWidth * 0.5f, hipY - legLength, 0),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.RightAnkle,
                TargetPosition = new Vector3(hipWidth * 0.5f, hipY - legLength, 0),
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            // Ground constraint
            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.LeftAnkle,
                TargetValue = 0f,
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            targets.Add(new LandmarkTarget
            {
                Landmark = LandmarkId.RightAnkle,
                TargetValue = 0f,
                Type = ConstraintType.Position,
                Weight = 1.0f
            });

            return targets.ToArray();
        }
    }
}