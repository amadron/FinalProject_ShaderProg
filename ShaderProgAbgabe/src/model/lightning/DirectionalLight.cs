using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.lightning
{
    class DirectionalLight : Light
    {
        public DirectionalLight() : base()
        {
            direction = new Vector3(0, 1, 0);
        }

        public DirectionalLight(Vector4 color, Vector3 direction, float intensity) : base(Vector3.Zero, color, intensity)
        {
            this.direction = direction;
        }
        
        public DirectionalLight(Vector4 color, Vector3 direction, float intensity, Vector4 specularColor, int specularFactor, float specularIntensity) :
            base(Vector3.Zero, color, intensity, specularColor, specularFactor, specularIntensity)
        {
            this.direction = direction;
        }

        public Vector3 direction;
    }
}
