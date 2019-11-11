using Example.src.util;
using System.Numerics;
using Zenseless.Geometry;

namespace Example.src.model.graphics.camera
{
    public class FirstPersonCamera : Camera
    {
        ProjectionType projectionType;
        CameraFirstPerson cam;
        Matrix4x4 projection;
        float zPlaneNear;
        float zPlaneFar;
        float fov;
        float aspectRatio;
        float width;
        float height;
        public FirstPersonCamera(Vector3 position, float rotX, float rotY, ProjectionType projectionType, float zPlaneFar = 50.0f, float zPlaneNear = 0.1f, float fov = 0.6f, float aspectRatio = 1, float width = 1, float height = 1)
        {
            cam = new CameraFirstPerson();
            cam.Position = position;
            cam.Heading = rotY;
            cam.Tilt = rotX;
            cam.NearClip = zPlaneNear;
            cam.FarClip = zPlaneFar;
            this.fov = fov;
            this.aspectRatio = aspectRatio;
            this.width = width;
            this.height = height;
            this.zPlaneNear = zPlaneNear;
            this.zPlaneFar = zPlaneFar;
            this.projectionType = projectionType;

            InitProjection();
        }

        private void InitProjection()
        {
            if (projectionType == ProjectionType.Perspective)
            {
                projection = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, zPlaneNear, zPlaneFar);
            }
            else
            {
                projection = Matrix4x4.CreateOrthographic(width, height, zPlaneNear, zPlaneFar);
            }
        }

        public override Matrix4x4 GetMatrix()
        {
            return Matrix4x4.Transpose(projection) * cam.CalcViewMatrix() ;
        }

        public override Vector3 GetPosition()
        {
            return cam.Position;
        }

        public override Vector3 GetDirection()
        {
            Vector4 forward = new Vector4(0, 0, 1, 1);
            Matrix4x4 rot = GetRotationMatrix();
            rot = Matrix4x4.Transpose(rot);
            forward = MathUtil.Transform(forward, rot);
            forward = Vector4.Normalize(forward);
            return new Vector3(forward.X, forward.Y, forward.Z);
        }

        public override Matrix4x4 GetRotationMatrix()
        {
            return cam.CalcRotationMatrix();
        }

        public override void SetPosition(Vector3 position)
        {
            cam.Position = position;
        }

        public override void SetRotation(Vector3 rotation)
        {
            cam.Tilt = rotation.X;
            cam.Heading = rotation.Y;
        }

        public override void Resize(int width, int height)
        {
            aspectRatio = (float)width/(float)height;
            InitProjection();
        }

        public override Matrix4x4 GetViewMatrix()
        {
            return cam.CalcViewMatrix();
        }
    }
}
