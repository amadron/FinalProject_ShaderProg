using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace Example.src.model.graphics.rendering
{
    class Renderable
    {
        public int hasAlbedoTexture;
        public ITexture2D albedoTexture;
        public int hasNormalMap;
        public ITexture2D normalMap;
        public int hasHeightMap;
        public ITexture2D heightMap;
        public float heightScaleFactor;
        public int hasAlphaMap;
        public ITexture2D alphaMap;
        public ITexture2D environmentMap;
        public float reflectivity;
        public int hasEnvironmentMap;
        public FaceCullingMode faceCullingMode;
        public int instances;

        private VAO mesh;
        private IShaderProgram shader;

        public Renderable()
        {
            hasAlbedoTexture = 0;
            albedoTexture = null;
            hasNormalMap = 0;
            normalMap = null;
            hasHeightMap = 0;
            heightMap = null;
            heightScaleFactor = 1;
            hasAlphaMap = 0;
            alphaMap = null;
            hasEnvironmentMap = 0;
            environmentMap = null;
            reflectivity = 0;
            faceCullingMode = FaceCullingMode.BACK_SIDE;
            instances = 0;
        }

        public void SetAlbedoTexture(ITexture2D texture)
        {
            albedoTexture = texture;
            hasAlbedoTexture = 1;
        }

        public void SetNormalMap(ITexture2D texture)
        {
            normalMap = texture;
            hasNormalMap = 1;
        }

        public void SetHeightMap(ITexture2D texture)
        {
            heightMap = texture;
            hasHeightMap = 1;
        }

        public void SetAlphaMap(ITexture2D texture)
        {
            alphaMap = texture;
            hasAlphaMap = 1;
        }

        public void SetEnvironmentMap(ITexture2D texture)
        {
            environmentMap = texture;
            hasEnvironmentMap = 1;
            reflectivity = 1;
        }


        public void SetMesh(VAO mesh, IShaderProgram shader)
        {
            this.mesh = mesh;
            this.shader = shader;
        }

        public VAO GetMesh()
        {
            return mesh;
        }

        public IShaderProgram GetShader()
        {
            return shader;
        }
    }
}
