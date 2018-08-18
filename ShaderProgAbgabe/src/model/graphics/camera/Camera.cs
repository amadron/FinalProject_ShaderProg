using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.graphics.camera
{
    public abstract class Camera
    {
        public enum ProjectionType { Perspective, Orthographic };
        public abstract Matrix4x4 GetMatrix();
        public abstract Matrix4x4 GetRotationMatrix();
        public abstract Vector3 GetDirection();
        public abstract Vector3 GetPosition();
        public abstract void SetPosition(Vector3 position);
        public abstract void SetRotation(Vector3 rotation);
        public abstract void Resize(int widht, int height);
    }
}
