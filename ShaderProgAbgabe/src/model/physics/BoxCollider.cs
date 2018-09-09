
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Zenseless.Geometry;

namespace Example.src.model.physics
{
    class BoxCollider
    {
        public BoxCollider()
        {
            this.position = Vector3.Zero;
        }
        public BoxCollider(Vector3 position)
        {
            this.position = position;
        }

        public BoxCollider(Vector3 position, float sizeX, float sizeY, float sizeZ)
        {
            this.position = position;
            SetSizeX(sizeX);
            SetSizeY(sizeY);
            SetSizeZ(sizeZ);
        }

        public void SetPosition(Vector3 position)
        {
            this.position = position;
            SetSizes(sizeX, sizeY, sizeZ);
        }

        public void SetSizes(float sizeX, float sizeY, float sizeZ)
        {
            SetSizeX(sizeX);
            SetSizeY(sizeY);
            SetSizeZ(sizeZ);
        }

        public void SetSizeX(float sizeX)
        {
            this.sizeX = sizeX;
            float half = sizeX / 2;
            this.minBounds.X = position.X - half;
            this.maxBounds.X = position.X + half;
        }

        public void SetSizeY(float sizeY)
        {
            this.sizeY = sizeY;
            float half = sizeY / 2;
            this.minBounds.Y = position.Y - half;
            this.maxBounds.Y = position.Y + half;
        }

        public void SetSizeZ(float sizeZ)
        {
            this.sizeZ = sizeZ;
            float half = sizeZ / 2;
            this.minBounds.Z = position.Z - half;
            this.maxBounds.Z = position.Z + half;
        }

        public Vector3 GetMinBounds()
        {
            return minBounds;
        }

        public Vector3 GetMaxBounds()
        {
            return maxBounds;
        }

        public float sizeX;
        public float sizeY;
        public float sizeZ;

        public Vector3 minBounds;
        public Vector3 maxBounds;

        public Vector3 position;
    }
}
