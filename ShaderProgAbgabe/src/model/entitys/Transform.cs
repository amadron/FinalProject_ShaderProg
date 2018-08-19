using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.entitys
{
    class Transform
    {
        public Transform()
        {
            position = Vector3.Zero;
            rotation = Vector3.Zero;
            scale = Vector3.One;
        }
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }
}
