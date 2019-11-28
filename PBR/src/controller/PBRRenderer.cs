using OpenTK.Graphics.OpenGL;
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
            dLight.direction = new Vector3(0, 1, -1);
            dLight.color = new Vector3(10);
            dLight.position = new Vector3(0, 1, -1);
            int width = 2;
            float step = 0.5f;
            Vector3 startPos = new Vector3(-step, step, -1);
            pointLights = new PointLight[width * width];
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    int idx = i * width + j;

                    pointLights[idx] = new PointLight();
                    pointLights[idx].position = startPos;
                    pointLights[idx].position.X += i * step;
                    pointLights[idx].position.Y += j * step;
                }
            }
            
           
        }

        public IShaderProgram GetShader()
        {
            return pbrShader;
        }

        DirectionalLight dLight;
        PointLight[] pointLights;

        public void StartRendering()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void Render(Matrix4x4 camMatrix, Vector3 camPosition,VAO geometry,PBRMaterial material)
        {
            
            
            pbrShader.Uniform("albedo", material.albedoColor);
            pbrShader.Uniform("roughness", material.roughness);
            pbrShader.Uniform("metal", material.metal);
            pbrShader.Uniform("ao", material.ao);
            Matrix4x4 mat = camMatrix;
            Vector3 camPos = camPosition;
            pbrShader.Uniform("cameraMatrix", mat, true);
            pbrShader.Uniform("camPosition", camPos);
            pbrShader.Uniform("lightPosition", dLight.position);
            pbrShader.Uniform("lightColor", dLight.color);
            
            pbrShader.Uniform("pointLightAmound", pointLights.Length);



            pbrShader.Activate();
            geometry.Draw();
            pbrShader.Deactivate();
        }
    }
}
