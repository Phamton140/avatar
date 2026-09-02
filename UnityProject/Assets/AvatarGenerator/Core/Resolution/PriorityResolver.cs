using System.Collections.Generic;
using AvatarGenerator.Core.Parameters;

namespace AvatarGenerator.Core.Resolution
{
    public static class PriorityResolver
    {
        public static ResolvedParameters Resolve(
            ParameterBag bag,
            CanonModel canon,
            IRuleEngine ruleEngine,
            IDependencyGraph dependencyGraph)
        {
            var resolved = new ResolvedParameters();
            var evaluated = new HashSet<string>();
            var evalOrder = dependencyGraph.GetEvaluationOrder();

            foreach (var paramId in evalOrder)
            {
                if (evaluated.Contains(paramId)) continue;

                var value = ResolveParameter(paramId, bag, canon, resolved, ruleEngine);
                resolved.Set(paramId, value, bag.HasUserOverride(paramId));
                evaluated.Add(paramId);
            }

            return resolved;
        }

        private static float ResolveParameter(
            string paramId,
            ParameterBag bag,
            CanonModel canon,
            ResolvedParameters resolved,
            IRuleEngine ruleEngine)
        {
            var intent = bag.GetIntent(paramId);
            var def = bag.Schema.Get(paramId);

            if (intent.State == ResolutionState.Locked && intent.Value.HasValue)
            {
                return intent.Value.Value;
            }

            if (intent.State == ResolutionState.Overridden && intent.Value.HasValue)
            {
                return intent.Value.Value;
            }

            if (intent.State == ResolutionState.Derived && !string.IsNullOrEmpty(intent.Expression))
            {
                var evaluator = new ExpressionEvaluator();
                return evaluator.Evaluate(intent.Expression, resolved);
            }

            var ruleOverrides = ruleEngine.EvaluateAll(resolved);
            if (ruleOverrides.TryGet(paramId, out var ruleValue))
            {
                return ruleValue;
            }

            if (intent.State == ResolutionState.Auto && intent.Value.HasValue)
            {
                return intent.Value.Value;
            }

            return def.DefaultValue;
        }
    }
}