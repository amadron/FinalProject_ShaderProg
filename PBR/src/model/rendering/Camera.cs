using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTK.Graphics.OpenGL;
using System.Text;
using System.Threading.Tasks;

namespace PBR.src.model.rendering
{
    class Camera
    {
        public enum ProjectionMode { Ortogonal, Perspective }
        public float clippingNear = 0.1f;
        public float clippingFar = 1000.0f;
        public Transform transform = new Transform();
        public float fov = 60f;
        public float aspectRatio = 1;
        public float orthographicWidth = 1;
        public float orthographicHeight = 1;
        public ProjectionMode projectionMode = ProjectionMode.Perspective;

        public Matrix4x4 Matrix
        {
            get => GetMatrix();
        }

        public Matrix4x4 GetRotationMatrix()
        {
            Matrix4x4 rotationX = Matrix4x4.CreateRotationX(transform.rotation.X);
            Matrix4x4 rotationY = Matrix4x4.CreateRotationY(transform.rotation.Y);
            Matrix4x4 rotationZ = Matrix4x4.CreateRotationZ(transform.rotation.Z);
            return rotationZ * rotationY * rotationX;
        }

        public Matrix4x4 GetTranslationMatrix()
        {
            return Matrix4x4.CreateTranslation(-transform.position);
        }

        public Matrix4x4 GetTransformationMatrix()
        {
            return transform.GetTranslationMatrix() * GetRotationMatrix();
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            if(projectionMode == ProjectionMode.Perspective)
            {
                float fovRad = (float)Math.PI * fov / 180.0f;
                return Matrix4x4.CreatePerspectiveFieldOfView(fovRad, aspectRatio, clippingNear, clippingFar);
            }
            else
            {
                return Matrix4x4.CreateOrthographic(orthographicWidth, orthographicHeight, clippingNear, clippingFar);
            }
        }

        public Matrix4x4 GetMatrix()
        {
            return GetTransformationMatrix() * GetProjectionMatrix();
        }
    }
}
