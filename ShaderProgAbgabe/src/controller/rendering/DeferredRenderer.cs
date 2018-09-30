using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using OpenTK.Graphics.OpenGL4;
using Zenseless.Geometry;
using System.Numerics;
using Example.src.controller;
using Example.src.model.lightning;
using Example.src.model.graphics.camera;
using Example.src.model.entitys;

namespace Example.src.model.graphics.rendering
{
    class DeferredRenderer
    {
        #region SetUp
        public enum DrawableType
        {
            deferredDefaultMesh,
            lightViewMesh,
            shadowMapMesh,
            particleMesh,
            particleShadowLightView,
            particleShadowMap
        }

        public enum RenderMode { deferred, postion, color, normal, pointlight, shadow, directional };

        public DeferredRenderer(IContentLoader contentLoader, IRenderState renderState)
        {
            renderState.Set<DepthTest>(new DepthTest(true));
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.CullFace(CullFaceMode.Back);


            this.renderState = renderState;
            this.contentLoader = contentLoader;

            deferredParticleShader = contentLoader.Load<IShaderProgram>("deferred_particle.*");
            shadowMapShaderParticle = contentLoader.Load<IShaderProgram>("shadowMapParticle.*");
            shadowLightViewShaderParticle = contentLoader.Load<IShaderProgram>("shadowLightViewParticle.*");

            deferredGeometryShader = contentLoader.Load<IShaderProgram>("deferred_geometry.*");

            deferredPost = contentLoader.LoadPixelShader("deferred_post");
            pointLightShader = contentLoader.Load<IShaderProgram>("def_pointLight.*");
            shadowMapShader = contentLoader.Load<IShaderProgram>("shadowMap.*");
            shadowLightViewShader = contentLoader.Load<IShaderProgram>("shadowLightView.*");
            var lmesh = Meshes.CreateSphere(1, 2);
            pointLightSphere = VAOLoader.FromMesh(lmesh, pointLightShader);
        }
        IRenderState renderState;
        IContentLoader contentLoader;

        VAO pointLightSphere;
        int pointLightAmount;

        public IRenderSurface mainFBO;
        public IRenderSurface pointLightFBO;
        public IRenderSurface shadowMapFBO;
        public IRenderSurface lightViewFBO;
        public IRenderSurface finalPassFBO;

        IShaderProgram deferredParticleShader;
        IShaderProgram shadowMapShaderParticle;
        IShaderProgram shadowLightViewShaderParticle;

        IShaderProgram deferredGeometryShader;

        IShaderProgram deferredPost;
        IShaderProgram pointLightShader;
        IShaderProgram shadowMapShader;
        IShaderProgram shadowLightViewShader;

        SATGpuFilter satFilter;

        int shadowExponent = 20;

        public RenderMode currentRenderMode = RenderMode.deferred;
        
