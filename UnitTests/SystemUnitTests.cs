using EffectSystemPrototype;
using FluentAssertions;

namespace UnitTests
{
    [TestClass]
    public class SystemUnitTests
    {
        [TestMethod]
        public void AddRemoveInput()
        {
            var system = new EffectSystem();
            system.SetInput("last_kill_time", 1233434234234L);

            system.InputVector.Should().HaveCount(1);
            system.InputVector.First().Should().Be(("last_kill_time", 1233434234234L));
            system.TryGetInputValue<long>("last_kill_time", out var value).Should().BeTrue();
            value.Should().Be(1233434234234L);

            system.RemoveInput("last_kill_time");
            system.TryGetInputValue<long>("last_kill_time", out var value2).Should().BeFalse();
            system.InputVector.Should().HaveCount(0);
        }

        [TestMethod]
        public void TryGetInputValue()
        {
            var system = new EffectSystem();
            system.SetInput("last_kill_time", 1233434234234L);
            system.TryGetInputValue<long>("last_kill_time", out var value).Should().BeTrue();
            system.TryGetInputValue<int>("last_kill_time", out var value2).Should().BeFalse();
            system.TryGetInputValue<string>("last_kill_time", out var value3).Should().BeFalse();

        }


        [TestMethod]
        public void AddRemoveProperties()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);

            system.PipelineCount.Should().Be(1);
            system.Properties.Count.Should().Be(1);

            system.Properties.Remove("health");

