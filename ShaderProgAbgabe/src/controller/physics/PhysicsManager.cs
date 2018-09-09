using Example.src.model.physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.controller.physics
{
    class PhysicsManager
    {
        public static bool SphereIntercts(SphereCollider sphere, SphereCollider other)
        {
            double deltaX = sphere.position.X - other.position.X;
            deltaX *= deltaX;
            double deltaY = sphere.position.Y - other.position.Y;
            deltaY *= deltaY;
            double deltaZ = sphere.position.Z - other.position.Z;
            deltaZ *= deltaZ;
            double sum = deltaX + deltaZ + deltaY;

            double radi = sphere.radius + other.radius;
            radi *= radi;
            
            return sum <= radi;
        }

        public static bool BoxIntersects(BoxCollider box, BoxCollider other)
        {
            Vector3 minA = box.GetMinBounds();
            Vector3 minB = other.GetMinBounds();
            Vector3 maxA = box.GetMaxBounds();
            Vector3 maxB = other.GetMaxBounds();
            if(!TestBoxAxis(minA.X, minB.X, maxA.X, maxB.X))
            {
                return false;
            }
            else if(!TestBoxAxis(minA.Y, minB.Y, maxA.Y, maxB.Y))
            {
                return false;
            }
            else if(!TestBoxAxis(minA.Z, minB.Z, maxA.Z, maxB.Z))
            {
                return false;
            }
            else
            {
                return true;
            }
            
        }

        private static bool TestBoxAxis(float minA, float minB, float maxA, float maxB)
        {
            return (minA < maxB && maxA > minB);
        }
    }
}
