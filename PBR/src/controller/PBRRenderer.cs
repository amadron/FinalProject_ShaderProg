using OpenTK.Graphics.OpenGL4;
using Zenseless.Geometry;
using PBR.src.model.rendering;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using System.Numerics;
using PBR.src.model;

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
            
            pbrShader = contentLoader.Load<IShaderProgram>("pbrLighting.*");
            dLight = new DirectionalLight();
            dLight.direction = new Vector3(0, 1, -1);
            dLight.color = new Vector3(10);
            dLight.position = new Vector3(0, 1, -1);
            int width = 1;
            float step = 0.5f;
            Vector3 startPos = new Vector3(0, step, 1f);
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
                    pointLights[idx].position.Y -= j * step;
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

        public struct PointLight
        {
            public Vector3 position;
            public float radius;
            public Vector3 color;
            public float offset; //Because of block alignment in shader uniform block
        }

        
        DirectionalLight dLight;
        PointLight[] pointLights;

        public void StartRendering()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private void SetSampler(int programmID, int samplerNumber, string uniformName, ITexture2D texture)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + samplerNumber);
            GL.BindTexture(TextureTarget.Texture2D, texture.ID);
            //SetUniform
            int samplerID = GL.GetUniformLocation(programmID, uniformName);
            GL.Uniform1(samplerID, samplerNumber);

        }

        public void Render(Matrix4x4 camMatrix, Vector3 camPosition, GameObject obj)
        {
            if(obj == null || obj.mesh == null || obj.material == null)
            {
                return;
            }

            pbrShader.Activate();

            GL.BindBuffer(BufferTarget.UniformBuffer, ubo);

            pbrShader.Uniform("albedoColor", obj.material.albedoColor);
            int textCounter = 0;

            if (obj.material.albedoMap != null)
            {
                pbrShader.Uniform("hasAlbedoMap", 1);
                SetSampler(pbrShader.ProgramID, textCounter++, "albedoMap", obj.material.albedoMap);
            }
            else
            {
                pbrShader.Uniform("hasAlbedoMap", 0);
            }

            if(obj.material.normalMap != null)
            {
                pbrShader.Uniform("hasNormalMap", 1);
                SetSampler(pbrShader.ProgramID, textCounter++, "normalMap", obj.material.normalMap);
            }
            else
            {
                pbrShader.Uniform("hasNormalMap", 0);
            }

            pbrShader.Uniform("roughnessFactor", obj.material.roughness);
            if(obj.material.roughnessMap != null)
            {
                pbrShader.Uniform("hasRougnessMap", 1);
                SetSampler(pbrShader.ProgramID, textCounter++, "roughnessMap", obj.material.roughnessMap);
            }
            else
            {
                pbrShader.Uniform("hasRougnessMap", 0);
            }

            pbrShader.Uniform("metalFactor", obj.material.metal);
            if(obj.material.metallicMap != null)
            {
                pbrShader.Uniform("hasRougnessMap", 1);
                SetSampler(pbrShader.ProgramID, textCounter++, "metallicMap", obj.material.metallicMap);
            }
            else
            {
                pbrShader.Uniform("hasRougnessMap", 0);
            }

            pbrShader.Uniform("aoFactor", obj.material.ao);
            if (obj.material.occlusionMap != null)
            {
                pbrShader.Uniform("hasOcclusionMap", 1);
                SetSampler(pbrShader.ProgramID, textCounter++, "occlusionMap", obj.material.occlusionMap);
            }
            else
            {
                pbrShader.Uniform("hasOcclusionMap", 0);
            }

            Matrix4x4 mat = camMatrix;
            Vector3 camPos = camPosition;
            pbrShader.Uniform("modelMatrix", obj.transform.modelMatrix, true);
            pbrShader.Uniform("cameraMatrix", mat, true);
            pbrShader.Uniform("camPosition", camPos);
            pbrShader.Uniform("lightPosition", dLight.position);
            pbrShader.Uniform("lightColor", dLight.color);
            pbrShader.Uniform("pointLightAmount", pointLights.Length);

            


            obj.mesh.Draw();

            pbrShader.Deactivate();


        }

        public void DebugLight(PointLight pLight)
        {

        }
    }
}
