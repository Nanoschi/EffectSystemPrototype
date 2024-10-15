using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectSystemPrototype
{
    public class Pipelines
    {
        public SortedList<int, Pipeline> PipelinesList { get; } = new();
        public Dictionary<string, int> Positions { get; } = new();

        public int Count => PipelinesList.Count;
        public int MaxPosition => PipelinesList.Keys[PipelinesList.Count - 1];

        private void AddAutoPos(string property, Pipeline pipeline)
        {
            if (PipelinesList.Count > 0)
            {
                int maxPos = PipelinesList.Keys[PipelinesList.Count - 1];
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
                PipelinesList[position] = pipeline;
            }

        }

        internal bool Remove(string property)
        {
            int position = Positions[property];
            Positions.Remove(property);
            return PipelinesList.Remove(position);
        }

        public void SetPosition(string property, int position)
        {
            if (PipelinesList.ContainsKey(position))
            {
                throw new ArgumentException($"Pipeline with position '{position}' already exists.");
            }
            else
            {
                if (Positions.TryGetValue(property, out int currentPos))
                {
                    Positions[property] = position;
                    var pipeline = PipelinesList[currentPos];
                    PipelinesList.Remove(currentPos);
                    PipelinesList[position] = pipeline;
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
                return PipelinesList[pos];
            }
            set => Add(property, value);
        }


        public void ClearTemporaryEffects(InputVector inputs)
        {
            foreach (var pipeline in PipelinesList.Values)
            {
                pipeline.ClearTemporaryEffects(inputs);
            }
        }
    }
}
