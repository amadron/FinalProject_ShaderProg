using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenseless.HLGL;

namespace Example.src.model.graphics.rendering
{
    class Renderable
    {
        public int hasAlbedoTexture;
        public ITexture2D albedoTexture;
        public int hasNormalMap;
        public ITexture2D normalMap;

        public IDrawable mesh;

        public Renderable()
        {
            hasAlbedoTexture = 0;
            albedoTexture = null;
            hasNormalMap = 0;
            normalMap = null;
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

        
    }
}
