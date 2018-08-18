using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.lightning
{
    class Light
    {
        public Light()
        {
            position = Vector3.Zero;
            lightColor = Vector4.One;
            intensity = 1;
        }

        public Light(Vector3 position, Vector4 color, float intensity)
        {
            this.position = position;
            this.lightColor = color;
            this.intensity = intensity;
        }

        public Light(Vector3 position, Vector4 color, float intensity, Vector4 specularColor, float specularFactor, float specularIntensity)
        {
            this.position = position;
            this.lightColor = color;
            this.intensity = intensity;
            this.specularColor = specularColor;
            this.specularFactor = specularFactor;
            this.specularIntensity = specularIntensity;
        }

        public Vector3 position;
        public Vector4 lightColor;
        public float intensity;
        public Vector4 specularColor = Vector4.One;
        public float specularFactor = 255;
        public float specularIntensity = 0;
    }
}
