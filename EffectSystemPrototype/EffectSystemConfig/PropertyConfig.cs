namespace EffectSystemPrototype
{
    public class PropertyConfig
    {
        public string Name { get; set; }
        public double StartValue { get; set; } = 0;
        public double MinValue { get; set; } = double.NegativeInfinity;
        public double MaxValue { get; set; } = double.PositiveInfinity;
        public int Position { get; set; } = -1;
        public PropertyConfig(string name)
        {
            Name = name;
        }

        public void Apply(EffectSystem system)
        {
            system.Properties.Add(Name, StartValue);
            system.Ranges.SetMinMaxValue(Name, MinValue, MaxValue);
            if (Position >= 0)
            {
                system.Pipelines.SetPosition(Name, Position);
            }
        }
    }
}
