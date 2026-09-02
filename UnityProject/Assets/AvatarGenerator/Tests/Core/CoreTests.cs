using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using AvatarGenerator.Core.Parameters;
using AvatarGenerator.Core.Resolution;
using AvatarGenerator.Core.Dependencies;
using AvatarGenerator.Core.Serialization;

namespace AvatarGenerator.Tests.Core
{
    public class ParameterResolutionTests
    {
        private ParameterSchema _schema;
        private CanonModel _canon;
        private RuleEngine _ruleEngine;
        private DependencyGraph _graph;

        [SetUp]
        public void Setup()
        {
            _schema = ParameterSchema.CreateDefault();
            _canon = new CanonModel();
            _ruleEngine = new RuleEngine();
            _graph = new DependencyGraph();

            _graph.TryAddEdge("body.height", "body.legLength");
            _graph.TryAddEdge("body.height", "body.armLength");
            _graph.TryAddEdge("body.height", "body.headHeight");
            _graph.TryAddEdge("body.legLength", "body.thighLength");
            _graph.TryAddEdge("body.legLength", "body.calfLength");
        }

        [Test]
        public void DefaultValues_ResolveToCanonProportions()
        {
            var bag = new ParameterBag(_schema);
            var resolved = PriorityResolver.Resolve(bag, _canon, _ruleEngine, _graph);

            Assert.AreEqual(1.75f, resolved.GetFloat("body.height"), 0.001f);
            Assert.AreEqual(1.0f, resolved.GetFloat("body.headScale"), 0.001f);
            Assert.AreEqual(1.0f, resolved.GetFloat("body.legLength"), 0.001f);
        }

        [Test]
        public void UserOverride_BlocksProceduralRule()
        {
            var bag = new ParameterBag(_schema);
            bag.SetValue("body.height", 1.80f, ValueSource.UserOverride);
            bag.SetValue("body.headScale", 1.40f, ValueSource.UserOverride);

            var resolved = PriorityResolver.Resolve(bag, _canon, _ruleEngine, _graph);

            Assert.AreEqual(1.80f, resolved.GetFloat("body.height"), 0.001f);
            Assert.AreEqual(1.40f, resolved.GetFloat("body.headScale"), 0.001f);

            float expectedLeg = _canon.GetAbsolute("legLength", 1.80f);
            Assert.AreEqual(expectedLeg, resolved.GetFloat("body.legLength"), 0.01f);
        }

        [Test]
        public void HeightChange_PropagatesToLegLength_ButNotOverriddenHead()
        {
            var bag = new ParameterBag(_schema);
            bag.SetValue("body.height", 1.80f, ValueSource.UserOverride);
            bag.SetValue("body.headScale", 1.40f, ValueSource.UserOverride);

            var resolved = PriorityResolver.Resolve(bag, _canon, _ruleEngine, _graph);

            Assert.AreEqual(1.80f, resolved.GetFloat("body.height"), 0.001f);
            float expectedLeg = _canon.GetAbsolute("legLength", 1.80f);
            Assert.AreEqual(expectedLeg, resolved.GetFloat("body.legLength"), 0.01f);
            Assert.AreEqual(1.40f, resolved.GetFloat("body.headScale"), 0.001f);
        }

        [Test]
        public void PresetApplication_ThenOverride_PreservesOverride()
        {
            var bag = new ParameterBag(_schema);

            bag.SetValue("body.legLength", 1.20f, ValueSource.Preset);
            bag.SetValue("body.legLength", 1.50f, ValueSource.UserOverride);

            var resolved = PriorityResolver.Resolve(bag, _canon, _ruleEngine, _graph);

            Assert.AreEqual(1.50f, resolved.GetFloat("body.legLength"), 0.001f);
        }

