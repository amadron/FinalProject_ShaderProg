using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Example.src.model;
using System.Drawing;
using Example.src.model.lightning;
using Example.src.controller;
using Example.src.model.entitys;

namespace Example
{
    class TestVisual
    {
        IRenderState renderState;
        IContentLoader contentLoader;
        public TestVisual(Zenseless.HLGL.IRenderState renderState, Zenseless.HLGL.IContentLoader contentLoader)
        {
            this.contentLoader = contentLoader;
            renderState.Set<DepthTest>(new DepthTest(true));
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            
            //GL.Enable(EnableCap.Blend);
            
            this.renderState = renderState;
            //Camera Setup
            Vector3 pos = campos;
            fCam.Position = pos;
            fCam.Heading = camrot.Y;
            fCam.Tilt = camrot.X;
            fCam.NearClip = 0.1f;
            fCam.FarClip = 50.0f;

            phongShading = contentLoader.Load<IShaderProgram>("phong.*");
            //Deferred
            //Deferred Shader
            deferredShading = contentLoader.Load<IShaderProgram>("deferred_geometry.*");
            deferredPost = contentLoader.LoadPixelShader("deferred_post");
            var mesh = Meshes.CreatePlane(5, 5, 10, 10);
            //var mesh = Meshes.CreateSphere(1, 2);
            var sphere = Meshes.CreateSphere(1, 2);
            var sphere2 = Meshes.CreateSphere(1, 2);

            //Lights
            defPointLightShader = contentLoader.Load<IShaderProgram>("def_pointLight.*");
            var lSphere = Meshes.CreateSphere(1, 2);
            pointLightSphere = VAOLoader.FromMesh(lSphere, defPointLightShader);
            pointLights = GetPointLights().ToArray();
            Vector3[] instPos = new Vector3[pointLights.Length];
            Vector4[] instCols = new Vector4[pointLights.Length];
            float[] instRadius = new float[pointLights.Length];
            float[] instIntensity = new float[pointLights.Length];

            Vector4[] instSpecCol = new Vector4[pointLights.Length];
            float[] instSpecFact = new float[pointLights.Length];
            float[] instSpecIntensity = new float[pointLights.Length];

            for(int i = 0; i < pointLights.Length; i++)
            {
                instPos[i] = pointLights[i].position;
                instCols[i] = pointLights[i].lightColor;
                instRadius[i] = pointLights[i].radius;
                instIntensity[i] = pointLights[i].intensity;

                instSpecCol[i] = pointLights[i].specularColor;
                instSpecFact[i] = pointLights[i].specularFactor;
                instSpecIntensity[i] = pointLights[i].specularIntensity;
            }
            pointLightSphere.SetAttribute(GL.GetAttribLocation(defPointLightShader.ProgramID, "instancePosition"), instPos, true);
            pointLightSphere.SetAttribute(GL.GetAttribLocation(defPointLightShader.ProgramID, "instanceColor"), instCols, true);
            pointLightSphere.SetAttribute(GL.GetAttribLocation(defPointLightShader.ProgramID, "instanceRadius"), instRadius, true);
            pointLightSphere.SetAttribute(GL.GetAttribLocation(defPointLightShader.ProgramID, "instanceIntensity"), instIntensity, true);

            pointLightSphere.SetAttribute(GL.GetAttribLocation(defPointLightShader.ProgramID, "instanceSpecularColor"), instSpecCol, true);
            pointLightSphere.SetAttribute(GL.GetAttribLocation(defPointLightShader.ProgramID, "instanceSpecularFactor"), instSpecFact, true);
            pointLightSphere.SetAttribute(GL.GetAttribLocation(defPointLightShader.ProgramID, "instanceSpecularIntensity"), instSpecIntensity, true);
            //PointLight end

            //Shadowmapping
            shadowMapLightViewShader = contentLoader.Load<IShaderProgram>("shadowLightView.*");
            shadowMapShader = contentLoader.Load<IShaderProgram>("shadowMap.*");
            dirLightCamera.View.Target = Vector3.Zero;

            //Scene/ Geometry
            sphere.SetConstantUV(new Vector2(0, 0));
            mesh.Add(sphere.Transform(Transformation.Translation(new Vector3(1f, 0.5f, -1f))));
            mesh.Add(sphere2.Transform(Transformation.Translation(new Vector3(-1f, 0.5f, 1f))));
            var cube = Meshes.CreateCubeWithNormals(1);
            mesh.Add(cube.Transform(Transformation.Translation(new Vector3(-0.5f, 0.5f, -1.5f))));
            geometryPhong = VAOLoader.FromMesh(mesh, phongShading);
            geometryDeferred = VAOLoader.FromMesh(mesh, deferredShading);
            fullScreenQuad = contentLoader.Load<IShaderProgram>("FullQuad.*");
            terrain = getTerrain();
        }

