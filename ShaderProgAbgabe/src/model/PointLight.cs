using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace Example.src.model
{
    class PointLight
    {
        public PointLight()
        {
            position = Vector3.Zero;
            lightColor = Color.White;
            radius = 1;
        }

        public PointLight(Vector3 position, Color color, float radius )
        {
            this.position = position;
            this.lightColor = color;
            this.radius = radius;
        }

        public Vector3 position;
        public Color lightColor;
        public float radius;
    }
}
