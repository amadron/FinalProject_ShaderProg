using OpenTK.Graphics.OpenGL4;
using Zenseless.Geometry;
using PBR.src.model.rendering;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using System.Numerics;

namespace PBR.src.controller
{
    class PBRRenderer
    {
        int ubo = 0;
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
            Vector3 startPos = new Vector3(-step, step, -0.5f);
            pointLights = new PointLight[width * width];
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    int idx = i * width + j;

                    pointLights[idx] = new PointLight();
                    pointLights[idx].color = new Vector3(1);
                    pointLights[idx].position = startPos;
                    pointLights[idx].position.X += i * step;
                    pointLights[idx].position.Y += j * step;
                    pointLights[idx].radius = 0.5f;
                }
            }
            int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(PointLight)) * pointLights.Length;

            //Generate buffer and allocate memory
            ubo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, ubo);
            GL.BufferData(BufferTarget.UniformBuffer, size, pointLights, BufferUsageHint.DynamicDraw);

            //Assign Buffer Block to ubo
            int uniformID = GL.GetUniformBlockIndex(pbrShader.ProgramID, "BufferPointLights");
            GL.UniformBlockBinding(pbrShader.ProgramID, uniformID, 0);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, ubo);

            

        }

        public IShaderProgram GetShader()
        {
            return pbrShader;
        }

        struct PointLight
        {
            public Vector3 position;
            public float radius;
            public Vector3 color;
            public float offset;
        }

        
        DirectionalLight dLight;
        PointLight[] pointLights;

        public void StartRendering()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void Render(Matrix4x4 camMatrix, Vector3 camPosition,VAO geometry,PBRMaterial material)
        {
            pbrShader.Activate();

            GL.BindBuffer(BufferTarget.UniformBuffer, ubo);
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
            pbrShader.Uniform("pointLightAmount", pointLights.Length);

            geometry.Draw();

            pbrShader.Deactivate();
        }
    }
}