        [Test]
        public void SerializationRoundTrip_PreservesValues()
        {
            var bag = new ParameterBag(_schema);
            bag.SetValue("body.height", 1.82f, ValueSource.UserOverride);
            bag.SetValue("body.headScale", 1.10f, ValueSource.UserOverride);
            bag.SetValue("body.legLength", 1.20f, ValueSource.UserOverride);

            var def = CharacterDefinition.FromParameterBag(bag);
            var serializer = new CharacterSerializer();
            var json = serializer.Serialize(def);
            var loaded = serializer.Deserialize(json);
            var loadedBag = loaded.ToParameterBag(_schema);

            Assert.AreEqual(1.82f, loadedBag.GetValue("body.height").AsFloat(), 0.001f);
            Assert.AreEqual(1.10f, loadedBag.GetValue("body.headScale").AsFloat(), 0.001f);
            Assert.AreEqual(1.20f, loadedBag.GetValue("body.legLength").AsFloat(), 0.001f);
        }

        [Test]
        public void Seed_ReproducesSameCharacter()
        {
            var bag1 = new ParameterBag(_schema);
            bag1.SetValue("body.height", 1.80f, ValueSource.UserOverride);

            var bag2 = new ParameterBag(_schema);
            bag2.SetValue("body.height", 1.80f, ValueSource.UserOverride);

            var resolved1 = PriorityResolver.Resolve(bag1, _canon, _ruleEngine, _graph);
            var resolved2 = PriorityResolver.Resolve(bag2, _canon, _ruleEngine, _graph);

            Assert.AreEqual(resolved1.ComputeHash(), resolved2.ComputeHash());
        }
    }

    public class DependencyGraphTests
    {
        private DependencyGraph _graph;

        [SetUp]
        public void Setup()
        {
            _graph = new DependencyGraph();
        }

        [Test]
        public void SimpleChain_ResolvesCorrectOrder()
        {
            _graph.TryAddEdge("a", "b");
            _graph.TryAddEdge("b", "c");

            var order = new List<string>(_graph.GetEvaluationOrder());

            Assert.IsTrue(order.IndexOf("a") < order.IndexOf("b"));
            Assert.IsTrue(order.IndexOf("b") < order.IndexOf("c"));
        }

        [Test]
        public void CycleDetection_ReportsCycle()
        {
            _graph.TryAddEdge("a", "b");
            _graph.TryAddEdge("b", "c");
            _graph.TryAddEdge("c", "a");

            Assert.IsTrue(_graph.HasCycle(out var cycle));
            Assert.IsNotNull(cycle);
            Assert.Greater(cycle.CyclePath.Length, 0);
        }

        [Test]
        public void TryAddEdge_RejectsCycle()
        {
            _graph.TryAddEdge("a", "b");
            _graph.TryAddEdge("b", "c");

            bool result = _graph.TryAddEdge("c", "a", out var cycle);

            Assert.IsFalse(result);
            Assert.IsNotNull(cycle);
        }

        [Test]
        public void GetAffectedParams_ReturnsDescendants()
        {
            _graph.TryAddEdge("a", "b");
            _graph.TryAddEdge("b", "c");
            _graph.TryAddEdge("a", "d");

            var affected = _graph.GetAffectedParams("a");

            Assert.IsTrue(affected.Contains("b"));
            Assert.IsTrue(affected.Contains("c"));
            Assert.IsTrue(affected.Contains("d"));
            Assert.IsFalse(affected.Contains("a"));
        }

        [Test]
        public void GetAffectedParams_ExcludesOverridden()
        {
            _graph.TryAddEdge("a", "b");
            _graph.TryAddEdge("b", "c");

            var overridden = new HashSet<string> { "b" };
            var affected = _graph.GetAffectedParams("a", overridden);

            Assert.IsFalse(affected.Contains("b"));
            Assert.IsFalse(affected.Contains("c"));
        }
    }

    public class RuleEngineTests
    {
        private RuleEngine _engine;

        [SetUp]
        public void Setup()
        {
            _engine = new RuleEngine();
        }

        [Test]
        public void RulePriority_ExecutesInOrder()
        {
            var executionOrder = new List<int>();

            _engine.RegisterRule(new TestRule(10, executionOrder));
            _engine.RegisterRule(new TestRule(5, executionOrder));
            _engine.RegisterRule(new TestRule(20, executionOrder));

            var input = new ResolvedParameters();
            _engine.EvaluateAll(input);

            Assert.AreEqual(new[] { 5, 10, 20 }, executionOrder);
        }

