using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.util
{
    struct Range3D
    {
        public Range3D(Vector3 value)
        {
            min = value;
            max = value;
            range = Vector3.Zero;
        }

        public Range3D(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
            range = max - min;
        }

        public Vector3 min;
        public Vector3 max;
        private Vector3 range;

        public Vector3 GetRandomValue(Random random)
        {
            float x = min.X + (float)random.NextDouble() * range.X;
            float y = min.Y + (float)random.NextDouble() * range.Y;
            float z = min.Z + (float)random.NextDouble() * range.Z;
            return new Vector3(x,y,z);
        }
    }
}
