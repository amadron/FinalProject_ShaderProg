using System.Numerics;
using System;
using Zenseless.ExampleFramework;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace PBR.src.model.rendering
{
    class PBRMaterial
    {
        public PBRMaterial()
        {
            albedoColor = new Vector3(1);
            albedoMap = null;
            normalMap = null;
            roughness = 1;
            roughnessMap = null;
            metal = 1;
            metallicMap = null;
            ao = 1;
            occlusionMap = null;
        }

        public Vector3 albedoColor;
        public ITexture2D albedoMap;
        public ITexture2D normalMap;
        public float roughness;
        public ITexture2D roughnessMap;
        public float metal;
        public ITexture2D metallicMap;
        public float ao;
        public ITexture2D occlusionMap;
    }
}
