using EffectSystemPrototype;
using FluentAssertions;
using System.Reflection.Metadata.Ecma335;

namespace UnitTests
{
    [TestClass]
    public class SystemUnitTests
    {
        [TestMethod]
        public void AddRemoveInput()
        {
            var system = new EffectSystem();
            system.Inputs.SetValue("last_kill_time", 1233434234234L);

            system.InputVector.Should().HaveCount(1);
            system.InputVector.First().Should().Be(("last_kill_time", 1233434234234L));
            system.TryGetInputValue<long>("last_kill_time", out var value).Should().BeTrue();
            value.Should().Be(1233434234234L);

            system.Inputs.Remove("last_kill_time");
            system.TryGetInputValue<long>("last_kill_time", out var value2).Should().BeFalse();
            system.InputVector.Should().HaveCount(0);
        }

        [TestMethod]
        public void TryGetInputValue()
        {
            var system = new EffectSystem();
            system.Inputs.SetValue("last_kill_time", 1233434234234L);
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
            system.GetGroupEffectCount("health", "add").Should().Be(1);

            system.RemoveEffect(effect);
            system.GetGroupEffectCount("health", "add").Should().Be(0);
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
            system.Inputs.SetValue("intelligence", 300);

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
            system.Inputs.SetValue("health_add", 50);

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
            system.Inputs.SetValue("health_add", 50);

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
            system.Inputs.SetValue("health_add", 50);

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
            system.Inputs.SetValue("health_add", 50);

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
            system.Properties.Add("health", 100);
            system.Ranges.SetMinValue("health", 0);

            system.AddEffect(new ConstantEffect("health", -200, "add"));
            system.Process();
            system.Results["health"].Should().Be(0);
        }

        [TestMethod]
        public void PropertyMaxValue()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            system.Ranges.SetMaxValue("health", 200);

            system.AddEffect(new ConstantEffect("health", 150, "add"));
            system.Process();
            system.Results["health"].Should().Be(200);
        }

        [TestMethod]
        public void AddRemovePipelineGroup()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100, false, false);

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
        public void ApplyThresholdNumeric()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            var effect = new ConstantEffect("health", 100, "add");
            system.AddEffect(effect);

            system.Inputs["time"] = 0;
            system.Thresholds.AddEffect(effect, "time", 1, RemoveCondition.Greater);

            system.Inputs["time"] = Convert.ToDouble(system.Inputs["time"]) + 0.5;
            system.Process();
            system.Results["health"].Should().Be(200);

            system.Inputs["time"] = Convert.ToDouble(system.Inputs["time"]) + 1.5;
            system.Process();
            system.Results["health"].Should().Be(100);
        }

        [TestMethod]
        public void ApplyThresholdCompare()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            var effect = new ConstantEffect("health", 100, "add");
            system.AddEffect(effect);

            system.Inputs["name"] = "John";
            system.Thresholds.AddEffect(effect, "name", "Mike", RemoveCondition.Equals);

            system.Process();
            system.Results["health"].Should().Be(200);

            system.Inputs["name"] = "Mike";
            system.Process();
            system.Results["health"].Should().Be(100);
        }

        [TestMethod]
        public void ApplyThresholdFunc()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            var effect = new ConstantEffect("health", 100, "add");
            system.AddEffect(effect);

            system.Inputs["name"] = "John";
            var comp = (object value) => ((string)value).Contains('M');
            system.Thresholds.AddEffect(effect, "name", comp);

            system.Process();
            system.Results["health"].Should().Be(200);

            system.Inputs["name"] = "Mike";
            system.Process();
            system.Results["health"].Should().Be(100);
        }

        [TestMethod]
        public void AddConfig()
        {
            var system = new EffectSystem();
            
            system.CreateConfig("energy_shield", 100)
                .AddGroup("increased", EffectOp.Mul, EffectOp.Add)
                .AddGroup("more", EffectOp.Mul, EffectOp.Mul)
                .AddConstantEffect(1, "increased")
                .AddConstantEffect(0.2, "increased")
                .AddConstantEffect(0.3, "increased")
                .AddConstantEffect(1.1, "more")
                .AddConstantEffect(1.2, "more")
                .Add();

            system.Process();
            system.Results["energy_shield"].Should().Be(198);
        }

        [TestMethod]
        public void ApplyPropertyConfig()
        {
            var system = new EffectSystem();
            var config = new PropertyConfig("health");

            config.StartValue = 100;
            config.MaxValue = 50;
            config.Position = 1;
            config.Apply(system);
            system.Process();

            system.Results["health"].Should().Be(50);
        }

        [TestMethod]
        public void InputReadingResults()
        {
            var system = new EffectSystem();
            var config1 = new PropertyConfig("strength");
            config1.StartValue = 500;
            config1.Apply(system);
            var config2 = new PropertyConfig("health");
            config2.StartValue = 100;
            config2.Apply(system);

            Func<InputVector, double> f = (inputs) =>
                inputs.PropertyValue("strength") / 2;

            var effect = new InputEffect("health", f, "add");
            system.AddEffect(effect);

            system.Process();
            system.Results["health"].Should().Be(350);
        }

        [TestMethod]
        public void PipelineOrder()
        {
            var system = new EffectSystem();
            var config2 = new PropertyConfig("health");
            config2.StartValue = 100;
            config2.Position = 1;
            config2.Apply(system);

            var config1 = new PropertyConfig("strength");
            config1.StartValue = 400;
            config1.Position = 0;
            config1.Apply(system);

            Func<InputVector, double> f = (inputs) =>
                inputs.PropertyValue("strength") / 2;

            var effect1 = new InputEffect("health", f, "add");
            var effect2 = new ConstantEffect("strength", 2, "mul");
            system.AddEffect(effect1);
            system.AddEffect(effect2);

            system.Process();
            system.Results["health"].Should().Be(500);
        }

        [TestMethod]
        public void TemporaryEffects()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            system.AddTempEffect(new ConstantEffect("health", 50, "add"));

            system.Process();
            system.Results["health"].Should().Be(150);

            system.Process();
            system.Results["health"].Should().Be(100);
        }

        [TestMethod]
        public void DataEffectFunction()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            system.Inputs.TryAddValue("time", 10.0);

            Func<InputVector, Dictionary<string, object>, double> f = (inputs, data) =>
            {
                return ((double)inputs["time"] - (double)data["start_time"]) * 2.0;
            };

            var effect = new DataEffect("health", f, "add");
            effect.Data.Add("start_time", 3.0);
            system.AddEffect(effect);

            system.Process();

            system.Results["health"].Should().Be(114);
        }

        [TestMethod]
        public void DataEffectCtorDtor()
        {
            var system = new EffectSystem();
            system.Properties.Add("health", 100);
            system.Inputs.TryAddValue("time", 10.0);

            Func<InputVector, Dictionary<string, object>, double> f = (inputs, data) =>
            {
                return ((double)inputs["time"] - (double)data["start_time"]) * 2.0;
            };

            Action<InputVector, Dictionary<string, object>> ctor = (inputs, data) =>
            {
                data["start_time"] = 3.0;
            };

            Action<InputVector, Dictionary<string, object>> dtor = (inputs, data) =>
            {
                inputs["time"] = 0.0;
            };

            var effect = new DataEffect("health", f, ctor, dtor, "add");
            effect.Data.Add("start_time", 3.0);
            system.AddEffect(effect);

            system.Process();
            system.RemoveEffect(effect);

            system.Results["health"].Should().Be(114);
            system.Inputs["time"].Should().Be(0);
        }
    }
}