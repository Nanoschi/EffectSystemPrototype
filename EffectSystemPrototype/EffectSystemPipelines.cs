using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectSystemPrototype
{
    public class EffectSystemPipelines
    {
        public SortedList<int, Pipeline> Pipelines { get; } = new();
        public Dictionary<string, int> Positions { get; } = new();

        public int Count => Pipelines.Count;
        public int MaxPosition => Pipelines.Keys[Pipelines.Count - 1];

        private void AddAutoPos(string property, Pipeline pipeline)
        {
            if (Pipelines.Count > 0)
            {
                int maxPos = Pipelines.Keys[Pipelines.Count - 1];
                Add(property, pipeline, maxPos + 1);
            }
            else
            {
                Add(property, pipeline, 0);
            }
        }

        internal void Add(string property, Pipeline pipeline, int position = -1)
        {
            if (position < 0)
            {
                AddAutoPos(property, pipeline);
            }
            else
            {
                Positions[property] = position;
                Pipelines[position] = pipeline;
            }

        }

        internal bool Remove(string property)
        {
            int position = Positions[property];
            Positions.Remove(property);
            return Pipelines.Remove(position);
        }

        public void SetPosition(string property, int position)
        {
            if (Pipelines.ContainsKey(position))
            {
                throw new ArgumentException($"Pipeline with position '{position}' already exists.");
            }
            else
            {
                if (Positions.TryGetValue(property, out int currentPos))
                {
                    Positions[property] = position;
                    var pipeline = Pipelines[currentPos];
                    Pipelines.Remove(currentPos);
                    Pipelines[position] = pipeline;
                }
                else
                {
                    throw new ArgumentException($"Property '{property}' not found.");
                }
            }
        }


        public Pipeline this[string property]
        {
            get
            {
                int pos = Positions[property];
                return Pipelines[pos];
            }
            set => Add(property, value);
        }


        public void ClearGeneratedEffects(InputVector inputs)
        {
            foreach (var pipeline in Pipelines.Values)
            {
                pipeline.ClearGeneratedEffects(inputs);
            }
        }
    }
}
