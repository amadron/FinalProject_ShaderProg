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

        public Vector3 forward
        {
            get => GetForwardVector();
        }
        public Vector3 up
        {
            get => GetRotatedVector(new Vector3(0, 1, 0));
        }
        public Vector3 right
        {
            get => GetRotatedVector(new Vector3(1, 0, 0));
        }

        public Transform()
        {
            position = Vector3.Zero;
            scale = Vector3.One;
            rotation = Vector3.Zero;
        }

        Vector3 GetForwardVector()
        {
            Matrix4x4 inverted = Matrix4x4.Identity;
            Matrix4x4.Invert(GetModelMatrix(), out inverted);
            //inverted = Matrix4x4.Transpose(inverted);
            Vector3 forward = new Vector3(inverted.M12, inverted.M22, inverted.M32);
            return Vector3.Normalize(forward);
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
            Matrix4x4 zRot = Matrix4x4.CreateRotationZ(rotation.Z);
            Matrix4x4 yRot = Matrix4x4.CreateRotationY(rotation.Y);
            Matrix4x4 xRot = Matrix4x4.CreateRotationX(rotation.X);
            return zRot * yRot * xRot;
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