        [Test]
        public void RuleOverrides_AppliedByPriority()
        {
            _engine.RegisterRule(new OverrideRule("param1", 1.0f, 10));
            _engine.RegisterRule(new OverrideRule("param1", 2.0f, 5));
            _engine.RegisterRule(new OverrideRule("param1", 3.0f, 20));

            var input = new ResolvedParameters();
            var overrides = _engine.EvaluateAll(input);

            Assert.IsTrue(overrides.TryGet("param1", out var value));
            Assert.AreEqual(1.0f, value);
        }

        private class TestRule : ICharacterRule
        {
            public string RuleId => $"TestRule_{Priority}";
            public int Priority { get; }
            public RuleScope Scope => RuleScope.Global;
            public IEnumerable<string> Reads => System.Array.Empty<string>();
            public IEnumerable<string> Writes => System.Array.Empty<string>();
            private List<int> _log;

            public TestRule(int priority, List<int> log)
            {
                Priority = priority;
                _log = log;
            }

            public void Evaluate(IResolvedParameters input, ref ParameterOverrides output)
            {
                _log.Add(Priority);
            }

            public ValidationResult Validate(IResolvedParameters input) => new ValidationResult();
        }

        private class OverrideRule : ICharacterRule
        {
            public string RuleId => $"OverrideRule_{Priority}";
            public int Priority { get; }
            public RuleScope Scope => RuleScope.Parameter;
            public IEnumerable<string> Reads => System.Array.Empty<string>();
            public IEnumerable<string> Writes => new[] { _param };
            private string _param;
            private float _value;

            public OverrideRule(string param, float value, int priority)
            {
                _param = param;
                _value = value;
                Priority = priority;
            }

            public void Evaluate(IResolvedParameters input, ref ParameterOverrides output)
            {
                output.Set(_param, _value, ValueSource.Procedural);
            }

            public ValidationResult Validate(IResolvedParameters input) => new ValidationResult();
        }
    }

    public class SkeletonSolverTests
    {
        private CanonModel _canon;
        private ParameterSchema _schema;
        private DependencyGraph _graph;
        private RuleEngine _ruleEngine;

        [SetUp]
        public void Setup()
        {
            _canon = new CanonModel();
            _schema = ParameterSchema.CreateDefault();
            _graph = new DependencyGraph();
            _ruleEngine = new RuleEngine();
        }

        [Test]
        public void BuildFromCanon_CreatesCorrectBoneCount()
        {
            var bag = new ParameterBag(_schema);
            var resolved = PriorityResolver.Resolve(bag, _canon, _ruleEngine, _graph);
            var skeleton = SkeletonBuilderFK.BuildFromCanon(_canon, resolved);

            Assert.AreEqual(19, skeleton.Bones.Length);
        }

        [Test]
        public void BoneLengths_ScaleWithHeight()
        {
            var bag1 = new ParameterBag(_schema);
            bag1.SetValue("body.height", 1.75f, ValueSource.UserOverride);

            var bag2 = new ParameterBag(_schema);
            bag2.SetValue("body.height", 2.00f, ValueSource.UserOverride);

            var resolved1 = PriorityResolver.Resolve(bag1, _canon, _ruleEngine, _graph);
            var resolved2 = PriorityResolver.Resolve(bag2, _canon, _ruleEngine, _graph);

            var skel1 = SkeletonBuilderFK.BuildFromCanon(_canon, resolved1);
            var skel2 = SkeletonBuilderFK.BuildFromCanon(_canon, resolved2);

            var thigh1 = skel1.Bones[skel1.GetBoneIndex("LeftThigh")];
            var thigh2 = skel2.Bones[skel2.GetBoneIndex("LeftThigh")];

            float ratio = thigh2.LocalPosition.magnitude / thigh1.LocalPosition.magnitude;
            Assert.AreEqual(2.00f / 1.75f, ratio, 0.01f);
        }

