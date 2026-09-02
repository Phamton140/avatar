using System;

namespace AvatarGenerator.Core.Parameters
{
    [Serializable]
    public struct ParameterDefinition
    {
        public string Id;
        public ParameterType Type;
        public string DisplayName;
        public string Category;
        public float? MinSuggested;
        public float? MaxSuggested;
        public float DefaultValue;
        public string Unit;
        public ParameterFlags Flags;
        public string[] DependsOn;
        public string[] Drives;
        public string DerivationExpression;
    }

    [Serializable]
    public struct ParameterValue
    {
        public object Value;
        public ValueSource Source;
        public ResolutionState State;
        public bool IsDirty;

        public static ParameterValue CreateDefault(float defaultValue)
        {
            return new ParameterValue
            {
                Value = defaultValue,
                Source = ValueSource.Default,
                State = ResolutionState.Auto,
                IsDirty = true
            };
        }

        public static ParameterValue Create(float value, ValueSource source, ResolutionState state = ResolutionState.Auto)
        {
            return new ParameterValue
            {
                Value = value,
                Source = source,
                State = state,
                IsDirty = true
            };
        }

        public float AsFloat()
        {
            return Value is float f ? f : Convert.ToSingle(Value);
        }

        public int AsInt()
        {
            return Value is int i ? i : Convert.ToInt32(Value);
        }

        public bool AsBool()
        {
            return Value is bool b ? b : Convert.ToBoolean(Value);
        }
    }

    [Serializable]
    public struct ParameterIntent
    {
        public ParameterSpace Space;
        public float? Value;
        public string Expression;
        public ResolutionState State;

        public static ParameterIntent Auto()
        {
            return new ParameterIntent { Space = ParameterSpace.Parametric, Value = null, Expression = null, State = ResolutionState.Auto };
        }

        public static ParameterIntent Direct(float value, ParameterSpace space = ParameterSpace.Parametric)
        {
            return new ParameterIntent { Space = space, Value = value, Expression = null, State = ResolutionState.Overridden };
        }

        public static ParameterIntent Derived(string expression)
        {
            return new ParameterIntent { Space = ParameterSpace.Parametric, Value = null, Expression = expression, State = ResolutionState.Derived };
        }
    }
}