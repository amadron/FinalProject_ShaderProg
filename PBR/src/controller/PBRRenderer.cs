﻿using OpenTK.Graphics.OpenGL4;
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
        IShaderProgram textureTest;

        ITexture2D iblTexture;

        DirectionalLight dLight;
        PointLight[] pointLights;

        IRenderState renderstate;

        //IBL Creation Shader
        IShaderProgram cubeProjectionShader;
        //PBR IBL Diffuse
        IShaderProgram irradianceMapShader;
        uint ibl_skyboxMap;
        uint ibl_irradianceMap;
        //PBR IBL Specular part
        IShaderProgram prefilterMapShader;
        uint ibl_prefilteredEnvironment;
        IShaderProgram integrationMapShader;
        uint ibl_BrdfIntegraionMap;

        public PBRRenderer(IRenderState renderState, IContentLoader contentLoader)
        {
            GL.Enable(EnableCap.DebugOutput);
            
            this.renderstate = renderState;
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.TextureCubeMap);
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.BACK_SIDE));
            //pbrShader = contentLoader.Load<IShaderProgram>("PBRLightingBasic.*");
            pbrShader = contentLoader.Load<IShaderProgram>("PBRLightingIBLDiffuse.*");
            skyboxShader = contentLoader.Load<IShaderProgram>("Skybox.*");
            cubeProjectionShader = contentLoader.Load<IShaderProgram>("CubeMapProjection.*");
            textureTest = contentLoader.Load<IShaderProgram>("DisplayTexture2D.*");
            irradianceMapShader = contentLoader.Load<IShaderProgram>("IrradianceMap.*");
            prefilterMapShader = contentLoader.Load<IShaderProgram>("PrefilterIBLMap.*");
            dLight = new DirectionalLight();
            dLight.direction = new Vector3(0, 1, -1);
            dLight.color = new Vector3(10);
            dLight.position = new Vector3(0, 1, -1);

            Vector3 startPos = new Vector3(0, 0, 0.5f);
            pointLights = new PointLight[4];
            for(int i = 0; i < 4; i++)
            {
                pointLights[i] = new PointLight();
                pointLights[i].color = new Vector3(1);
                pointLights[i].radius = 1;
                pointLights[i].position = startPos;
            }
            pointLights[0].position = new Vector3(-1, 1, 0.5f);
            pointLights[1].position = new Vector3(-1, -1, 0.5f);
            pointLights[2].position = new Vector3(1, -1, 0.5f);
            pointLights[3].position = new Vector3(1, 1, 0.5f);
            

            int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(PointLight)) * pointLights.Length;

            //Generate buffer and allocate memory
            ubo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, ubo);
            GL.BufferData(BufferTarget.UniformBuffer, size, pointLights, BufferUsageHint.DynamicDraw);

            //Assign Buffer Block to ubo
            int uniformID = GL.GetUniformBlockIndex(pbrShader.ProgramID, "BufferPointLights");
            GL.UniformBlockBinding(pbrShader.ProgramID, uniformID, 0);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, ubo);

            DefaultMesh cubeMesh = Meshes.CreateCubeWithNormals();
            unitCube = VAOLoader.FromMesh(cubeMesh, cubeProjectionShader);

        }

        public void ShowTexture(uint textureID)
        {
            textureTest.Activate();
            SetSampler(textureTest.ProgramID, 0, "text", textureID);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            DeactivateTexture(0);
            textureTest.Deactivate();
        }

        public void SetIBLMap(string path)
        {
            ITexture2D sphereText = GetIBLTexture(path);
            GetCubeIBLMapsFromSphereMap(sphereText, ref ibl_skyboxMap, ref ibl_irradianceMap, ref ibl_prefilteredEnvironment, ref ibl_BrdfIntegraionMap);
        }

        public IShaderProgram GetPBRShader()
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

        public void StartRendering()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private void SetSampler(int programmID, int samplerNumber, string uniformName, ITexture2D texture)
        {
            SetSampler(programmID, samplerNumber, uniformName, texture.ID, TextureTarget.Texture2D);

        }

        private void SetSampler(int programmID, int samplerNumber, string uniformName, uint textureID)
        {
            SetSampler(programmID, samplerNumber, uniformName, textureID, TextureTarget.Texture2D);

        }

        private void SetSampler(int programmID, int samplerNumber, string uniformName, uint textureID, TextureTarget textureType)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + samplerNumber);
            GL.BindTexture(textureType, textureID);
            //SetUniform
            int samplerID = GL.GetUniformLocation(programmID, uniformName);
            GL.Uniform1(samplerID, samplerNumber);

        }

        private void DeactivateTexture(int samplerNumber)
        {
            DeactivateTexture(samplerNumber, TextureTarget.Texture2D);
        }

        private void DeactivateTexture(int samplerNumber, TextureTarget textureType)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + samplerNumber);
            GL.BindTexture(textureType, 0);
        }

        VAO unitCube;
        public void RenderSkybox(Matrix4x4 view, Matrix4x4 projection)
        {
            //RenderSkybox(ibl_skyboxMap, view, projection);
            RenderSkybox(ibl_skyboxMap, view, projection);
        }

        public void RenderSkybox(uint skyboxTexture, Matrix4x4 view, Matrix4x4 projection)
        {
            if(skyboxShader == null)
            {
                return;
            }
            GL.CullFace(CullFaceMode.Front);
            GL.DepthFunc(DepthFunction.Lequal);
            skyboxShader.Activate();
            skyboxShader.Uniform("view", view, true);
            skyboxShader.Uniform("projection", projection, true);
            //GL.ActiveTexture(TextureUnit.Texture0);
            //GL.BindTexture(TextureTarget.TextureCubeMap, skyboxTexture);
            SetSampler(skyboxShader.ProgramID, 0, "environmentMap", skyboxTexture, TextureTarget.TextureCubeMap);
            unitCube.Draw();
            GL.DepthFunc(DepthFunction.Less);
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            GL.CullFace(CullFaceMode.Back);
            //DeactivateTexture(skyboxTexture, TextureTarget.TextureCubeMap);

        }

        #region DiffuseIBL
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


        public void ShowTexture(uint texture, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            cubeProjectionShader.Activate();
            SetSampler(cubeProjectionShader.ProgramID, 0, "equirectangularMap", texture);
            cubeProjectionShader.Uniform("projection", projectionMatrix, true);
            cubeProjectionShader.Uniform("view", viewMatrix, true);

            unitCube.Draw();
            DeactivateTexture(0);
            cubeProjectionShader.Deactivate();
            DeactivateTexture(0);
        }

        private void SetUpCubeMapParameters()
        {
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        }

        private int CreateCubeMap(int resX, int resY)
        {
            int cubeMap = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeMap);

            for (int i = 0; i < 6; i++)
            {
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgb16f, resX, resY, 0,
                              PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            }
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            return cubeMap;
        }

        public void GetCubeIBLMapsFromSphereMap(ITexture2D sphereTexture, ref uint cubeMap, ref uint irradianceMap, ref uint prefilterMap, ref uint integrationMap)
        {
            GL.CullFace(CullFaceMode.Front);
            DefaultMesh cubeMesh = Meshes.CreateCubeWithNormals();

            //Set up Projection Matrix and View Matrix for rendering into Cubemap
            Matrix4x4 captureProjection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), 1.0f, 0.1f, 10.0f);
            Matrix4x4[] captureView =
            {
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3( 1.0f, 0.0f, 0.0f), new Vector3(0.0f,-1.0f, 0.0f)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f,-1.0f, 0.0f)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f,-1.0f, 0.0f), new Vector3(0.0f, 0.0f,-1.0f)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f, 0.0f, 1.0f), new Vector3(0.0f,-1.0f, 0.0f)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f, 0.0f,-1.0f), new Vector3(0.0f,-1.0f, 0.0f)),
            };
            int captureFBO = GL.GenFramebuffer();
            //Basic Skybox/IBL CubeMap
            cubeMap = CreateEnvironmentMap(captureFBO, captureProjection, captureView, sphereTexture);
            //Irradiance map, which is presampled map of irradiance of IBL Cubemap
            irradianceMap = CreateIrradianceMap(captureFBO, captureProjection, captureView, cubeMap);

            prefilterMap = CreatePrefilteredMap(captureFBO, captureProjection, captureView, cubeMap);
            integrationMap = CreateIntegratedMap(captureFBO, captureProjection, captureView, cubeMap);
            /*
             * 
             * Create BDRF Integration Map
             * 
             */

            GL.CullFace(CullFaceMode.Back);
        }

        uint CreateEnvironmentMap(int frameBuffer, Matrix4x4 projection, Matrix4x4[] viewMatrices, ITexture2D sphereTexture)
        {
            int fbCubeWidht = 512;
            int fbCubeHeight = 512;
            DefaultMesh cubeMesh = Meshes.CreateCubeWithNormals();
            VAO renderCubeMapCube = VAOLoader.FromMesh(cubeMesh, cubeProjectionShader);

            //Cubemap
            int envCubeMap = CreateCubeMap(fbCubeWidht, fbCubeHeight);
            //FBO

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            //GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMap, envCubeMap, 0);

            //Render into Cubemap
            //Setting up shader
            cubeProjectionShader.Activate();
            cubeProjectionShader.Uniform("projection", projection, true);

            SetSampler(cubeProjectionShader.ProgramID, 0, "equirectangularMap", sphereTexture);
            GL.Viewport(0, 0, fbCubeWidht, fbCubeHeight);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            //GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            for (int i = 0; i < 6; i++)
            {
                cubeProjectionShader.Uniform("view", viewMatrices[i], true);

                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                                        TextureTarget.TextureCubeMapPositiveX + i, envCubeMap, 0);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                renderCubeMapCube.Draw();
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            DeactivateTexture(0);
            return (uint)envCubeMap;
        }

        uint CreateIrradianceMap(int frameBuffer, Matrix4x4 projection, Matrix4x4[] viewMatrices, uint cubeMap)
        {
            DefaultMesh cubeMesh = Meshes.CreateCubeWithNormals();
            VAO renderIrradMapCube = VAOLoader.FromMesh(cubeMesh, irradianceMapShader);
            //Create Irradiance Texture
            int fbIrrWidth = 32;
            int fbIrrHeight = 32;
            int irradMap = CreateCubeMap(32, 32);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            //GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMap, irradMap, 0);
            //Render 
            //Irradiance Map
            irradianceMapShader.Activate();
            SetSampler(irradianceMapShader.ProgramID, 0, "environmentMap", cubeMap, TextureTarget.TextureCubeMap);
            irradianceMapShader.Uniform("projection", projection, true);

            GL.Viewport(0, 0, fbIrrWidth, fbIrrHeight);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            for (int i = 0; i < 6; i++)
            {
                irradianceMapShader.Uniform("view", viewMatrices[i], true);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                                       TextureTarget.TextureCubeMapPositiveX + i, irradMap, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                renderIrradMapCube.Draw();
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            return (uint)irradMap;
        }

        uint CreatePrefilteredMap(int frameBuffer, Matrix4x4 projection, Matrix4x4[] viewMatrices, uint cubeMap)
        {
            DefaultMesh cubeMesh = Meshes.CreateCubeWithNormals();
            VAO renderPrefilterCube = VAOLoader.FromMesh(cubeMesh, prefilterMapShader);
            int texResX = 128;
            int texResY = 128;

            int prefiltMap = CreateCubeMap(texResX, texResY);
            GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);

            prefilterMapShader.Activate();
            SetSampler(prefilterMapShader.ProgramID, 0, "environmentMap", cubeMap, TextureTarget.TextureCubeMap);
            prefilterMapShader.Uniform("projection", projection);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            int maxMipLvl = 5;
            for(int i = 0; i < maxMipLvl; i++)
            {
                int mipWidth = texResX * (int)Math.Pow(0.5, i);
                int mipHeight = texResY * (int)Math.Pow(0.5, i);

                float roughness = (float)mipHeight / (float)(maxMipLvl - 1);
                prefilterMapShader.Uniform("roughness", roughness);
                for(int j = 0; j < 6; j++)
                {
                    prefilterMapShader.Uniform("view", viewMatrices[j]);
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                                            TextureTarget.TextureCubeMapPositiveX + j, prefiltMap, i);

                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                    renderPrefilterCube.Draw();
                }
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            return (uint) prefiltMap;
        }

        uint CreateIntegratedMap(int frameBuffer, Matrix4x4 projection, Matrix4x4[] viewMatrices, uint cubeMap)
        {
            uint integratedMap = (uint)GL.GenTexture();
            return integratedMap;
        }

        public void ShowCubeMap(ITexture2D sphereTexture, int side)
        {
            GL.CullFace(CullFaceMode.Front);
            DefaultMesh cubeMesh = Meshes.CreateCubeWithNormals();
            VAO renderCube = VAOLoader.FromMesh(cubeMesh, cubeProjectionShader);

            //Set up Projection Matrix and View Matrix for rendering into Cubemap
            Matrix4x4 captureProjection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), 1.0f, 0.1f, 10.0f);
            Matrix4x4[] captureView =
            {
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3( 1.0f, 0.0f, 0.0f), new Vector3(0.0f,-1.0f, 0.0f)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f,-1.0f, 0.0f)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f,-1.0f, 0.0f), new Vector3(0.0f, 0.0f,-1.0f)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f, 0.0f, 1.0f), new Vector3(0.0f,-1.0f, 0.0f)),
                Matrix4x4.CreateLookAt(Vector3.Zero, new Vector3( 0.0f, 0.0f,-1.0f), new Vector3(0.0f,-1.0f, 0.0f)),
            };

            //Setting up shader
            cubeProjectionShader.Activate();
            SetSampler(cubeProjectionShader.ProgramID, 0, "equirectangularMap", sphereTexture);
            cubeProjectionShader.Uniform("projection", captureProjection, true);

            GL.ActiveTexture(TextureUnit.Texture0);
            sphereTexture.Activate();

            //Render into Cubemap
            GL.Viewport(0, 0, 512, 512);
            cubeProjectionShader.Uniform("view", captureView[side], true);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            renderCube.Draw();

            sphereTexture.Deactivate();
            DeactivateTexture(0);
            GL.CullFace(CullFaceMode.Back);
        }

        #endregion


        public void Render(Matrix4x4 camMatrix, Vector3 camPosition, GameObject obj)
        {
            if (pbrShader == null || obj == null || obj.mesh == null || obj.material == null)
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

            if (obj.material.normalMap != null)
            {
                pbrShader.Uniform("hasNormalMap", 1);
                SetSampler(pbrShader.ProgramID, textCounter++, "normalMap", obj.material.normalMap);
            }
            else
            {
                pbrShader.Uniform("hasNormalMap", 0);
            }

            pbrShader.Uniform("roughnessFactor", obj.material.roughness);
            if (obj.material.roughnessMap != null)
            {
                pbrShader.Uniform("hasRoughnessMap", 1);
                SetSampler(pbrShader.ProgramID, textCounter++, "roughnessMap", obj.material.roughnessMap);
            }
            else
            {
                pbrShader.Uniform("hasRoughnessMap", 0);
            }

            pbrShader.Uniform("metalFactor", obj.material.metal);
            if (obj.material.metallicMap != null)
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

            SetSampler(pbrShader.ProgramID, textCounter++, "irradianceMap", ibl_irradianceMap,TextureTarget.TextureCubeMap);

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

            for (int i = textCounter; i >= 0; i--)
            {
                DeactivateTexture(i);
            }

        }
    }


}