        [Test]
        public void IKSolver_ArmReach_ResolvesElbowPosition()
        {
            var bag = new ParameterBag(_schema);
            bag.SetValue("body.height", 1.80f, ValueSource.UserOverride);
            bag.SetValue("body.armLength", 1.0f, ValueSource.UserOverride);

            var resolved = PriorityResolver.Resolve(bag, _canon, _ruleEngine, _graph);
            var skeleton = SkeletonBuilderFK.BuildFromCanon(_canon, resolved);
            var targets = LandmarkTargetGenerator.Generate(resolved, _canon);

            IKSolver.SolveAll(skeleton, targets);

            var wristIdx = skeleton.GetBoneIndex("LeftHand");
            var wristPos = skeleton.GetBoneWorldPosition(wristIdx, ComputeWorldMatrices(skeleton));

            var target = Array.Find(targets, t => t.Landmark == LandmarkId.LeftWrist);

            Assert.Less(Vector3.Distance(wristPos, target.TargetPosition), 0.02f);
        }

        [Test]
        public void ExtremeProportions_GenerateWithoutError()
        {
            var bag = new ParameterBag(_schema);
            bag.SetValue("body.height", 3.00f, ValueSource.UserOverride);
            bag.SetValue("body.headScale", 2.5f, ValueSource.UserOverride);
            bag.SetValue("body.armLength", 2.0f, ValueSource.UserOverride);
            bag.SetValue("body.legLength", 0.3f, ValueSource.UserOverride);

            var resolved = PriorityResolver.Resolve(bag, _canon, _ruleEngine, _graph);
            var skeleton = SkeletonBuilderFK.BuildFromCanon(_canon, resolved);
            var targets = LandmarkTargetGenerator.Generate(resolved, _canon);

            Assert.DoesNotThrow(() => IKSolver.SolveAll(skeleton, targets));

            foreach (var bone in skeleton.Bones)
            {
                Assert.IsFalse(float.IsNaN(bone.LocalPosition.x));
                Assert.IsFalse(float.IsNaN(bone.LocalPosition.y));
                Assert.IsFalse(float.IsNaN(bone.LocalPosition.z));
            }
        }

