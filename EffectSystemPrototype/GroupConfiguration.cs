namespace EffectSystemPrototype
{
    public class GroupConfiguration
    {
        private readonly EffectSystem _effectSystem;
        private readonly List<(string Name, PipelineGroup)> _pipelineGroups = new();
        private readonly List<Effect> _effects = new();

        public string Property { get; }
        public double Value { get; }

        public Effect[] Effects => _effects.ToArray();

        public (string Name, PipelineGroup PipelineGroup)[] PipelineGroups => _pipelineGroups.ToArray();

        public GroupConfiguration(string property, double value, EffectSystem effectSystem)
        {
            _effectSystem = effectSystem;
            Property = property;
            Value = value;
        }

        public GroupConfiguration AddGroup(string name, EffectOp baseOp, EffectOp effectOp)
        {
            _pipelineGroups.Add(( name, new PipelineGroup(baseOp, effectOp)));
            return this;
        }
        
        public GroupConfiguration AddEffect(Effect effect)
        {
            _effects.Add(effect);
            return this;
        }

        public GroupConfiguration AddConstantEffect(double value, string group)
        {
            _effects.Add(new ConstantEffect(Property, value, group));
            return this;
        }

        public GroupConfiguration AddInputEffect(string group, Func<InputVector, double> function)
        {
            _effects.Add(new InputEffect(Property, function, group));
            return this;
        }

        public GroupConfiguration AddMetaEffect(Func<InputVector, Effect[]> function)
        {
            _effects.Add(new MetaEffect(function));
            return this;
        }

        public void Add()
        {
            _effectSystem.InternalAddGroupConfig(this);
        }
        public GroupConfiguration AddPostProcessGroup(string name)
        {
            throw new NotImplementedException();
        }
    }
}
