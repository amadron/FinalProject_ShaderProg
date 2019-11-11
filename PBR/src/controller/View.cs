using PBR.src.controller;
using PBR.src.model.rendering;
using System.Numerics;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace PBR
{
    class View
    {
        PBRRenderer renderer;
        VAO geometry;
        PBRMaterial mat;
        
        public View(IRenderState renderState, IContentLoader contentLoader)
        {
            renderer = new PBRRenderer(renderState, contentLoader);
            DefaultMesh sphere = Meshes.CreateSphere(1, 10);
            //sphere.Transform(Transformation.Rotation(45, Axis.X)).Transform(Transformation.Translation(new Vector3(0,0,-1)));
            geometry = VAOLoader.FromMesh(sphere, renderer.GetShader());
            mat = new PBRMaterial();
        }

        public void Render()
        {
            renderer.Render(geometry, mat);
        }
    }
}
