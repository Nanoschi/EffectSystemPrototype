namespace EffectSystemPrototype.TestApp;


internal class Program
{
    public static void Main()
    {

        //new SomeFeaturesRunner().Run();
        new PerformanceRunner().Run();
    }

}

public class PerformanceRunner
{
    public void Run()
    {
        var system = new EffectSystem();
        var amount = 100.000;
        for (int i = 0; i < amount; i++)
        {
            system.CreateConfig($"Prop_{i}", i)
                .AddGroup($"group_mul_{i}", EffectOp.Mul, EffectOp.Mul)
                .AddGroup($"group_add_{i}", EffectOp.Mul, EffectOp.Add)
                .AddConstantEffect(i, $"group_mul_{i}")
                .AddConstantEffect(i, $"group_add_{i}")
                .Add();
        }

        //for (int i = 0; i < 10; i++)
        //{
        //    system.Process();
        //    var res = system.Results[$"Prop_{i}"];
        //}

        for (int i = 0; i < amount; i++)
        {
            system.Process();
            var res = system.Results[$"Prop_{i}"];
        }

    }
}