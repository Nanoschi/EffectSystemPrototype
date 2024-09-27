class FlaskBelt
{
    public Effect[][]? flasks;
}


class Program
{
    public static void Main()
    {
        EffectSystem system = new EffectSystem();
        system.AddProperty("health", 100);
        system.AddProperty("mana", 100);
        system.AddProperty("speed", 1);
        system.AddProperty("fire_res", 0);
        system.AddProperty("lightning_res", 0);
        system.AddProperty("cold_res", 0);
        system.AddProperty("armour", 0);

        FlaskBelt belt = new();
        belt.flasks = new Effect[][] {
            new Effect[] { new ConstantEffect("speed", 1.4, EffectOp.Mul) }, // Quicksilver
            new Effect[] { 
            new ConstantEffect("fire_res", 0.35, EffectOp.Add),
            new ConstantEffect("lightning_res", 0.35, EffectOp.Add),
            new ConstantEffect("cold_res", 0.35, EffectOp.Add)
            }, // Bismuth
            new Effect[] { new ConstantEffect("armour", 1500, EffectOp.Add) }, // Granite
        };


        system.SetInput("int", 266);
        system.SetInput("belt", belt);

        Effect[] effects = // Alle aktiven Effekte
        {
        new ConstantEffect("health", 50, EffectOp.Add, 1),
        new ConstantEffect("health", 2, EffectOp.Mul),
        new ConstantEffect("mana", 2, EffectOp.Mul),
        new MetaEffect(MagebloodEffect), // Fügt die Effekte der ersten drei Flasks in belt hinzu
        new MetaEffect(IntManaEffect), // Fügt 1 Mana pro 2 int hinzu
        };

        foreach (Effect effect in effects)
        {
            system.AddEffect(effect);
        }
        system.IncreaseTime(1.5); // health + 50 Effekt wird entfernt
        system.RemoveEffect(effects[2]); // health * 2 Effekt wird entfernt
        system.Process();

        foreach (var kv in system.ProcessedProperties)
        {
            Console.WriteLine($"{kv.Key} => Base: {system.BaseProperties[kv.Key]}, Processed: {kv.Value}");
        }
    }

    static Effect[] MagebloodEffect(Dictionary<string, object> inputs)
    {
        if (inputs.ContainsKey("belt")) {
            FlaskBelt belt = inputs["belt"] as FlaskBelt;
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

    static Effect[] IntManaEffect(Dictionary<string, object> inputs)
    {
        if (inputs.ContainsKey("int"))
        {
            int intelligence = (int)inputs["int"];
            return new Effect[] { new ConstantEffect("mana", intelligence / 2, EffectOp.Add) };
        }
        return new Effect[] { };
    }

}
