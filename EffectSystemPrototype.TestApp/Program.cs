using System.Diagnostics;

namespace EffectSystemPrototype.TestApp;


internal class Program
{
    public static void Main()
    {
        //new SomeFeaturesRunner().Run();
        PerformanceRunner.Run(100, 5, 1000);
    }

}

public class PerformanceRunner
{
    public static void Run(int propertyCount, int effectCount, int processCount = 1)
    {
        var system = new EffectSystem();

        Stopwatch timer = new Stopwatch();
        timer.Start();

        for (int i = 0; i < propertyCount; i++)
        {
            system.Properties.Add($"prop_{i}", 0);

            for (int j = 0; j <  effectCount; j++)
            {
                var effect = new ConstantEffect($"prop_{i}", 1, "add");
                system.AddEffect(effect);
            }
        }

        timer.Stop();
        Console.WriteLine($"Property setup elapsed time: {timer.ElapsedMilliseconds}ms");

        timer.Restart();
        for (int i = 0; i < processCount; i++)
        {
            system.Process();
        }
        timer.Stop();
        Console.WriteLine($"Process elapsed time: {timer.ElapsedMilliseconds}ms");
        Console.WriteLine($"Timer per Process: {timer.ElapsedMilliseconds / (double)processCount}ms");
    }
}