        private Matrix4x4[] ComputeWorldMatrices(SkeletonDefinition skeleton)
        {
            var matrices = new Matrix4x4[skeleton.Bones.Length];
            var nameToIndex = new Dictionary<string, int>();
            for (int i = 0; i < skeleton.Bones.Length; i++)
                nameToIndex[skeleton.Bones[i].Name] = i;

            for (int i = 0; i < skeleton.Bones.Length; i++)
            {
                var bone = skeleton.Bones[i];
                var local = Matrix4x4.TRS(bone.LocalPosition, bone.LocalRotation, bone.LocalScale);

                if (string.IsNullOrEmpty(bone.ParentName))
                {
                    matrices[i] = local;
                }
                else
                {
                    matrices[i] = matrices[nameToIndex[bone.ParentName]] * local;
                }
            }
            return matrices;
}
        }
    }

    public class PresetSystemTests
    {
        private ParameterSchema _schema;
        private CanonModel _canon;
        private RuleEngine _ruleEngine;
        private DependencyGraph _graph;
        private PresetLibrary _library;
        private PresetApplier _applier;

        [SetUp]
        public void Setup()
        {
            _schema = ParameterSchema.CreateDefault();
            _canon = new CanonModel();
            _ruleEngine = new RuleEngine();
            _graph = new DependencyGraph();
            _library = PresetLibrary.CreateDefault();
            _applier = new PresetApplier(_library, _schema);
        }

        [Test]
        public void DefaultPresets_Exist()
        {
            Assert.IsNotNull(_library.Get("HUMAN_REALISTIC"));
            Assert.IsNotNull(_library.Get("ANIME"));
            Assert.IsNotNull(_library.Get("CHIBI"));
            Assert.IsNotNull(_library.Get("HEROIC"));
            Assert.IsNotNull(_library.Get("ATHLETIC_BUILD"));
            Assert.IsNotNull(_library.Get("HEAVY_BUILD"));
            Assert.IsNotNull(_library.Get("THIN_BUILD"));
            Assert.IsNotNull(_library.Get("ELDERLY"));
            Assert.IsNotNull(_library.Get("CHILD"));
        }

        [Test]
        public void ApplyPreset_SetsParameters()
        {
            var bag = new ParameterBag(_schema);
            _applier.ApplyPreset(bag, "HUMAN_REALISTIC");

            Assert.AreEqual(1.75f, bag.GetValue("body.height").AsFloat(), 0.001f);
            Assert.AreEqual(1.0f, bag.GetValue("body.headScale").AsFloat(), 0.001f);
            Assert.AreEqual(0.5f, bag.GetValue("body.muscleMass").AsFloat(), 0.001f);
        }

        [Test]
        public void ApplyAdditivePreset_StacksOnBase()
        {
            var bag = new ParameterBag(_schema);
            _applier.ApplyPreset(bag, "HUMAN_REALISTIC");
            _applier.ApplyPreset(bag, "ANIME");

            Assert.AreEqual(1.75f, bag.GetValue("body.height").AsFloat(), 0.001f);
            Assert.AreEqual(1.25f, bag.GetValue("body.headScale").AsFloat(), 0.001f);
            Assert.AreEqual(1.15f, bag.GetValue("body.legLength").AsFloat(), 0.001f);
            Assert.AreEqual(1.4f, bag.GetValue("face.eyeSize").AsFloat(), 0.001f);
        }

        [Test]
        public void UserOverride_BlocksAdditivePreset()
        {
            var bag = new ParameterBag(_schema);
            _applier.ApplyPreset(bag, "HUMAN_REALISTIC");
            bag.SetValue("body.headScale", 1.50f, ValueSource.UserOverride);
            _applier.ApplyPreset(bag, "ANIME");

            Assert.AreEqual(1.50f, bag.GetValue("body.headScale").AsFloat(), 0.001f);
        }

        [Test]
        public void ApplyPresetStack_OrdersByPriority()
        {
            var bag = new ParameterBag(_schema);
            _applier.ApplyPresetStack(bag, new[] { "HEROIC", "ATHLETIC_BUILD" });

            Assert.AreEqual(1.9f, bag.GetValue("body.height").AsFloat(), 0.001f);
            Assert.AreEqual(1.2f, bag.GetValue("body.shoulderWidth").AsFloat(), 0.001f);
            Assert.AreEqual(0.8f, bag.GetValue("body.muscleMass").AsFloat(), 0.001f);
        }

        [Test]
        public void RemovePreset_RestoresAuto()
        {
            var bag = new ParameterBag(_schema);
            _applier.ApplyPreset(bag, "ANIME");
            _applier.RemovePreset(bag, "ANIME");

            var intent = bag.GetIntent("body.headScale");
            Assert.AreEqual(ResolutionState.Auto, intent.State);
        }

        [Test]
        public void GetActivePresets_DetectsActive()
        {
            var bag = new ParameterBag(_schema);
            _applier.ApplyPreset(bag, "HUMAN_REALISTIC");

            var active = _applier.GetActivePresets(bag);
            Assert.Contains("HUMAN_REALISTIC", active);
        }

        [Test]
        public void AnimePreset_ProducesExpectedProportions()
        {
            var bag = new ParameterBag(_schema);
            _applier.ApplyPreset(bag, "ANIME");

            var resolved = PriorityResolver.Resolve(bag, _canon, _ruleEngine, _graph);

            Assert.Greater(resolved.GetFloat("body.headScale"), 1.1f);
            Assert.Greater(resolved.GetFloat("face.eyeSize"), 1.2f);
            Assert.Less(resolved.GetFloat("face.jawWidth"), 0.9f);
        }

        [Test]
        public void ChibiPreset_ProducesExpectedProportions()
        {
            var bag = new ParameterBag(_schema);
            _applier.ApplyPreset(bag, "CHIBI");

            var resolved = PriorityResolver.Resolve(bag, _canon, _ruleEngine, _graph);

            Assert.AreEqual(1.2f, resolved.GetFloat("body.height"), 0.01f);
            Assert.Greater(resolved.GetFloat("body.headScale"), 1.5f);
            Assert.Less(resolved.GetFloat("body.legLength"), 0.6f);
        }
    }
}