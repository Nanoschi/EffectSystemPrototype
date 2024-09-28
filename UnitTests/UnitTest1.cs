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
            system.AddProperty("health", 100);

            system.BasePipelines.Count.Should().Be(1);
            system.BaseProperties.Count.Should().Be(1);

            system.RemoveProperty("health");

            system.BasePipelines.Count.Should().Be(0);
            system.BaseProperties.Count.Should().Be(0);
        }

        [TestMethod]
        public void AddRemoveEffects()
        {
            var system = new EffectSystem();
            system.AddProperty("health", 100);
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
            system.AddProperty("health", 100);

            system.AddEffect(new ConstantEffect("health", 50, EffectOp.Add));
            system.AddEffect(new ConstantEffect("health", 25, EffectOp.Add));

            system.Process();

            system.ProcessedProperties["health"].Should().Be(175);

        }

        [TestMethod]
        public void MultiplyEffects()
        {
            var system = new EffectSystem();
            system.AddProperty("health", 100);

            system.AddEffect(new ConstantEffect("health", 2, EffectOp.Mul));
            system.AddEffect(new ConstantEffect("health", 3, EffectOp.Mul));

            system.Process();

            system.ProcessedProperties["health"].Should().Be(600);

        }

        [TestMethod]
        public void SumMultiplyEffects()
        {
            var system = new EffectSystem();
            system.AddProperty("health", 100);

            system.AddEffect(new ConstantEffect("health", 2, EffectOp.Mul));
            system.AddEffect(new ConstantEffect("health", 50, EffectOp.Add));

            system.Process();

            system.ProcessedProperties["health"].Should().Be(250);
        }

        [TestMethod]
        public void InputEffects()
        {
            var system = new EffectSystem();
            system.SetInput("intelligence", 300);

            Func<Dictionary<string, object>, double> effectFunction = (input) => (int)input["intelligence"] * 0.5;

            system.AddProperty("mana", 100);
            system.AddEffect(new InputEffect("mana", effectFunction, EffectOp.Add));

            system.Process();

            system.ProcessedProperties["mana"].Should().Be(250);
        }

        [TestMethod]
        public void MetaEffects()
        {
            var system = new EffectSystem();
            system.AddProperty("health", 100);
            system.SetInput("health_add", 50);

            Func<Dictionary<string, object>, Effect[]> metaFunction = (inputs)
                => new[] { new ConstantEffect("health", (double)(int)inputs["health_add"], EffectOp.Add)};

            system.AddEffect(new MetaEffect(metaFunction));
            system.Process();
            system.ProcessedProperties["health"].Should().Be(150);
        }

        [TestMethod]
        public void MetaEffectsCreatingMetaEffects()
        {
            var system = new EffectSystem();
            system.AddProperty("health", 100);
            system.SetInput("health_add", 50);

            Func<Dictionary<string, object>, Effect[]> metaFunction2 = (inputs)
                => new[] { new ConstantEffect("health", (double)(int)inputs["health_add"], EffectOp.Add) };

            Func<Dictionary<string, object>, Effect[]> metaFunction1 = (inputs)
                => new[] { new MetaEffect(metaFunction2) };

            system.AddEffect(new MetaEffect(metaFunction1));
            system.Process();
            system.ProcessedProperties["health"].Should().Be(150);

        }

        [TestMethod]
        public void MetaEffectsCreatingMixedEffects()
        {
            var system = new EffectSystem();
            system.AddProperty("health", 100);
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
            system.ProcessedProperties["health"].Should().Be(1051);
        }
    }
}