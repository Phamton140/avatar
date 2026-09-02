using System.Collections.Generic;
using System.Linq;

namespace AvatarGenerator.Core.Dependencies
{
    public class DependencyGraph : IDependencyGraph
    {
        private readonly Dictionary<string, HashSet<string>> _adjacency = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, HashSet<string>> _reverse = new Dictionary<string, HashSet<string>>();

        public bool TryAddEdge(string from, string to, out CycleInfo cycle)
        {
            cycle = null;

            if (!_adjacency.ContainsKey(from))
                _adjacency[from] = new HashSet<string>();
            if (!_reverse.ContainsKey(to))
                _reverse[to] = new HashSet<string>();

            if (_adjacency[from].Contains(to))
            {
                return true;
            }

            _adjacency[from].Add(to);
            _reverse[to].Add(from);

            if (HasCycle(out cycle))
            {
                _adjacency[from].Remove(to);
                _reverse[to].Remove(from);
                return false;
            }

            return true;
        }

        public void RemoveEdge(string from, string to)
        {
            if (_adjacency.TryGetValue(from, out var set))
                set.Remove(to);
            if (_reverse.TryGetValue(to, out var rev))
                rev.Remove(from);
        }

        public IEnumerable<string> GetEvaluationOrder()
        {
            var visited = new HashSet<string>();
            var temp = new HashSet<string>();
            var order = new List<string>();

            foreach (var node in _adjacency.Keys)
            {
                if (!visited.Contains(node))
                {
                    Visit(node, visited, temp, order);
                }
            }

            foreach (var node in _reverse.Keys)
            {
                if (!visited.Contains(node))
                {
                    Visit(node, visited, temp, order);
                }
            }

            order.Reverse();
            return order;
        }

        private void Visit(string node, HashSet<string> visited, HashSet<string> temp, List<string> order)
        {
            if (temp.Contains(node))
                return;
            if (visited.Contains(node))
                return;

            temp.Add(node);

            if (_adjacency.TryGetValue(node, out var edges))
            {
                foreach (var edge in edges)
                {
                    Visit(edge, visited, temp, order);
                }
            }

            temp.Remove(node);
            visited.Add(node);
            order.Add(node);
        }

        public HashSet<string> GetAffectedParams(string changedParam, HashSet<string> excludeOverridden = null)
        {
            var affected = new HashSet<string>();
            var queue = new Queue<string>();
            queue.Enqueue(changedParam);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (affected.Contains(current))
                    continue;

                if (excludeOverridden != null && excludeOverridden.Contains(current))
                    continue;

                affected.Add(current);

                if (_adjacency.TryGetValue(current, out var edges))
                {
                    foreach (var edge in edges)
                    {
                        queue.Enqueue(edge);
                    }
                }
            }

            affected.Remove(changedParam);
            return affected;
        }

        public bool HasCycle()
        {
            return HasCycle(out _);
        }

        public bool HasCycle(out CycleInfo cycle)
        {
            var visited = new HashSet<string>();
            var stack = new HashSet<string>();
            var path = new List<string>();

            foreach (var node in _adjacency.Keys)
            {
                if (!visited.Contains(node))
                {
                    if (DetectCycle(node, visited, stack, path, out cycle))
                    {
                        return true;
                    }
                }
            }

            cycle = null;
            return false;
        }

        private bool DetectCycle(string node, HashSet<string> visited, HashSet<string> stack, List<string> path, out CycleInfo cycle)
        {
            visited.Add(node);
            stack.Add(node);
            path.Add(node);

            if (_adjacency.TryGetValue(node, out var edges))
            {
                foreach (var edge in edges)
                {
                    if (!visited.Contains(edge))
                    {
                        if (DetectCycle(edge, visited, stack, path, out cycle))
                            return true;
                    }
                    else if (stack.Contains(edge))
                    {
                        var idx = path.IndexOf(edge);
                        cycle = new CycleInfo
                        {
                            CyclePath = path.GetRange(idx, path.Count - idx).ToArray(),
                            Message = $"Cycle detected: {string.Join(" -> ", path.GetRange(idx, path.Count - idx))} -> {edge}"
                        };
                        return true;
                    }
                }
            }

            stack.Remove(node);
            path.RemoveAt(path.Count - 1);
            cycle = null;
            return false;
        }

        public CycleInfo FindCycle()
        {
            HasCycle(out var cycle);
            return cycle;
        }
    }
}