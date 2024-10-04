namespace EffectSystemPrototype;

internal class FlaskBelt
{
    public Effect[][]? flasks;
}


internal class Program
{
    public static void Main()
    {
        
        EffectSystem system = new EffectSystem();
        system.Properties.Add("health", 100);
        system.Properties.Add("mana", 100);
        system.Properties.Add("speed", 1);
        system.Properties.Add("fire_res", 0);
        system.Properties.Add("lightning_res", 0);
        system.Properties.Add("cold_res", 0);
        system.Properties.Add("armour", 0);

        FlaskBelt belt = new();
        belt.flasks = new Effect[][] {
            new Effect[] { new ConstantEffect("speed", 1.4, "mul") }, // Quicksilver
            new Effect[] { 
                new ConstantEffect("fire_res", 0.35, "add"),
                new ConstantEffect("lightning_res", 0.35, "add"),
                new ConstantEffect("cold_res", 0.35, "add")
            }, // Bismuth
            new Effect[] { new ConstantEffect("armour", 1500, "add") }, // Granite
        };


        system.SetInput("int", 266);
        system.SetInput("belt", belt);

        Effect[] effects = // Alle aktiven Effekte
        {
            new ConstantEffect("health", 50, "add"),
            new ConstantEffect("health", 2, "mul"),
            new ConstantEffect("mana", 2, "mul"),
            new MetaEffect(MagebloodEffect), // Fügt die Effekte der ersten drei Flasks in belt hinzu
            new MetaEffect(IntManaEffect), // Fügt 1 Mana pro 2 int hinzu
        };

        foreach (Effect effect in effects)
        {
            system.AddEffect(effect);
        }

        system.RemoveEffect(effects[2]); // health * 2 Effekt wird entfernt
        system.Process();

        foreach (var kv in system.Results.properties)
        {
            Console.WriteLine($"{kv.Key} => Base: {system.Properties[kv.Key]}, Processed: {kv.Value.Value}");
        }
    }

    private static Effect[] MagebloodEffect(InputVector inputsVector)
    {
        if (inputsVector.Contains("belt")) {
            FlaskBelt belt = inputsVector["belt"] as FlaskBelt;
            List<Effect> effects = new List<Effect>();
            foreach (var flask in belt.flasks)
            {
                foreach(var effect in flask)
                {
                    effects.Add(effect);
                }
            }
            return effects.ToArray();
        }
        return new Effect[] { };
    }

    private static Effect[] IntManaEffect(InputVector inputsVector)
    {
        if (inputsVector.Contains("int"))
        {
            int intelligence = (int)inputsVector["int"];
            return new Effect[] { new ConstantEffect("mana", intelligence / 2, "add") };
        }
        return new Effect[] { };
    }

}