        List<PointLight> GetPointLights()
        {
            List<PointLight> lightList = new List<PointLight>();
            PointLight l = new PointLight(new Vector3(0, 0.4f, 0), new Vector4(Color.Green.ToVector3(), 1), 1f, 3f, new Vector4(1), 80, 0.6f);
            PointLight l2 = new PointLight(new Vector3(0.8f, 0.4f, 0.5f), new Vector4(Color.Red.ToVector3(),1), 1f, 3f);
            lightList.Add(l);
            lightList.Add(l2);
            return lightList;
        }

        public void RenderDeferred()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //DrawShadowMap
            DrawShadowMapPass();

            //Render Geometry Data to Framebuffer
            DrawDeferredGeometry();

            //Render Lights as Spheres to texture
            DrawDeferredPointLightPass();

            //TextureDebugger.Draw(renderToTextureShading.Textures[0]);
            //TextureDebugger.Draw(renderToTexturePointLights.Textures[0]);
            //satFilter.FilterTexture(renderToTextureDirectionalLightView.Texture);
            //TextureDebugger.Draw(renderToTextureDirectionalLightView.Texture);
            //TextureDebugger.Draw(renderToTextureShadowMap.Texture);
            //satFilter.Test();
            //satFilter.FilterTexture(renderToTextureShading.Textures[0]);
            //TextureDebugger.Draw(satFilter.GetFilterTexture());
            //Vector4[,] buff = new Vector4[satFilter.GetFilterTexture().Width, satFilter.GetFilterTexture().Height];
            //satFilter.GetFilterTexture().ToBuffer(ref buff);
            //return;
            //DeferredLightning
            DrawDeferredFinalPass();
        }

