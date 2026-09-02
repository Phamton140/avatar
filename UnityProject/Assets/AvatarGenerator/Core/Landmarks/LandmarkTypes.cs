namespace AvatarGenerator.Core.Landmarks
{
    public enum LandmarkId
    {
        HeadTop,
        Chin,
        NeckBase,
        NeckTop,
        LeftShoulder,
        RightShoulder,
        LeftClavicleEnd,
        RightClavicleEnd,
        LeftElbow,
        RightElbow,
        LeftWrist,
        RightWrist,
        LeftHandRoot,
        RightHandRoot,
        LeftHandTip,
        RightHandTip,
        ChestCenter,
        Sternum,
        Navel,
        PelvisCenter,
        LeftHip,
        RightHip,
        LeftKnee,
        RightKnee,
        LeftAnkle,
        RightAnkle,
        LeftHeel,
        RightHeel,
        LeftToeTip,
        RightToeTip,
        LeftEye,
        RightEye,
        NoseTip,
        MouthCenter,
        LeftEar,
        RightEar
    }

    public enum ConstraintType
    {
        Position,
        Distance,
        Ratio,
        Angle
    }

    [Serializable]
    public struct LandmarkDefinition
    {
        public LandmarkId Id;
        public UnityEngine.Vector3 LocalPosition;
        public string DrivingBone;
        public LandmarkConstraints Constraints;
    }

    [Serializable]
    public struct LandmarkConstraints
    {
        public float? MinDistanceTo;
        public float? MaxDistanceTo;
        public UnityEngine.Vector3? PlaneConstraint;
        public bool IsGrounded;
    }

    [Serializable]
    public struct LandmarkTarget
    {
        public LandmarkId Landmark;
        public UnityEngine.Vector3 TargetPosition;
        public float Weight;
        public ConstraintType Type;
        public LandmarkId? RelativeTo;
        public float TargetValue;
    }
}