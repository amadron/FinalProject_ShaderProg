using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.camera
{
    abstract class Camera
    {

        public abstract Matrix4x4 GetMatrix();

        public Vector3 position;
        public Vector3 rotation;
        public float clippingPlaneNear;
        public float clippingPlaneFar;

    }
}