        public void DrawDeferredGeometry()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderToTextureShading.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.BACK_SIDE));
            //DrawDeferredTerrain(terrain);
            deferredShading.Activate();

            deferredShading.Uniform("camera", fCam.CalcMatrix());
            //Activate Textures of FBO
            int textAmount = renderToTextureShading.Textures.Count; //Number of Texture Channels of FBO
            for(int i = 0; i < textAmount; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                renderToTextureShading.Textures[i].Activate();
            }

            DrawBuffersEnum[] buffers = new DrawBuffersEnum[textAmount];
            for(int i = 0; i < textAmount; i++)
            {
                buffers[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }

            GL.DrawBuffers(textAmount, buffers);

            //Draw Gemetry
            geometryDeferred.Draw();
            //Deactivate Textures of FBO
            for(int i  = textAmount - 1; i >= 0; i--)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                renderToTextureShading.Textures[i].Deactivate();
            }
            deferredShading.Deactivate();
            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
            renderToTextureShading.Deactivate();
        }



        private void DrawDeferredPointLightPass()
        {
            renderToTexturePointLights.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderState.Set(new DepthTest(false));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.FRONT_SIDE));
            renderState.Set(BlendStates.Additive);
            defPointLightShader.Activate();

            int position = GL.GetUniformLocation(defPointLightShader.ProgramID, "positionSampler");
            int albedo = GL.GetUniformLocation(defPointLightShader.ProgramID, "albedoSampler");
            int normal = GL.GetUniformLocation(defPointLightShader.ProgramID, "normalSampler");

            GL.Uniform1(position, 1);
            GL.Uniform1(albedo, 2);
            GL.Uniform1(normal, 3);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, renderToTextureShading.Textures[0].ID);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, renderToTextureShading.Textures[1].ID);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, renderToTextureShading.Textures[2].ID);


            defPointLightShader.Uniform("camera", fCam.CalcMatrix());
            defPointLightShader.Uniform("cameraPosition", campos);
            GL.ActiveTexture(TextureUnit.Texture0);
            renderToTexturePointLights.Textures[0].Activate();

            DrawBuffersEnum[] drawBuffers = new DrawBuffersEnum[1];

            drawBuffers[0] = DrawBuffersEnum.ColorAttachment0;
            GL.DrawBuffers(1, drawBuffers);

            pointLightSphere.Draw(pointLights.Length);

            GL.ActiveTexture(TextureUnit.Texture0);
            renderToTexturePointLights.Textures[0].Deactivate();

            defPointLightShader.Deactivate();
            renderState.Set(BlendStates.Opaque);
            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
            renderToTexturePointLights.Deactivate();
        }

        private void DrawShadowMapPass()
        {
            //Create ShadowMap
            renderToTextureDirectionalLightView.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.BACK_SIDE));

            shadowMapLightViewShader.Activate();
            GL.ActiveTexture(TextureUnit.Texture0);
            renderToTextureDirectionalLightView.Texture.Activate();
            //Render
            shadowMapLightViewShader.Uniform("lightCamera", dirLightCamera);
            geometryDeferred.Draw();
            GL.ActiveTexture(TextureUnit.Texture0);
            renderToTextureDirectionalLightView.Texture.Deactivate();
            shadowMapLightViewShader.Deactivate();

            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
            renderToTextureDirectionalLightView.Deactivate();

            //Filter ShadowMap
            satFilter.FilterTexture(renderToTextureDirectionalLightView.Texture);

            //Create Shadow Map
            renderToTextureShadowMap.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.BACK_SIDE));

            shadowMapShader.Activate();

            //Render
            GL.ActiveTexture(TextureUnit.Texture0);
            //renderToTextureDirectionalLightView.Texture.Activate();
            satFilter.GetFilterTexture().Activate();
            shadowMapShader.Uniform("camera", fCam.CalcMatrix());
            shadowMapShader.Uniform("lightCamera", dirLightCamera);
            geometryDeferred.Draw();

            GL.ActiveTexture(TextureUnit.Texture0);
            renderToTextureDirectionalLightView.Texture.Deactivate();
            shadowMapShader.Deactivate();

            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
            renderToTextureShadowMap.Deactivate();
        }

        private void DrawDeferredFinalPass()
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
            GL.BindTexture(TextureTarget.Texture2D, renderToTextureShading.Textures[0].ID);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, renderToTextureShading.Textures[1].ID);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, renderToTextureShading.Textures[2].ID);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, renderToTexturePointLights.Textures[0].ID);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, renderToTextureShadowMap.Texture.ID);

            //Pass Parameters
            deferredPost.Uniform("camPos", campos);

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


        public void Resize(int width, int height)
        {
            //Deferred
            renderToTextureShading = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));
            renderToTextureShading.Attach(Texture2dGL.Create(width, height, 4, true));
            renderToTextureShading.Attach(Texture2dGL.Create(width, height, 4, true));
            foreach (ITexture text in renderToTextureShading.Textures)
            {
                text.WrapFunction = TextureWrapFunction.MirroredRepeat;
            }
            renderToTexturePointLights = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));
            renderToTexturePointLights.Texture.WrapFunction = TextureWrapFunction.MirroredRepeat;
            //Shadowmap
            renderToTextureDirectionalLightView = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));
            renderToTextureDirectionalLightView.Texture.WrapFunction = TextureWrapFunction.MirroredRepeat;

            renderToTextureShadowMap = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));
            renderToTextureShadowMap.Texture.WrapFunction = TextureWrapFunction.MirroredRepeat;

            satFilter = new SATGpuFilter(contentLoader, renderState,16, 16, width, height, 6, 6);
        }

        private void MoveCam(Vector3 move)
        {
            campos += move;
            fCam.Position = campos;
        }

        private void RotateCam(Vector2 rot)
        {
            camrot += rot;
            fCam.Tilt = camrot.X;
            fCam.Heading = camrot.Y;
        }

        //Main
        IShaderProgram fullScreenQuad;
        CameraFirstPerson fCam = new CameraFirstPerson();
        Vector3 campos = new Vector3(0, 3, 5f);
        Vector2 camrot = new Vector2(0, 0);
        float rotSpeedY = 40;
        float rotSpeedX = 40;
        PointLight[] pointLights;

        //Phong
        IShaderProgram phongShading;
        IDrawable geometryPhong;
        //PostProcessing
        //Deferred
        IShaderProgram defPointLightShader;
        VAO pointLightSphere;
        IShaderProgram deferredShading;
        IShaderProgram deferredPost;
        private IRenderSurface renderToTextureShading;
        private IRenderSurface renderToTexturePointLights;
        IDrawable geometryDeferred;
        //Shading
        DirectionalLight dirLight = new DirectionalLight(new Vector4(1f, 0.968f, 0.878f, 1), new Vector3(0.1f, -0.5f, 1f), 0.5f, new Vector4(1, 1, 1, 1), 255, 0);
        Vector4 ambientColor = new Vector4(0.1f, 0.10f, 0.074f, 1);
        //Shadows
        Camera<Orbit, Perspective> dirLightCamera = new Camera<Orbit, Perspective>(new Orbit(4.3f, 180, 45), new Perspective(farClip: 50));
        IShaderProgram shadowMapLightViewShader;
        IShaderProgram shadowMapShader;
        IRenderSurface renderToTextureDirectionalLightView;
        IRenderSurface renderToTextureShadowMap;
        SATGpuFilter satFilter;
        //Shadowmap
        //ShadowRendering

        public void Update(float deltatime)
        {
            OpenTK.Vector4 dirVec = new OpenTK.Vector4(0, 0, 1, 1);
            OpenTK.Vector4 rightVec = new OpenTK.Vector4(1, 0, 0, 1);
            Matrix4x4 rot = fCam.CalcRotationMatrix();
            
            OpenTK.Matrix4 testMat = new OpenTK.Matrix4(rot.M11, rot.M12, rot.M13, rot.M14,
                rot.M21, rot.M22, rot.M23, rot.M24,
                rot.M31, rot.M32, rot.M33, rot.M34,
                rot.M41, rot.M42, rot.M43, rot.M44);
            testMat.Transpose();
            dirVec = testMat * dirVec;
            rightVec = testMat * rightVec;
            dirVec.Normalize();
            rightVec.Normalize();
            Vector3 dirVecConvert = new Vector3(dirVec.X, dirVec.Y, dirVec.Z);
            Vector3 rightVecConvert = new Vector3(rightVec.X, rightVec.Y, rightVec.Z);
            KeyboardState kstate = Keyboard.GetState();
            if (kstate.IsKeyDown(Key.A))
            {
                MoveCam(rightVecConvert * deltatime * -1);
            }
            if (kstate.IsKeyDown(Key.D))
            {
                MoveCam(rightVecConvert * deltatime * 1);
            }
            if (kstate.IsKeyDown(Key.W))
            {
                MoveCam(dirVecConvert * -1 * deltatime);
            }
            if (kstate.IsKeyDown(Key.S))
            {
                MoveCam(dirVecConvert * 1 * deltatime);
            }
            if (kstate.IsKeyDown(Key.Q))
            {
                MoveCam(new Vector3(0, 1 * deltatime, 0));
            }
            if (kstate.IsKeyDown(Key.E))
            {
                MoveCam(new Vector3(0, -1 * deltatime, 0));
            }
            if (kstate.IsKeyDown(Key.Up))
            {
                RotateCam(new Vector2(-rotSpeedX * deltatime, 0));
            }
            if (kstate.IsKeyDown(Key.Down))
            {
                RotateCam(new Vector2(rotSpeedX * deltatime, 0));
            }
            if (kstate.IsKeyDown(Key.Left))
            {
                RotateCam(new Vector2(0, -rotSpeedY * deltatime));
            }
            if (kstate.IsKeyDown(Key.Right))
            {
                RotateCam(new Vector2(0, rotSpeedY * deltatime));
            }
            MouseState mstate = Mouse.GetState();

        }

        public void RenderPhong()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            phongShading.Activate();
            phongShading.Uniform("camera", fCam.CalcMatrix());
            phongShading.Uniform("ambientColor", ambientColor);
            phongShading.Uniform("camPos", campos);
            phongShading.Uniform("dirLightDir", Vector3.Normalize(dirLight.direction));
            phongShading.Uniform("dirLightCol", dirLight.lightColor);
            phongShading.Uniform("dirSpecCol", dirLight.specularColor);
            phongShading.Uniform("specFactor", 90);
            geometryPhong.Draw();
            phongShading.Deactivate();


        }

        Terrain terrain;

        private Terrain getTerrain()
        {
            Terrain res = new Terrain(contentLoader, new Vector2(5, 5), new Vector2(10, 10));
            ITexture2D hmap = contentLoader.Load<ITexture2D>("Heightmap.*");
            res.heightMap = hmap;
            return res;
        }

        public void DrawDeferredTerrain(Terrain terrain)
        {
            IShaderProgram shader = terrain.shader;
            shader.Activate();

            shader.Uniform("camera", fCam.CalcMatrix());
            shader.Uniform("heightScale", 1);
            int heightMap = GL.GetUniformLocation(shader.ProgramID, "heightMap");
            GL.Uniform1(heightMap, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, heightMap);
            //Activate Textures of FBO
            int textAmount = renderToTextureShading.Textures.Count; //Number of Texture Channels of FBO
            for (int i = 0; i < textAmount; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                renderToTextureShading.Textures[i].Activate();
            }

            DrawBuffersEnum[] buffers = new DrawBuffersEnum[textAmount];
            for (int i = 0; i < textAmount; i++)
            {
                buffers[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }

            GL.DrawBuffers(textAmount, buffers);

            //Draw Gemetry
            terrain.mesh.Draw();
            //Deactivate Textures of FBO
            for (int i = textAmount - 1; i >= 0; i--)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                renderToTextureShading.Textures[i].Deactivate();
            }
            shader.Deactivate();
            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
            renderToTextureShading.Deactivate();
        }
    }
}
