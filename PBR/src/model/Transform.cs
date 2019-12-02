using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PBR.src.model
{
    class Transform
    {
        public Vector3 position;
        public Vector3 scale;
        public Vector3 rotation;

        public Transform()
        {
            position = Vector3.Zero;
            scale = Vector3.One;
            rotation = Vector3.Zero;
        }

        public Matrix4x4 modelMatrix {
                get => GetModelMatrix();
            }

        private Matrix4x4 GetModelMatrix()
        {
            return GetScaleMatrix() * GetTranslationMatrix();
        }

        private Matrix4x4 GetTranslationMatrix()
        {
            return Matrix4x4.CreateTranslation(-position);
        }

        private Matrix4x4 GetScaleMatrix()
        {
            return Matrix4x4.CreateScale(scale);
        }

        //Havent tested, if Rotation is properly
        //ToDo:Replace by Quaternion
        private Matrix4x4 GetRotationMatrix()
        {
            Matrix4x4 zRot = Matrix4x4.CreateRotationZ(rotation.Z);
            Matrix4x4 yRot = Matrix4x4.CreateRotationY(rotation.Y);
            Matrix4x4 xRot = Matrix4x4.CreateRotationX(rotation.X);
            return zRot * xRot * yRot;
        }
    }
}
