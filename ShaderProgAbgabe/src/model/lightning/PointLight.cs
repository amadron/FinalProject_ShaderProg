using Example.src.model.lightning;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace Example.src.model
{
    class PointLight : Light
    {
        public PointLight() : base()
        {
            this.radius = 1;
        }

        public PointLight(Vector3 position, Vector4 color, float radius, float intensity ) : base(position, color, intensity)
        {
            this.radius = radius;
        }

        public PointLight(Vector3 position, Vector4 color, float radius, float intensity, Vector4 specularColor, int specularFactor, float specularIntensity) : 
            base(position, color, intensity, specularColor, specularFactor, specularIntensity)
        {
            this.radius = radius;
        }

        public float radius;

    }
}
