using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace Example.src.model.entitys
{
    class Terrain : Entity
    {
        public Terrain(IContentLoader contentLoader, Vector2 size, Vector2 segments):base()
        {
            shader = contentLoader.Load<IShaderProgram>("terrain.*");
            var geom = Meshes.CreatePlane(size.X, size.Y, (uint) segments.X, (uint) segments.Y);
            mesh = VAOLoader.FromMesh(geom, shader);
        }
        
        public IDrawable mesh;
        public ITexture2D color;
        public ITexture2D heightMap;

        public IShaderProgram shader;
    }
}
