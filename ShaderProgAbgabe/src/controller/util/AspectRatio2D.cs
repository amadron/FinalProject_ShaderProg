using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.controller.util
{
    struct AspectRatio2D
    {
        public enum Axis { XAxis, YAxis };
        Axis axis;
        public AspectRatio2D(Axis axis, float ratio)
        {
            this.axis = axis;
            this.ratio = ratio;
        }
        
        public Vector2 GetAspectRatio(Vector2 vec)
        {
            if (axis == Axis.XAxis)
            {
              vec.Y = vec.X * ratio;
            }
            else
            {
              vec.X = vec.Y * ratio;
            }
            return vec;
        }

        float ratio;
    }
}
