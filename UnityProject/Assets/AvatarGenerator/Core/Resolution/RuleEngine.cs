using System.Collections.Generic;
using System.Linq;
using AvatarGenerator.Core.Parameters;

namespace AvatarGenerator.Core.Resolution
{
    public class RuleEngine : IRuleEngine
    {
        private readonly List<ICharacterRule> _rules = new List<ICharacterRule>();

        public void RegisterRule(ICharacterRule rule)
        {
            _rules.Add(rule);
            _rules.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        public void UnregisterRule(string ruleId)
        {
            _rules.RemoveAll(r => r.RuleId == ruleId);
        }

        public ParameterOverrides EvaluateAll(IResolvedParameters input)
        {
            var overrides = new ParameterOverrides();
            foreach (var rule in _rules)
            {
                rule.Evaluate(input, ref overrides);
            }
            return overrides;
        }

        public ValidationResult ValidateAll(IResolvedParameters input)
        {
            var result = new ValidationResult();
            foreach (var rule in _rules)
            {
                var ruleResult = rule.Validate(input);
                foreach (var issue in ruleResult.Issues)
                {
                    result.AddIssue(issue);
                }
            }
            return result;
        }
    }

    public interface IRuleEngine
    {
        void RegisterRule(ICharacterRule rule);
        void UnregisterRule(string ruleId);
        ParameterOverrides EvaluateAll(IResolvedParameters input);
        ValidationResult ValidateAll(IResolvedParameters input);
    }
}