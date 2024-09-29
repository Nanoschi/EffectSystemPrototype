using FluentAssertions;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void AddRemoveProperties()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);

            system.BasePipelines.Count.Should().Be(1);
            system.Properties.Count.Should().Be(1);

            system.Properties.Remove("health");

            system.BasePipelines.Count.Should().Be(0);
            system.Properties.Count.Should().Be(0);
        }

        [TestMethod]
        public void AddRemoveEffects()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            var effect = new ConstantEffect("health", 50, "add", 1);
            
            system.AddEffect(effect);
            system.BasePipelines["health"].GroupNames["add"].Effects.Count.Should().Be(1);

            system.RemoveEffect(effect);
            system.BasePipelines["health"].GroupNames["add"].Effects.Count.Should().Be(0);
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

            Func<Dictionary<string, object>, double> effectFunction = (input) => (int)input["intelligence"] * 0.5;

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

            Func<Dictionary<string, object>, Effect[]> metaFunction = (inputs)
                => new[] { new ConstantEffect("health", (double)(int)inputs["health_add"], "add")};

            system.AddEffect(new MetaEffect(metaFunction));
            system.Process();
            system.Results["health"].Should().Be(150);
        }

        [TestMethod]
        public void MetaEffectsCreatingMetaEffects()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            system.SetInput("health_add", 50);

            Func<Dictionary<string, object>, Effect[]> metaFunction2 = (inputs)
                => new[] { new ConstantEffect("health", (double)(int)inputs["health_add"], "add") };

            Func<Dictionary<string, object>, Effect[]> metaFunction1 = (inputs)
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

            Func<Dictionary<string, object>, Effect[]> metaFunction2 = (inputs)
                => new[] { new ConstantEffect("health", (double)(int)inputs["health_add"], "add") };

            Func<Dictionary<string, object>, Effect[]> metaFunction1 = (inputs)
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

        public void PropertyMaxValue()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100, double.NegativeInfinity, 200);

            system.AddEffect(new ConstantEffect("health", 150, "add"));
            system.Process();
            system.Results["health"].Should().Be(200);
        }
    }
}