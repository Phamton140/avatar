using System.Collections.Generic;
using AvatarGenerator.Core.Parameters;

namespace AvatarGenerator.Core.Resolution
{
    public enum RuleScope
    {
        Parameter,
        Module,
        Global
    }

    public interface ICharacterRule
    {
        string RuleId { get; }
        int Priority { get; }
        RuleScope Scope { get; }
        IEnumerable<string> Reads { get; }
        IEnumerable<string> Writes { get; }
        void Evaluate(IResolvedParameters input, ref ParameterOverrides output);
        ValidationResult Validate(IResolvedParameters input);
    }

    public struct ValidationResult
    {
        public readonly List<ValidationIssue> Issues;

        public ValidationResult()
        {
            Issues = new List<ValidationIssue>();
        }

        public void AddIssue(ValidationIssue issue)
        {
            Issues.Add(issue);
        }

        public bool HasErrors => Issues.Exists(i => i.Severity == ValidationSeverity.Error);
        public bool HasWarnings => Issues.Exists(i => i.Severity == ValidationSeverity.Warning);

        public IEnumerable<ValidationIssue> Errors => Issues.FindAll(i => i.Severity == ValidationSeverity.Error);
        public IEnumerable<ValidationIssue> Warnings => Issues.FindAll(i => i.Severity == ValidationSeverity.Warning);
    }

    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    public struct ValidationIssue
    {
        public string ParameterId;
        public ValidationSeverity Severity;
        public string Message;
        public bool IsBlocking;
    }
}