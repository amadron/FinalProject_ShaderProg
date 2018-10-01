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
            Quaternion def = Quaternion.Identity;
            rotation = new Vector4(def.X, def.Y, def.Z, def.W);
            scale = Vector3.One;
        }
        public Vector3 position;
        public Vector4 rotation;
        public Vector3 scale;
    }
}
