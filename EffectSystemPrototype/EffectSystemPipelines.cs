using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectSystemPrototype
{
    internal class EffectSystemPipelines
    {
        public SortedList<int, Pipeline> Pipelines { get; private set; } = new();
        public Dictionary<string, int> Positions { get; private set; } = new();

        public int Count {  get { return Pipelines.Count; } }
        public int MaxPosition { get { return Pipelines.GetKeyAtIndex(Pipelines.Count - 1); } }

        public void Add(string property, Pipeline pipeline)
        {
            if (Pipelines.Count > 0)
            {
                int maxPos = Pipelines.GetKeyAtIndex(Pipelines.Count - 1);
                Add(property, pipeline, maxPos + 1);
            }
            else
            {
                Add(property, pipeline, 0);
            }
        }

        public void Add(string property, Pipeline pipeline, int position)
        {
            Positions[property] = position;
            Pipelines[position] = pipeline;
        }

        public bool Remove(string property)
        {
            int position = Positions[property];
            Positions.Remove(property);
            return Pipelines.Remove(position);
        }


        public Pipeline this[string property]
        {
            get
            {
                int pos = Positions[property];
                return Pipelines[pos];
            }
            set
            {
                Add(property, value);
            }
        }


        public EffectSystemPipelines Copy()
        {
            EffectSystemPipelines copy = new();
            copy.Positions = Positions.ToDictionary();
            foreach (var posPipe in Pipelines)
            {
                copy.Pipelines[posPipe.Key] = posPipe.Value.Copy();
            }
            return copy;
        }
    }
}
