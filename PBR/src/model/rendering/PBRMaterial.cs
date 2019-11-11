using System.Numerics;
using System;
using Zenseless.ExampleFramework;
using Zenseless.HLGL;

namespace PBR.src.model.rendering
{
    class PBRMaterial
    {
        public PBRMaterial()
        {
            albedoColor = new Vector3(1);
            roughness = 1;
            metal = 1;
            ao = 1;
        }

        public Vector3 albedoColor;
        public float roughness;
        public float metal;
        public float ao;
    }
}
