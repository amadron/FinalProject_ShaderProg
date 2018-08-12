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

namespace Example.src.model.graphics.rendering
{
    class DeferredRenderer
    {
        public DeferredRenderer(IContentLoader contentLoader, IRenderState renderState)
        {
            renderState.Set<DepthTest>(new DepthTest(true));
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            this.renderState = renderState;
            this.contentLoader = contentLoader;
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

        IShaderProgram deferredGeometryShader;
        IShaderProgram deferredPost;
        IShaderProgram pointLightShader;
        IShaderProgram shadowMapShader;
        IShaderProgram shadowLightViewShader;

        SATGpuFilter satFilter;

        public IDrawable GetDrawable(Mesh mesh)
        {
            return VAOLoader.FromMesh(mesh, deferredGeometryShader);
        }

        public void Resize(int width, int height)
        {
            mainFBO = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));
            mainFBO.Attach(Texture2dGL.Create(width, height, 4, true));
            mainFBO.Attach(Texture2dGL.Create(width, height, 4, true));
            foreach (ITexture2D text in mainFBO.Textures)
            {
                text.WrapFunction = TextureWrapFunction.MirroredRepeat;
            }
            pointLightFBO = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));
            pointLightFBO.Texture.WrapFunction = TextureWrapFunction.MirroredRepeat;

            lightViewFBO = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));
            lightViewFBO.Texture.WrapFunction = TextureWrapFunction.MirroredRepeat;

            shadowMapFBO = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));
            shadowMapFBO.Texture.WrapFunction = TextureWrapFunction.MirroredRepeat;

            satFilter = new SATGpuFilter(contentLoader, renderState, 32, 32, width, height, 4, 4);
        }


        
        public void SetPointLights(PointLight[] pointLights)
        {
            Vector3[] instPos = new Vector3[pointLights.Length];
            Vector4[] instCols = new Vector4[pointLights.Length];
            float[] instRadius = new float[pointLights.Length];
            float[] instIntensity = new float[pointLights.Length];

            Vector4[] instSpecCol = new Vector4[pointLights.Length];
            float[] instSpecFact = new float[pointLights.Length];
            float[] instSpecIntensity = new float[pointLights.Length];

            for (int i = 0; i < pointLights.Length; i++)
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
            pointLightAmount = pointLights.Length;
        }


        public void StartGeometryPass()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            mainFBO.Activate();
        }





        public void DrawDeferredGeometry(IDrawable geometry, CameraFirstPerson camera)
        {

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.BACK_SIDE));

            deferredGeometryShader.Activate();

            deferredGeometryShader.Uniform("camera", camera.CalcMatrix());
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

            //Draw Gemetry
            geometry.Draw();
            //Deactivate Textures of FBO
            for (int i = textAmount - 1; i >= 0; i--)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                mainFBO.Textures[i].Deactivate();
            }
            deferredGeometryShader.Deactivate();
        }
    
        public void FinishGeometryPass()
        {

            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
            mainFBO.Deactivate();
        }

        public void StartLightViewPass()
        {
            //Create ShadowMap
            lightViewFBO.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.BACK_SIDE));
        }

        public void DrawShadowLightView(Camera<Orbit, Perspective> camera, IDrawable geometry)
        {
            shadowLightViewShader.Activate();
            //GL.ActiveTexture(TextureUnit.Texture0);
            lightViewFBO.Texture.Activate();
            //Render
            shadowLightViewShader.Uniform("lightCamera", camera);
            geometry.Draw();
            //GL.ActiveTexture(TextureUnit.Texture0);
            lightViewFBO.Texture.Deactivate();
            shadowLightViewShader.Deactivate();
        }

        public void FinishLightViewPass()
        {
            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
            lightViewFBO.Deactivate();
            satFilter.FilterTexture(lightViewFBO.Texture);
        }

        public void StartShadowMapPass()
        {
            shadowMapFBO.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.BACK_SIDE));
        }

        public void CreateShadowMap(CameraFirstPerson viewCamera, Camera<Orbit, Perspective> lightViewCamera, IDrawable geometry)
        {
            shadowMapShader.Activate();

            satFilter.GetFilterTexture().Activate();
            shadowMapShader.Uniform("camera", viewCamera.CalcMatrix());
            shadowMapShader.Uniform("lightCamera", lightViewCamera);
            geometry.Draw();

            //GL.ActiveTexture(TextureUnit.Texture0);
            lightViewFBO.Textures[0].Deactivate();
            //satFilter.GetFilterTexture().Deactivate();
            shadowMapShader.Deactivate();
        }

        public void FinishShadowMassPass()
        {
            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
            shadowMapFBO.Deactivate();
        }

        public void PointLightPass(CameraFirstPerson camera, Vector3 cameraPosition)
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


            pointLightShader.Uniform("camera", camera.CalcMatrix());
            pointLightShader.Uniform("cameraPosition", cameraPosition );
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

        public void FinalPass(Vector3 cameraPosition, Vector4 ambientColor, DirectionalLight dirLight)
        {
            deferredPost.Activate();

            int position = GL.GetUniformLocation(deferredPost.ProgramID, "positionSampler");
            int albedo = GL.GetUniformLocation(deferredPost.ProgramID, "albedoSampler");
            int normal = GL.GetUniformLocation(deferredPost.ProgramID, "normalSampler");
            int lights = GL.GetUniformLocation(deferredPost.ProgramID, "pointLightSampler");
            int shadows = GL.GetUniformLocation(deferredPost.ProgramID, "shadowSampler");

            GL.Uniform1(position, 0);
            GL.Uniform1(albedo, 1);
            GL.Uniform1(normal, 2);
            GL.Uniform1(lights, 3);
            GL.Uniform1(shadows, 4);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, mainFBO.Textures[0].ID);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, mainFBO.Textures[1].ID);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, mainFBO.Textures[2].ID);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, pointLightFBO.Textures[0].ID);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, shadowMapFBO.Texture.ID);

            //Pass Parameters
            deferredPost.Uniform("camPos", cameraPosition);

            deferredPost.Uniform("ambientColor", ambientColor);

            deferredPost.Uniform("dirLightDir", dirLight.direction);
            deferredPost.Uniform("dirLightCol", dirLight.lightColor);
            deferredPost.Uniform("dirSpecCol", dirLight.specularColor);
            deferredPost.Uniform("dirIntensity", dirLight.intensity);
            deferredPost.Uniform("dirSpecIntensity", dirLight.specularIntensity);
            deferredPost.Uniform("specFactor", dirLight.specularFactor);

            //PostProcessQuad
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            deferredPost.Deactivate();
        }
        
    }
}
