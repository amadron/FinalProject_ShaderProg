using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.util
{
    struct Range
    {
        public Range(float value)
        {
            min = value;
            max = value;
            range = 0;
        }

        public Range(float min, float max)
        {
            this.min = min;
            this.max = max;
            range = max - min;
        }

        public float min;
        public float max;
        private float range;

        public float GetRandomValue(Random random)
        {
            return min + (float) random.NextDouble() * range;
        }
    }
}
