namespace AvatarGenerator.Core.Parameters
{
    public enum ParameterType
    {
        Float,
        Int,
        Bool,
        Enum,
        Color,
        Vector2,
        Vector3,
        Curve
    }

    [System.Flags]
    public enum ParameterFlags
    {
        None = 0,
        Hidden = 1 << 0,
        Advanced = 1 << 1,
        Deprecated = 1 << 2,
        DrivesGeometry = 1 << 3,
        DrivesSkeleton = 1 << 4,
        DrivesMorphs = 1 << 5,
        AffectsClothing = 1 << 6,
        ReadOnly = 1 << 7
    }

    public enum ParameterSpace
    {
        Canonical,
        Parametric,
        Absolute
    }

    public enum ValueSource
    {
        Default = 0,
        Preset = 10,
        Procedural = 20,
        UserOverride = 100
    }

    public enum ResolutionState
    {
        Auto,
        Derived,
        Overridden,
        Locked,
        Driven
    }
}