            system.PipelineCount.Should().Be(0);
            system.Properties.Count.Should().Be(0);
        }

        [TestMethod]
        public void AddRemoveEffects()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            var effect = new ConstantEffect("health", 50, "add");
            
            system.AddEffect(effect);
            system.GetEffectsOfGroup("health", "add").Should().HaveCount(1);

            system.RemoveEffect(effect);
            system.GetEffectsOfGroup("health", "add").Should().HaveCount(0);
        }

        [TestMethod]
        public void SumEffects()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);

            system.AddEffect(new ConstantEffect("health", 50, "add"));
            system.AddEffect(new ConstantEffect("health", 25, "add"));

            system.Process();

            system.Results["health"].Should().Be(175);

        }

        [TestMethod]
        public void MultiplyEffects()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);

            system.AddEffect(new ConstantEffect("health", 2, "mul"));
            system.AddEffect(new ConstantEffect("health", 3, "mul"));

            system.Process();

            system.Results["health"].Should().Be(600);

        }

        [TestMethod]
        public void SumMultiplyEffects()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);

            system.AddEffect(new ConstantEffect("health", 2, "mul"));
            system.AddEffect(new ConstantEffect("health", 50, "add"));

            system.Process();

            system.Results["health"].Should().Be(250);
        }

        [TestMethod]
        public void InputEffects()
        {
            var system = new EffectSystem();
            system.SetInput("intelligence", 300);

            Func<InputVector, double> effectFunction = (input) => (int)input["intelligence"] * 0.5;

            system.Properties.Add("mana", 100);
            system.AddEffect(new InputEffect("mana", effectFunction, "add"));

            system.Process();

            system.Results["mana"].Should().Be(250);
        }

        [TestMethod]
        public void MetaEffects()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            system.SetInput("health_add", 50);

            Func<InputVector, Effect[]> metaFunction = (inputs)
                => new[] { new ConstantEffect("health", (double)(int)inputs["health_add"], "add")};

            system.AddEffect(new MetaEffect(metaFunction));
            system.Process();
            system.Results["health"].Should().Be(150);
        }

        [TestMethod]
        public void MultiProcessMetaEffects()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            system.SetInput("health_add", 50);

            Func<InputVector, Effect[]> metaFunction = (inputs)
                => new[] { new ConstantEffect("health", (double)(int)inputs["health_add"], "add") };

            system.AddEffect(new MetaEffect(metaFunction));
            system.Process();
            system.Results["health"].Should().Be(150);
            system.Process();
            system.Results["health"].Should().Be(150);
            system.Process();
            system.Results["health"].Should().Be(150);
        }

        [TestMethod]
        public void MetaEffectsCreatingMetaEffects()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            system.SetInput("health_add", 50);

            Func<InputVector, Effect[]> metaFunction2 = (inputs)
                => new[] { new ConstantEffect("health", (double)(int)inputs["health_add"], "add") };

            Func<InputVector, Effect[]> metaFunction1 = (inputs)
                => new[] { new MetaEffect(metaFunction2) };

            system.AddEffect(new MetaEffect(metaFunction1));
            system.Process();
            system.Results["health"].Should().Be(150);

        }

        [TestMethod]
        public void MetaEffectsCreatingMixedEffects()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            system.SetInput("health_add", 50);

            Func<InputVector, Effect[]> metaFunction2 = (inputs)
                => new[] { new ConstantEffect("health", (double)(int)inputs["health_add"], "add") };

            Func<InputVector, Effect[]> metaFunction1 = (inputs)
                => new Effect[] { 
                    new MetaEffect(metaFunction2), 
                    new ConstantEffect("health", 1, "add"),
                    new ConstantEffect("health", 10, "mul")};

            system.AddEffect(new MetaEffect(metaFunction1));
            system.Process();
            system.Results["health"].Should().Be(1051);
        }

        [TestMethod]
        public void PropertyMinValue()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100, 0, double.PositiveInfinity);

            system.AddEffect(new ConstantEffect("health", -200, "add"));
            system.Process();
            system.Results["health"].Should().Be(0);
        }

        [TestMethod]
        public void PropertyMaxValue()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100, double.NegativeInfinity, 200);

            system.AddEffect(new ConstantEffect("health", 150, "add"));
            system.Process();
            system.Results["health"].Should().Be(200);
        }

        [TestMethod]
        public void AddRemovePipelineGroup()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100, false);

            system.AddGroup("health","group", EffectOp.Add, EffectOp.Add);
            system.GetGroups("health").Length.Should().Be(1);

            system.RemoveGroup("health", "group");
            system.GetGroups("health").Length.Should().Be(0);
        }

        [TestMethod]
        public void PoeLikeIncreasedMore()
        {
            //https://www.pathofexile.com/forum/view-thread/261646
            var system = new EffectSystem();
            system.Properties.Add("energy_shield", 100, false);
            system.AddGroup("energy_shield", "increased", EffectOp.Mul, EffectOp.Add);
            system.AddGroup("energy_shield", "more", EffectOp.Mul, EffectOp.Mul);

            system.AddEffect(new ConstantEffect("energy_shield", 1, "increased"));
            system.AddEffect(new ConstantEffect("energy_shield", 0.2, "increased"));
            system.AddEffect(new ConstantEffect("energy_shield", 0.3, "increased"));

            system.AddEffect(new ConstantEffect("energy_shield", 1.1, "more"));
            system.AddEffect(new ConstantEffect("energy_shield", 1.2, "more"));


            system.Process();
            system.Results["energy_shield"].Should().Be(198);
        }

        [TestMethod]
        public void AddRemoveThresholdValue()
        {
            var system = new EffectSystem();
            system.Thresholds.AddValue("time");
            system.Thresholds.Thresholds.Count.Should().Be(1);

            system.Thresholds.RemoveValue("time");
            system.Thresholds.Thresholds.Count.Should().Be(0);
        }

        [TestMethod]
        public void ApplyThreshold()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            system.Thresholds.AddValue("time", 0);
            var effect = new ConstantEffect("health", 100, "add");
            system.AddEffect(effect);
            system.Thresholds.AddEffect(effect, "time", 1, LimitDirection.Max);

            system.Thresholds.IncValue("time", 0.5);
            system.Process();
            system.Results["health"].Should().Be(200);

            system.Thresholds.IncValue("time", 1.5);
            system.Process();
            system.Results["health"].Should().Be(100);
        }

        [TestMethod]
        public void AddConfig()
        {
            var system = new EffectSystem();
            
            system.AddConfig("energy_shield", 100)
                .AddGroup("increased", EffectOp.Mul, EffectOp.Add)
                .AddGroup("more", EffectOp.Mul, EffectOp.Mul)
                .AddConstantEffect(1, "increased")
                .AddConstantEffect(0.2, "increased")
                .AddConstantEffect(0.3, "increased")
                .AddConstantEffect(1.1, "more")
                .AddConstantEffect(1.2, "more")
                .Build();

            system.Process();
            system.Results["energy_shield"].Should().Be(198);
        }
    }
}