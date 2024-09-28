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
            system.Properties.AddProperty("health", 100);

            system.BasePipelines.Count.Should().Be(1);
            system.Properties.Count.Should().Be(1);

            system.Properties.RemoveProperty("health");

            system.BasePipelines.Count.Should().Be(0);
            system.Properties.Count.Should().Be(0);
        }

        [TestMethod]
        public void AddRemoveEffects()
        {
            var system = new EffectSystem();
            system.Properties.AddProperty("health", 100);
            var effect = new ConstantEffect("health", 50, EffectOp.Add, 1);
            
            system.AddEffect(effect);
            system.BasePipelines["health"].add.Effects.Count.Should().Be(1);

            system.RemoveEffect(effect);
            system.BasePipelines["health"].add.Effects.Count.Should().Be(0);
        }

        [TestMethod]
        public void SumEffects()
        {
            var system = new EffectSystem();
            system.Properties.AddProperty("health", 100);

            system.AddEffect(new ConstantEffect("health", 50, EffectOp.Add));
            system.AddEffect(new ConstantEffect("health", 25, EffectOp.Add));

            system.Process();

            system.Results["health"].Should().Be(175);

        }

        [TestMethod]
        public void MultiplyEffects()
        {
            var system = new EffectSystem();
            system.Properties.AddProperty("health", 100);

            system.AddEffect(new ConstantEffect("health", 2, EffectOp.Mul));
            system.AddEffect(new ConstantEffect("health", 3, EffectOp.Mul));

            system.Process();

            system.Results["health"].Should().Be(600);

        }

        [TestMethod]
        public void SumMultiplyEffects()
        {
            var system = new EffectSystem();
            system.Properties.AddProperty("health", 100);

            system.AddEffect(new ConstantEffect("health", 2, EffectOp.Mul));
            system.AddEffect(new ConstantEffect("health", 50, EffectOp.Add));

            system.Process();

            system.Results["health"].Should().Be(250);
        }

        [TestMethod]
        public void InputEffects()
        {
            var system = new EffectSystem();
            system.SetInput("intelligence", 300);

            Func<Dictionary<string, object>, double> effectFunction = (input) => (int)input["intelligence"] * 0.5;

            system.Properties.AddProperty("mana", 100);
            system.AddEffect(new InputEffect("mana", effectFunction, EffectOp.Add));

            system.Process();

            system.Results["mana"].Should().Be(250);
        }

        [TestMethod]
        public void MetaEffects()
        {
            var system = new EffectSystem();
            system.Properties.AddProperty("health", 100);
            system.SetInput("health_add", 50);

            Func<Dictionary<string, object>, Effect[]> metaFunction = (inputs)
                => new[] { new ConstantEffect("health", (double)(int)inputs["health_add"], EffectOp.Add)};

            system.AddEffect(new MetaEffect(metaFunction));
            system.Process();
            system.Results["health"].Should().Be(150);
        }

        [TestMethod]
        public void MetaEffectsCreatingMetaEffects()
        {
            var system = new EffectSystem();
            system.Properties.AddProperty("health", 100);
            system.SetInput("health_add", 50);

            Func<Dictionary<string, object>, Effect[]> metaFunction2 = (inputs)
                => new[] { new ConstantEffect("health", (double)(int)inputs["health_add"], EffectOp.Add) };

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
            system.Properties.AddProperty("health", 100);
            system.SetInput("health_add", 50);

            Func<Dictionary<string, object>, Effect[]> metaFunction2 = (inputs)
                => new[] { new ConstantEffect("health", (double)(int)inputs["health_add"], EffectOp.Add) };

            Func<Dictionary<string, object>, Effect[]> metaFunction1 = (inputs)
                => new Effect[] { 
                    new MetaEffect(metaFunction2), 
                    new ConstantEffect("health", 1, EffectOp.Add),
                    new ConstantEffect("health", 10, EffectOp.Mul)};

            system.AddEffect(new MetaEffect(metaFunction1));
            system.Process();
            system.Results["health"].Should().Be(1051);
        }

        [TestMethod]
        public void PropertyMinValue()
        {
            var system = new EffectSystem();
            system.Properties.AddProperty("health", 100, 0, double.PositiveInfinity);

            system.AddEffect(new ConstantEffect("health", -200, EffectOp.Add));
            system.Process();
            system.Results["health"].Should().Be(0);
        }

        public void PropertyMaxValue()
        {
            var system = new EffectSystem();
            system.Properties.AddProperty("health", 100, double.NegativeInfinity, 200);

            system.AddEffect(new ConstantEffect("health", 150, EffectOp.Add));
            system.Process();
            system.Results["health"].Should().Be(200);
        }
    }
}