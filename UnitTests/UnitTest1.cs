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
    }
}