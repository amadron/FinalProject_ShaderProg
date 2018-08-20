using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.controller.util
{
    struct AspectRatio3D
    {
        public enum Axis { XAxis, YAxis, ZAxis };
        Axis axis;
        public AspectRatio3D(Axis axis, float ratio)
        {
            this.axis = axis;
            this.ratio = ratio;
        }
        
        public Vector3 GetAspectRatio(Vector3 vec)
        {
            if (axis == Axis.XAxis)
            {
              vec.Y = vec.Z = vec.X * ratio;
              
            }
            else if(axis == Axis.YAxis)
            {
              vec.X = vec.Z = vec.Y * ratio;
            }
            else
            {
                vec.X = vec.Y = vec.Z * ratio;
            }
            return vec;
        }

        float ratio;
    }
}
