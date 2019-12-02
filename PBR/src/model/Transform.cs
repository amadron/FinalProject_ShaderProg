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

        public Matrix4x4 GetModelMatrix()
        {
            return GetRotationMatrix() * GetScaleMatrix() * GetTranslationMatrix();
        }

        public Matrix4x4 GetTranslationMatrix()
        {
            return Matrix4x4.CreateTranslation(-position);
        }

        private Vector3 GetRotatedVector(Vector3 vector)
        {
            Vector4 rotated = new Vector4(vector, 1);
            Matrix4x4 rotationMatrix = GetRotationMatrix();
            rotated = Vector4.Transform(rotated, rotationMatrix);
            return new Vector3(rotated.X, rotated.Y, rotated.Z);
        }

        public Matrix4x4 GetScaleMatrix()
        {
            return Matrix4x4.CreateScale(scale);
        }

        //Havent tested, if Rotation is properly
        //ToDo:Replace by Quaternion
        public Matrix4x4 GetRotationMatrix()
        {
            return Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        }

        /*
        private Quaternion GetQuaternionRotation()
        {
            Quaternion xQuat = new Quaternion(new Vector3(1, 0, 0), rotation.X);
            Quaternion yQuat = new Quaternion(new Vector3(0, 1, 0), rotation.Y);
            Quaternion zQuat = new Quaternion(new Vector3(0, 0, 1), rotation.Z);
        }
        */
    }
}
