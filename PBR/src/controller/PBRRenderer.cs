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
            dLight = new DirectionalLight();
            dLight.direction = new Vector3(0, 1, 1);
            dLight.color = new Vector3(10);
            dLight.position = new Vector3(0, 1, -1);
        }

        public IShaderProgram GetShader()
        {
            return pbrShader;
        }

        DirectionalLight dLight;

        public void Render(Camera<Orbit, Perspective> camera,VAO geometry,PBRMaterial material)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            pbrShader.Uniform("albedo_Color", material.albedoColor);
            pbrShader.Uniform("roughness", material.roughness);
            pbrShader.Uniform("metal", material.metal);
            pbrShader.Uniform("ao", material.ao);
            Matrix4x4 mat = camera.Matrix;
            Vector3 camPos = camera.View.CalcPosition();
            pbrShader.Uniform("cameraMatrix", mat, true);
            pbrShader.Uniform("camPosition", camPos);
            pbrShader.Uniform("lightPosition", dLight.position);
            pbrShader.Uniform("lightColor", dLight.color);

            pbrShader.Activate();
            geometry.Draw();
            pbrShader.Deactivate();
        }
    }
}