        public void Resize(int width, int height)
        {
            mainFBO = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));
            mainFBO.Attach(Texture2dGL.Create(width, height, 4, true));
            mainFBO.Attach(Texture2dGL.Create(width, height, 4, true));
            mainFBO.Attach(Texture2dGL.Create(width, height, 4, true));
            mainFBO.Attach(Texture2dGL.Create(width, height, 4, true));
            foreach (ITexture2D text in mainFBO.Textures)
            {
                text.WrapFunction = TextureWrapFunction.MirroredRepeat;
            }
            pointLightFBO = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));
            pointLightFBO.Texture.WrapFunction = TextureWrapFunction.MirroredRepeat;

            lightViewFBO = new FBOwithDepth(Texture2dGL.Create(height, width, 4, true));
            lightViewFBO.Texture.WrapFunction = TextureWrapFunction.Repeat;

            shadowMapFBO = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));
            shadowMapFBO.Texture.WrapFunction = TextureWrapFunction.Repeat;

            finalPassFBO = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));

            satFilter = new SATGpuFilter(contentLoader, renderState, 32, 32, width, height, 4, 4);
        }


        
        public void SetPointLights(List<PointLight> pointLights)
        {
            Vector3[] instPos = new Vector3[pointLights.Count];
            Vector4[] instCols = new Vector4[pointLights.Count];
            float[] instRadius = new float[pointLights.Count];
            float[] instIntensity = new float[pointLights.Count];

            Vector4[] instSpecCol = new Vector4[pointLights.Count];
            float[] instSpecFact = new float[pointLights.Count];
            float[] instSpecIntensity = new float[pointLights.Count];

            for (int i = 0; i < pointLights.Count; i++)
            {
                instPos[i] = pointLights[i].position;
                instCols[i] = pointLights[i].lightColor;
                instRadius[i] = pointLights[i].radius;
                instIntensity[i] = pointLights[i].intensity;

                instSpecCol[i] = pointLights[i].specularColor;
                instSpecFact[i] = pointLights[i].specularFactor;
                instSpecIntensity[i] = pointLights[i].specularIntensity;
            }
            pointLightSphere.SetAttribute(GL.GetAttribLocation(pointLightShader.ProgramID, "instancePosition"), instPos, true);
            pointLightSphere.SetAttribute(GL.GetAttribLocation(pointLightShader.ProgramID, "instanceColor"), instCols, true);
            pointLightSphere.SetAttribute(GL.GetAttribLocation(pointLightShader.ProgramID, "instanceRadius"), instRadius, true);
            pointLightSphere.SetAttribute(GL.GetAttribLocation(pointLightShader.ProgramID, "instanceIntensity"), instIntensity, true);

            pointLightSphere.SetAttribute(GL.GetAttribLocation(pointLightShader.ProgramID, "instanceSpecularColor"), instSpecCol, true);
            pointLightSphere.SetAttribute(GL.GetAttribLocation(pointLightShader.ProgramID, "instanceSpecularFactor"), instSpecFact, true);
            pointLightSphere.SetAttribute(GL.GetAttribLocation(pointLightShader.ProgramID, "instanceSpecularIntensity"), instSpecIntensity, true);
            pointLightAmount = pointLights.Count;
        }
        #endregion

        #region Getter/Setter
        public VAO GetDrawable(DefaultMesh mesh, DrawableType type)
        {
            MeshAttribute uvs = mesh.GetAttribute("uv");

            int elems = uvs.ToArray().Length;
            if (elems == 0)
            {
                mesh.SetConstantUV(new Vector2(0, 0));
            }
            IShaderProgram shader = GetShader(type);
            VAO res = VAOLoader.FromMesh(mesh, shader);
            return res;
        }

        public IShaderProgram GetShader(DrawableType type)
        {
            IShaderProgram shader = deferredGeometryShader;
            switch(type)
            {
                case DrawableType.particleMesh:
                    shader = deferredParticleShader;
                    break;
                case DrawableType.lightViewMesh:
                    shader = shadowLightViewShader;
                    break;
                case DrawableType.shadowMapMesh:
                    shader = shadowMapShader;
                    break;
                case DrawableType.particleShadowLightView:
                    shader = shadowLightViewShaderParticle;
                    break;
                case DrawableType.particleShadowMap:
                    shader = shadowMapShaderParticle;
                    break;
            }
            
            return shader;
        }


        public ITexture2D GetFinalPassTexture()
        {
            if(currentRenderMode == RenderMode.deferred)
            {
                return finalPassFBO.Texture;
            }
            else if(currentRenderMode == RenderMode.postion)
            {
                return mainFBO.Textures[0];
            }
            else if(currentRenderMode == RenderMode.color)
            {
                return mainFBO.Textures[1];
            }
            else if(currentRenderMode == RenderMode.normal)
            {
                return mainFBO.Textures[2];
            }
            else if(currentRenderMode == RenderMode.directional)
            {
                return lightViewFBO.Texture;
            }
            else if(currentRenderMode == RenderMode.shadow)
            {
                return shadowMapFBO.Texture;
            }
            else
            {
                return pointLightFBO.Texture;
            }
        }
        #endregion

        #region GeometryPass
        public void StartGeometryPass()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            mainFBO.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }





        public void DrawDeferredGeometry(Renderable geometry, Matrix4x4 cameraMatrix, Vector3 cameraPosition, Vector3 cameraDirection)
        {
            
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(geometry.faceCullingMode));
            if (geometry.hasAlphaMap == 1)
            {
                GL.Enable(EnableCap.Blend);
                GL.DepthMask(false);
            }
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            deferredGeometryShader.Activate();
            

            deferredGeometryShader.Uniform("camera", cameraMatrix);
            deferredGeometryShader.Uniform("cameraPosition", cameraPosition);
            deferredGeometryShader.Uniform("cameraDirection", cameraDirection);
            deferredGeometryShader.Uniform("hasAlbedo", geometry.hasAlbedoTexture);
            deferredGeometryShader.Uniform("hasNormalMap", geometry.hasNormalMap);
            deferredGeometryShader.Uniform("hasHeightMap", geometry.hasHeightMap);
            deferredGeometryShader.Uniform("heightScaleFactor", geometry.heightScaleFactor);
            deferredGeometryShader.Uniform("hasAlphaMap", geometry.hasAlphaMap);
            deferredGeometryShader.Uniform("hasEnvironmentMap", geometry.hasEnvironmentMap);
            deferredGeometryShader.Uniform("reflectionFactor", geometry.reflectivity);
            deferredGeometryShader.Uniform("isUnlit", geometry.unlit);
            if(geometry.instances > 0)
            {
                deferredGeometryShader.Uniform("hasInstances", 1);
            }
            else
            {
                deferredGeometryShader.Uniform("hasInstances", 0);
            }


            //Activate Textures of FBO
            int textAmount = mainFBO.Textures.Count; //Number of Texture Channels of FBO
            for (int i = 0; i < textAmount; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                mainFBO.Textures[i].Activate();
            }

            DrawBuffersEnum[] buffers = new DrawBuffersEnum[textAmount];
            for (int i = 0; i < textAmount; i++)
            {
                buffers[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }

            GL.DrawBuffers(textAmount, buffers);

            int albedoText = GL.GetUniformLocation(deferredGeometryShader.ProgramID, "albedoSampler");
            int normalMap = GL.GetUniformLocation(deferredGeometryShader.ProgramID, "normalSampler");
            int heightMap = GL.GetUniformLocation(deferredGeometryShader.ProgramID, "heightSampler");
            int alphaMap = GL.GetUniformLocation(deferredGeometryShader.ProgramID, "alphaSampler");
            int environMap = GL.GetUniformLocation(deferredGeometryShader.ProgramID, "environmentSampler");
            GL.Uniform1(albedoText, 0);
            GL.Uniform1(normalMap, 1);
            GL.Uniform1(heightMap, 2);
            GL.Uniform1(alphaMap, 3);
            GL.Uniform1(environMap, 4);
            GL.ActiveTexture(TextureUnit.Texture0);
            if (geometry.albedoTexture != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.albedoTexture.ID);
            }

            GL.ActiveTexture(TextureUnit.Texture1);
            if (geometry.normalMap != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.normalMap.ID);
            }
            GL.ActiveTexture(TextureUnit.Texture2);
            if(geometry.heightMap != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.heightMap.ID);
            }
            GL.ActiveTexture(TextureUnit.Texture3);
            if(geometry.alphaMap != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.alphaMap.ID);
            }
            GL.ActiveTexture(TextureUnit.Texture4);
            if(geometry.environmentMap != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.environmentMap.ID);
            }

            //Draw Gemetry

            if(geometry.instances > 0)
            {
                geometry.GetDeferredMesh().Draw(geometry.instances);
            }
            else
            {
                geometry.GetDeferredMesh().Draw();
            }
            //Deactivate Textures of FBO
            for (int i = textAmount - 1; i >= 0; i--)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                mainFBO.Textures[i].Deactivate();
            }
            deferredGeometryShader.Deactivate();
            if (geometry.hasAlphaMap == 1)
            {
                GL.DepthMask(true);
            }
            GL.Disable(EnableCap.Blend);
        }

        public void DrawDeferredParticle(Renderable geometry, Camera camera, Vector3 cameraPosition, Vector3 cameraDirection, ParticleSystem paricleSystem)
        {

            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(geometry.faceCullingMode));
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            if (geometry.hasAlphaMap == 1)
            {
                GL.Enable(EnableCap.Blend);
                GL.DepthMask(false);
            }
            
            deferredParticleShader.Activate();

            deferredParticleShader.Uniform("camera", camera.GetMatrix());
            deferredParticleShader.Uniform("viewMatrix", camera.GetViewMatrix());
            deferredParticleShader.Uniform("cameraPosition", cameraPosition);
            deferredParticleShader.Uniform("cameraDirection", cameraDirection);
            deferredParticleShader.Uniform("hasAlbedo", geometry.hasAlbedoTexture);
            deferredParticleShader.Uniform("hasNormalMap", geometry.hasNormalMap);
            deferredParticleShader.Uniform("hasAlphaMap", geometry.hasAlphaMap);
            deferredParticleShader.Uniform("hasEnvironmentMap", geometry.hasEnvironmentMap);
            deferredParticleShader.Uniform("reflectionFactor", geometry.reflectivity);

            //Activate Textures of FBO
            int textAmount = mainFBO.Textures.Count; //Number of Texture Channels of FBO
            for (int i = 0; i < textAmount; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                mainFBO.Textures[i].Activate();
            }

            DrawBuffersEnum[] buffers = new DrawBuffersEnum[textAmount];
            for (int i = 0; i < textAmount; i++)
            {
                buffers[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }

            GL.DrawBuffers(textAmount, buffers);

            int albedoText = GL.GetUniformLocation(deferredParticleShader.ProgramID, "albedoSampler");
            int normalMap = GL.GetUniformLocation(deferredParticleShader.ProgramID, "normalSampler");
            int alphaMap = GL.GetUniformLocation(deferredParticleShader.ProgramID, "alphaSampler");
            int environMap = GL.GetUniformLocation(deferredParticleShader.ProgramID, "environmentSampler");
            GL.Uniform1(albedoText, 0);
            GL.Uniform1(normalMap, 1);
            GL.Uniform1(alphaMap, 2);
            GL.Uniform1(environMap, 3);
            GL.ActiveTexture(TextureUnit.Texture0);
            if (geometry.albedoTexture != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.albedoTexture.ID);
            }

            GL.ActiveTexture(TextureUnit.Texture1);
            if (geometry.normalMap != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.normalMap.ID);
            }
            GL.ActiveTexture(TextureUnit.Texture2);
            if (geometry.alphaMap != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.alphaMap.ID);
            }
            GL.ActiveTexture(TextureUnit.Texture3);
            if (geometry.environmentMap != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.environmentMap.ID);
            }
            //paricleSystem.RegisterParticleData(deferredParticleShader);
            //Draw Gemetry
            geometry.GetMesh().Draw(paricleSystem.NumberOfSpawnedParticles());
            //Deactivate Textures of FBO
            for (int i = textAmount - 1; i >= 0; i--)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                mainFBO.Textures[i].Deactivate();
            }
            deferredParticleShader.Deactivate();
            GL.Disable(EnableCap.Blend);
            GL.DepthMask(true);
        }

        public void FinishGeometryPass()
        {

            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
            
            mainFBO.Deactivate();
            
        }
        #endregion

        #region LightViewPass
        public void StartLightViewPass()
        {
            //Create ShadowMap
            lightViewFBO.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.FRONT_SIDE));
        }

        public void DrawShadowLightView(Camera camera, Renderable geometry)
        {
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(geometry.faceCullingMode));
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            if (geometry.hasAlphaMap == 1)
            {
                GL.Enable(EnableCap.Blend);
                GL.DepthMask(false);
            }
            shadowLightViewShader.Activate();
            //GL.ActiveTexture(TextureUnit.Texture0);
            lightViewFBO.Texture.Activate();
            //Render

            shadowLightViewShader.Uniform("shadowMapExponent", shadowExponent);
            shadowLightViewShader.Uniform("lightCamera", camera.GetMatrix());
            
            shadowLightViewShader.Uniform("hasHeightMap", geometry.hasHeightMap);
            shadowLightViewShader.Uniform("heightScaleFactor", geometry.heightScaleFactor);
            shadowLightViewShader.Uniform("hasAlphaMap", geometry.hasAlphaMap);
            shadowLightViewShader.Uniform("isUnlit", geometry.unlit);
            if (geometry.instances > 0)
            {
                shadowLightViewShader.Uniform("hasInstances", 1);
            }
            else
            {
                shadowLightViewShader.Uniform("hasInstances", 0);
            }


            int heightMap = GL.GetUniformLocation(shadowLightViewShader.ProgramID, "heightSampler");
            
            GL.Uniform1(heightMap, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            if (geometry.heightMap != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.heightMap.ID);
            }

            int alphaMap = GL.GetUniformLocation(shadowLightViewShader.ProgramID, "alphaSampler");
            GL.Uniform1(alphaMap, 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            if(geometry.alphaMap != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.alphaMap.ID);
            }
            if(geometry.instances > 0)
            {
                geometry.GetLightViewMesh().Draw(geometry.instances);
            }
            else
            {
                geometry.GetLightViewMesh().Draw();
            }
            //GL.ActiveTexture(TextureUnit.Texture0);
            lightViewFBO.Texture.Deactivate();
            shadowLightViewShader.Deactivate();
            GL.Disable(EnableCap.Blend);
            if(geometry.hasAlphaMap == 1)
            {
                GL.DepthMask(true);
            }
            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
        }

        public void DrawShadowLightViewParticle(Camera camera, Renderable geometry, ParticleSystem particleSystem)
        {
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(geometry.faceCullingMode));
            //GL.DepthMask(false);
            if (geometry.hasAlphaMap == 1)
            {
                GL.Enable(EnableCap.Blend);
                GL.DepthMask(false);
            }
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            shadowLightViewShaderParticle.Activate();
            //GL.ActiveTexture(TextureUnit.Texture0);
            lightViewFBO.Texture.Activate();
            //Render

            shadowLightViewShaderParticle.Uniform("shadowMapExponent", shadowExponent);
            shadowLightViewShaderParticle.Uniform("lightCamera", camera.GetMatrix());
            shadowLightViewShaderParticle.Uniform("lightViewMatrix", camera.GetViewMatrix());
            shadowLightViewShaderParticle.Uniform("hasAlphaMap", geometry.hasAlphaMap);

            int alphaMap = GL.GetUniformLocation(shadowLightViewShaderParticle.ProgramID, "alphaSampler");
            GL.Uniform1(alphaMap, 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            if (geometry.alphaMap != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.alphaMap.ID);
            }

            //particleSystem.RegisterParticleData(shadowLightViewShaderParticle);
            geometry.GetMesh().Draw(particleSystem.NumberOfSpawnedParticles());
            //GL.ActiveTexture(TextureUnit.Texture0);
            lightViewFBO.Texture.Deactivate();
            shadowLightViewShaderParticle.Deactivate();
            GL.Disable(EnableCap.Blend);
            GL.DepthMask(true);
            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
        }



        public void FinishLightViewPass()
        {
            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
            lightViewFBO.Deactivate();
            satFilter.FilterTexture(lightViewFBO.Texture);
        }

        #endregion

        #region ShadowMapPass
        public void StartShadowMapPass()
        {
            shadowMapFBO.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
        }

        public void CreateShadowMap(Matrix4x4 cameraMatrix, Camera lightViewCamera, Renderable geometry, Vector3 lightDir)
        {
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(geometry.faceCullingMode));
            if (geometry.hasAlphaMap == 1)
            {
                GL.Enable(EnableCap.Blend);
                GL.DepthMask(false);
            }
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            shadowMapShader.Activate();

            satFilter.GetFilterTexture().Activate();
            shadowMapShader.Uniform("shadowMapExponent", shadowExponent);
            shadowMapShader.Uniform("camera", cameraMatrix);
            shadowMapShader.Uniform("lightCamera", lightViewCamera.GetMatrix());
            shadowMapShader.Uniform("lightDirection", lightDir);

            shadowMapShader.Uniform("hasHeightMap", geometry.hasHeightMap);
            shadowMapShader.Uniform("heightScaleFactor", geometry.heightScaleFactor);
            shadowMapShader.Uniform("hasAlphaMap", geometry.hasAlphaMap);
            shadowMapShader.Uniform("isUnlit", geometry.unlit);

            if (geometry.instances > 0)
            {
                shadowMapShader.Uniform("hasInstances", 1);
            }
            else
            {
                shadowMapShader.Uniform("hasInstances", 0);
            }

            int heightMap = GL.GetUniformLocation(shadowMapShader.ProgramID, "heightSampler");

            GL.Uniform1(heightMap, 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            if (geometry.heightMap != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.heightMap.ID);
            }

            int normalMap = GL.GetUniformLocation(shadowMapShader.ProgramID, "normalSampler");
            GL.Uniform1(normalMap, 2);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, mainFBO.Textures[2].ID);

            int alphaMap = GL.GetUniformLocation(shadowMapShader.ProgramID, "alphaSampler");
            GL.Uniform1(alphaMap, 3);
            GL.ActiveTexture(TextureUnit.Texture3);
            if (geometry.alphaMap != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.alphaMap.ID);
            }

            if(geometry.instances > 0)
            {
                geometry.GetShadowMapMesh().Draw(geometry.instances);
            }
            else
            {
                geometry.GetShadowMapMesh().Draw();
            }
            shadowMapShader.Deactivate();
            GL.Disable(EnableCap.Blend);
            if(geometry.hasAlphaMap == 1)
            {
                GL.DepthMask(true);
            }
            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
        }

        public void CreateShadowMapParticle(Camera camera, Camera lightViewCamera, Renderable geometry, Vector3 lightDir, ParticleSystem particleSystem)
        {
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(geometry.faceCullingMode));
            if (geometry.hasAlphaMap == 1)
            {
                GL.Enable(EnableCap.Blend);
                GL.DepthMask(false);
            }
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            shadowMapShaderParticle.Activate();

            satFilter.GetFilterTexture().Activate();
            shadowMapShaderParticle.Uniform("shadowMapExponent", shadowExponent);
            shadowMapShaderParticle.Uniform("camera", camera.GetMatrix());
            shadowMapShaderParticle.Uniform("cameraViewMatrix", camera.GetViewMatrix());
            shadowMapShaderParticle.Uniform("lightCamera", lightViewCamera.GetMatrix());
            shadowMapShaderParticle.Uniform("lightViewMatrix", lightViewCamera.GetViewMatrix());
            shadowMapShaderParticle.Uniform("lightDirection", lightDir);

            shadowMapShaderParticle.Uniform("hasAlphaMap", geometry.hasAlphaMap);

            int normalMap = GL.GetUniformLocation(shadowMapShaderParticle.ProgramID, "normalSampler");
            GL.Uniform1(normalMap, 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, mainFBO.Textures[2].ID);

            int alphaMap = GL.GetUniformLocation(shadowMapShaderParticle.ProgramID, "alphaSampler");
            GL.Uniform1(alphaMap, 2);
            GL.ActiveTexture(TextureUnit.Texture2);
            if (geometry.alphaMap != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, geometry.alphaMap.ID);
            }

            //particleSystem.RegisterParticleData(shadowMapShaderParticle);
            geometry.GetMesh().Draw(particleSystem.NumberOfSpawnedParticles());

            //GL.ActiveTexture(TextureUnit.Texture0);
            //lightViewFBO.Textures[0].Deactivate();
            //satFilter.GetFilterTexture().Deactivate();
            shadowMapShaderParticle.Deactivate();
            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);
            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
        }

        public void FinishShadowMassPass()
        {
            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
            shadowMapFBO.Deactivate();
        }

        #endregion

        #region PointLightPass
        public void PointLightPass(Matrix4x4 cameraMatrix, Vector3 cameraPosition, Vector3 cameraDirection)
        {
            pointLightFBO.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderState.Set(new DepthTest(false));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.FRONT_SIDE));
            renderState.Set(BlendStates.Additive);
            pointLightShader.Activate();

            int position = GL.GetUniformLocation(pointLightShader.ProgramID, "positionSampler");
            int albedo = GL.GetUniformLocation(pointLightShader.ProgramID, "albedoSampler");
            int normal = GL.GetUniformLocation(pointLightShader.ProgramID, "normalSampler");

            GL.Uniform1(position, 1);
            GL.Uniform1(albedo, 2);
            GL.Uniform1(normal, 3);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, mainFBO.Textures[0].ID);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, mainFBO.Textures[1].ID);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, mainFBO.Textures[2].ID);


            pointLightShader.Uniform("camera", cameraMatrix);
            pointLightShader.Uniform("cameraPosition", cameraPosition );
            pointLightShader.Uniform("camDir", cameraDirection);
            GL.ActiveTexture(TextureUnit.Texture0);
            pointLightFBO.Textures[0].Activate();

            DrawBuffersEnum[] drawBuffers = new DrawBuffersEnum[1];

            drawBuffers[0] = DrawBuffersEnum.ColorAttachment0;
            GL.DrawBuffers(1, drawBuffers);

            pointLightSphere.Draw(pointLightAmount);

            GL.ActiveTexture(TextureUnit.Texture0);
            pointLightFBO.Textures[0].Deactivate();

            pointLightShader.Deactivate();
            renderState.Set(BlendStates.Opaque);
            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
            pointLightFBO.Deactivate();
        }
        #endregion

        #region FinalPass
        public void FinalPass(Vector3 cameraPosition, Vector4 ambientColor, DirectionalLight dirLight, Vector3 cameraDirection, bool renderToTexture)
        {
            int textAmount = finalPassFBO.Textures.Count; //Number of Texture Channels of FBO
            if (renderToTexture)
            {
                finalPassFBO.Activate();

                for (int i = 0; i < textAmount; i++)
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + i);
                    finalPassFBO.Textures[i].Activate();
                }

                DrawBuffersEnum[] buffers = new DrawBuffersEnum[textAmount];
                for (int i = 0; i < textAmount; i++)
                {
                    buffers[i] = DrawBuffersEnum.ColorAttachment0 + i;
                }

                GL.DrawBuffers(textAmount, buffers);
            }
            deferredPost.Activate();
            //renderState.Set(BlendStates.Additive);

            //Activate Textures of FBO
            



            int position = GL.GetUniformLocation(deferredPost.ProgramID, "positionSampler");
            int albedo = GL.GetUniformLocation(deferredPost.ProgramID, "albedoSampler");
            int normal = GL.GetUniformLocation(deferredPost.ProgramID, "normalSampler");
            int lights = GL.GetUniformLocation(deferredPost.ProgramID, "pointLightSampler");
            int shadows = GL.GetUniformLocation(deferredPost.ProgramID, "shadowSampler");
            int unlit = GL.GetUniformLocation(deferredPost.ProgramID, "unlitSampler");

            GL.Uniform1(position, 1);
            GL.Uniform1(albedo, 2);
            GL.Uniform1(normal, 3);
            GL.Uniform1(lights, 4);
            GL.Uniform1(shadows, 5);
            GL.Uniform1(unlit, 6);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, mainFBO.Textures[0].ID);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, mainFBO.Textures[1].ID);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, mainFBO.Textures[2].ID);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, pointLightFBO.Textures[0].ID);
            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, shadowMapFBO.Texture.ID);
            GL.ActiveTexture(TextureUnit.Texture6);
            GL.BindTexture(TextureTarget.Texture2D, mainFBO.Textures[4].ID);

            //Pass Parameters
            deferredPost.Uniform("ambientColor", ambientColor);
            deferredPost.Uniform("camPos", cameraPosition);
            deferredPost.Uniform("camDir", cameraDirection);


            deferredPost.Uniform("dirLightDir", dirLight.direction);
            deferredPost.Uniform("dirLightCol", dirLight.lightColor);
            deferredPost.Uniform("dirSpecCol", dirLight.specularColor);
            deferredPost.Uniform("dirIntensity", dirLight.intensity);
            deferredPost.Uniform("dirSpecIntensity", dirLight.specularIntensity);
            deferredPost.Uniform("specFactor", dirLight.specularFactor);

            //PostProcessQuad
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            deferredPost.Deactivate();
            renderState.Set(BlendStates.Opaque);
            if (renderToTexture)
            {
                for (int i = textAmount - 1; i >= 0; i--)
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + i);
                    mainFBO.Textures[i].Deactivate();
                }

                finalPassFBO.Deactivate();
                //TextureDebugger.Draw(finalPassFBO.Texture);
            }
        }
        #endregion
    }
}
