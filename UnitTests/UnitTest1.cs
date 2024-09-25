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

            Assert.AreEqual(1, system.basePipelines.Count);
            Assert.AreEqual(1, system.baseProperties.Count);

            system.RemoveProperty("health");

            Assert.AreEqual(0, system.basePipelines.Count);
            Assert.AreEqual(0, system.baseProperties.Count);
        }
    }
}