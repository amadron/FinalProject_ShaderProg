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
            intensity = 1;
        }

        public PointLight(Vector3 position, Color color, float radius, float intensity )
        {
            this.position = position;
            this.lightColor = color;
            this.radius = radius;
            this.intensity = intensity;
        }

        public PointLight(Vector3 position, Color color, float radius, float intensity, Color specularColor, int specularFactor, float specularIntensity)
        {
            this.position = position;
            this.lightColor = color;
            this.radius = radius;
            this.intensity = intensity;
            this.specularColor = specularColor;
            this.specularFactor = specularFactor;
            this.specularIntensity = specularIntensity;
        }

        public Vector3 position;
        public Color lightColor;
        public float radius;
        public float intensity;
        public Color specularColor = Color.White;
        public int specularFactor = 255;
        public float specularIntensity = 0;
    }
}
