using OpenTK.Graphics.OpenGL4;
using Zenseless.Geometry;
using PBR.src.model.rendering;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using System.Numerics;
using PBR.src.model;
using System.Runtime.InteropServices;
using System;
using System.Drawing;

namespace PBR.src.controller
{
    class PBRRenderer
    {
        int ubo = 0;
        IShaderProgram pbrShader;
        IShaderProgram skyboxShader;

        ITexture2D iblTexture;

        public PBRRenderer(IRenderState renderState, IContentLoader contentLoader)
        {
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.BACK_SIDE));
            
            pbrShader = contentLoader.Load<IShaderProgram>("pbrLighting.*");
            skyboxShader = contentLoader.Load<IShaderProgram>("Skybox.*");
            dLight = new DirectionalLight();
            dLight.direction = new Vector3(0, 1, -1);
            dLight.color = new Vector3(10);
            dLight.position = new Vector3(0, 1, -1);
            int width = 1;
            float step = 0.5f;
            Vector3 startPos = new Vector3(0, step, 0.5f);
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

            cubeProjectionShader = contentLoader.Load<IShaderProgram>("cubeMapProjection.*");
            DefaultMesh cubeMesh = Meshes.CreateCubeWithNormals();
            unitCube = VAOLoader.FromMesh(cubeMesh, cubeProjectionShader);

        }

        public void SetPBRShader(IShaderProgram shader)
        {
            this.pbrShader = shader;
        }

        public void SetSkyboxShader(IShaderProgram shader)
        {
            this.skyboxShader = shader;
        }
        

        public IShaderProgram GetPBRShader()
        {
            return pbrShader;
        }

        public void SetIBLTexture(ITexture2D texture)
        {
            this.iblTexture = texture;
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

        private void DeactivateTexture(int samplerNumber)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + samplerNumber);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        VAO unitCube;
        public void RenderSkybox(ITexture2D skyboxTexture, Matrix4x4 projection, Matrix4x4 view)
        {
            if(skyboxShader == null)
            {
                return;
            }
            GL.DepthFunc(DepthFunction.Lequal);
            skyboxShader.Activate();
            skyboxShader.Uniform("view", view);
            skyboxShader.Uniform("projection", projection);
            SetSampler(skyboxShader.ProgramID, 0, "environmentMap", skyboxTexture);
            unitCube.Draw();
            GL.DepthFunc(DepthFunction.Less);

        }

        public void Render(Matrix4x4 camMatrix, Vector3 camPosition, GameObject obj)
        {
            if(pbrShader == null || obj == null || obj.mesh == null || obj.material == null)
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
                pbrShader.Uniform("hasRoughnessMap", 1);
                SetSampler(pbrShader.ProgramID, textCounter++, "roughnessMap", obj.material.roughnessMap);
            }
            else
            {
                pbrShader.Uniform("hasRoughnessMap", 0);
            }

            pbrShader.Uniform("metalFactor", obj.material.metal);
            if(obj.material.metallicMap != null)
            {
                pbrShader.Uniform("hasMetallicMap", 1);
                SetSampler(pbrShader.ProgramID, textCounter++, "metallicMap", obj.material.metallicMap);
            }
            else
            {
                pbrShader.Uniform("hasMetallicMap", 0);
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

            for(int i = textCounter; i >= 0; i--)
            {
                DeactivateTexture(i);
            }

        }

        public void DebugLight(PointLight pLight)
        {

        }

        #region DiffuseIBL
        IShaderProgram cubeProjectionShader;

        public ITexture2D GetIBLTexture(string path)
        {
            FreeImageAPI.FREE_IMAGE_FORMAT type = FreeImageAPI.FreeImage.GetFileType(path, 0);
            FreeImageAPI.FIBITMAP bitmap = FreeImageAPI.FreeImage.Load(type, path, FreeImageAPI.FREE_IMAGE_LOAD_FLAGS.DEFAULT);
            FreeImageAPI.FIBITMAP convert = FreeImageAPI.FreeImage.ToneMapping(bitmap, FreeImageAPI.FREE_IMAGE_TMO.FITMO_DRAGO03, 0, 0);

            if (bitmap.IsNull)
            {
                return null;
            }
            uint width = FreeImageAPI.FreeImage.GetWidth(convert);
            uint height = FreeImageAPI.FreeImage.GetHeight(convert);
            int channelNo = 3;
            byte[] imgData = new byte[width * height * channelNo];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int idx = i * (int)width + j;
                    FreeImageAPI.RGBQUAD color = new FreeImageAPI.RGBQUAD();
                    int byteIdx = idx * channelNo;
                    byteIdx -= 1;
                    bool pSuccess = FreeImageAPI.FreeImage.GetPixelColor(convert, (uint)j, (uint)i, out color);
                    Color nColor = color.Color;

                    imgData[byteIdx + 1] = color.rgbRed;
                    imgData[byteIdx + 2] = color.rgbGreen;
                    imgData[byteIdx + 3] = color.rgbBlue;

                    if (!pSuccess)
                    {
                        return null;
                    }
                }
            }

            Texture2dGL text = Texture2dGL.Create((int)width, (int)height);
            IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(imgData, 0);
            text.LoadPixels(ptr, (int)width, (int)height, OpenTK.Graphics.OpenGL4.PixelInternalFormat.Rgb8, OpenTK.Graphics.OpenGL4.PixelFormat.Rgb, OpenTK.Graphics.OpenGL4.PixelType.UnsignedByte);
            return text;
        }

        public void ShowTexture(ITexture2D texture, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            if (texture != null)
            {
                cubeProjectionShader.Activate();
                SetSampler(cubeProjectionShader.ProgramID, 0, "equirectangularMap", texture);
                cubeProjectionShader.Uniform("projection", projectionMatrix, true);
                cubeProjectionShader.Uniform("view", viewMatrix, true);

                unitCube.Draw();
                cubeProjectionShader.Deactivate();
                DeactivateTexture(0);
            }
        }

        public ITexture2D GetHDRCubeMap(string path)
        {
            ITexture2D sphereText = GetIBLTexture(path);
            return CreateHDRCubeMap(sphereText);
        }

        public ITexture2D CreateHDRCubeMap(ITexture2D sphereTexture)
        {
            DefaultMesh cubeMesh = Meshes.CreateCubeWithNormals();
            VAO renderCube = VAOLoader.FromMesh(cubeMesh, cubeProjectionShader);

            ITexture2D captureTexture = Texture2dGL.Create(512, 512);
            FBO captureFBO = new FBO(captureTexture);
            


            //Setting up the Cubemap to render to;
            int envCubeMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, envCubeMap);

            for(int i = 0; i < 6; i++)
            {
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgb16f, 512, 512, 0, PixelFormat.Rgb, PixelType.Float, System.IntPtr.Zero);
            }
            int[] wrapParam = { (int)TextureWrapMode.ClampToEdge };
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, wrapParam);
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, wrapParam);
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, wrapParam);
            int[] filterMinParam = { (int)TextureMinFilter.Linear };
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, filterMinParam);
            int[] filterMagParam = { (int)TextureMagFilter.Linear };
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, filterMagParam);

            Matrix4x4 captureProjection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), 1.0f, 0.1f, 10.0f);
            Matrix4x4[] captureView =
            {
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3(1.0f,0,0), new Vector3(0,-1.0f, 0)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3(-1.0f,0,0), new Vector3(0,-1.0f, 0)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3(0,1.0f,0), new Vector3(0, 0, 1.0f)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3(0,-1.0f,0), new Vector3(0, 0, -1.0f)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3(0,0,1.0f), new Vector3(0,-1.0f, 0)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3(0,0,-1.0f), new Vector3(0,-1.0f, 0)),
            };

            cubeProjectionShader.Activate();
            cubeProjectionShader.Uniform("projection", captureProjection);
            SetSampler(cubeProjectionShader.ProgramID, 0, "equirectangularMap", sphereTexture);

            GL.Viewport(0, 0, 512, 512);
            captureFBO.Activate();
            for(int i = 0; i < 6; i++)
            {
                cubeProjectionShader.Uniform("view", captureView[i]);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMapPositiveX + i, envCubeMap, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                renderCube.Draw();
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            return captureTexture;
        }

        #endregion
    }
}
