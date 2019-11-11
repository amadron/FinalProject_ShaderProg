using OpenTK.Graphics.OpenGL;
using Example.src.model.graphics.camera;
using Zenseless.Geometry;
using PBR.src.model.rendering;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using System.Numerics;

namespace PBR.src.controller
{
    class PBRRenderer
    {
        IShaderProgram pbrShader;
        public PBRRenderer(IRenderState renderState, IContentLoader contentLoader)
        {
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.BACK_SIDE));
            pbrShader = contentLoader.Load<IShaderProgram>("PBR.*");
        }

        public IShaderProgram GetShader()
        {
            return pbrShader;
        }

        public void Render(Matrix4x4 camMat,VAO geometry,PBRMaterial material)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            pbrShader.Uniform("albedo_Color", material.albedoColor);
            pbrShader.Uniform("rougness", material.roughness);
            pbrShader.Uniform("metal", material.metal);
            pbrShader.Uniform("ao", material.ao);
            Matrix4x4 mat = camMat;
            pbrShader.Uniform("cameraMatrix", mat);

            pbrShader.Activate();
            geometry.Draw();
            pbrShader.Deactivate();
        }
    }
}
