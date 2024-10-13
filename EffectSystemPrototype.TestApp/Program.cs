using System.Diagnostics;

namespace EffectSystemPrototype.TestApp;


internal class Program
{
    public static void Main()
    {

        PerformanceRunner.Run(100_000, 10);
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
        Console.WriteLine($"Property setup elapsed time: {timer.Elapsed.Milliseconds}ms");

        timer.Restart();
        for (int i = 0; i < processCount; i++)
        {
            system.Process();
        }
        timer.Stop();
        Console.WriteLine($"Process elapsed time: {timer.Elapsed.Milliseconds}ms");
    }
}