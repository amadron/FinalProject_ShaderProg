using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.camera
{
    class OrthographicCamera : Camera
    {
        
        public OrthographicCamera(Vector3 position, Vector3 rotation ,float width = 60, float height = 60, float clippingNear = 0.1f, float clippingFar = 50)
        {
            this.position = position;
            this.width = width;
            this.height = width / 16 * 9;
            //this.height = height;
            this.clippingPlaneNear = clippingNear;
            this.clippingPlaneFar = clippingFar;
        }

        
        public override Matrix4x4 GetMatrix()
        {
            Matrix4x4 translation = Matrix4x4.CreateTranslation(-position);
            Matrix4x4 rotX = Matrix4x4.CreateRotationX(-rotation.X * (float) (Math.PI/180));
            Matrix4x4 rotY = Matrix4x4.CreateRotationY(-rotation.Y * (float) (Math.PI/180));
            Matrix4x4 rotZ = Matrix4x4.CreateRotationZ(-rotation.Z * (float) (Math.PI/180));
            Matrix4x4 camMat = Matrix4x4.CreateOrthographic(width, height, clippingPlaneNear, clippingPlaneFar);
            Matrix4x4 rot = rotX * rotY * rotZ;
            return  translation *  rot * camMat;// * rotX * rotY * rotZ;
        }

        private float width;
        private float height;
    }
}
