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

            system.basePipelines.Count.Should().Be(1);
            system.baseProperties.Count.Should().Be(1);

            system.RemoveProperty("health");

            system.basePipelines.Count.Should().Be(0);
            system.baseProperties.Count.Should().Be(0);
        }

        [TestMethod]
        public void AddRemoveEffects()
        {
            var system = new EffectSystem();
            system.AddProperty("health", 100);
            var effect = new ConstantEffect("health", 50, EffectOp.Add, 1);
            
            system.AddEffect(effect);
            system.basePipelines.Count.Should().Be(1);

            system.RemoveEffect(effect);
            //Todo: Was ändert sich da wenn man einen Effekt entfernt?
            system.basePipelines.Count.Should().Be(0);
        }
    }
}