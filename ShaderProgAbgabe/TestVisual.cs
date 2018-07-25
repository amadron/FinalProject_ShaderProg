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

namespace Example
{
    class TestVisual
    {
        IRenderState renderState;
        public TestVisual(Zenseless.HLGL.IRenderState renderState, Zenseless.HLGL.IContentLoader contentLoader)
        {
            renderState.Set<DepthTest>(new DepthTest(true));
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            this.renderState = renderState;
            //Camera Setup
            Vector3 pos = campos;
            fCam.Position = pos;
            fCam.Heading = camrot.Y;
            fCam.Tilt = camrot.X;
            fCam.NearClip = 0.001f;
            fCam.FarClip = 50.0f;

            phongShading = contentLoader.Load<IShaderProgram>("phong.*");
            deferredShading = contentLoader.Load<IShaderProgram>("deferred.*");
            deferredPost = contentLoader.LoadPixelShader("deferred_post");
            var mesh = Meshes.CreatePlane(5, 5, 1, 1);
            var sphere = Meshes.CreateSphere(1, 2);

            //Lights
            defPointLightShader = contentLoader.Load<IShaderProgram>("def_pointLight.*");
            var lSphere = Meshes.CreateSphere(1, 1);
            pointLightSphere = VAOLoader.FromMesh(lSphere, defPointLightShader);
            pointLights = GetPointLights().ToArray();
            Vector3[] instPos = new Vector3[pointLights.Length];
            Vector4[] instCols = new Vector4[pointLights.Length];
            float[] instRadius = new float[pointLights.Length];
            for(int i = 0; i < pointLights.Length; i++)
            {
                instPos[i] = pointLights[i].position;
                instCols[i] = new Vector4(pointLights[i].lightColor.ToVector3(), 1);
                instRadius[i] = pointLights[i].radius;
            }
            pointLightSphere.SetAttribute(GL.GetAttribLocation(defPointLightShader.ProgramID, "instancePosition"), instPos, true);
            pointLightSphere.SetAttribute(GL.GetAttribLocation(defPointLightShader.ProgramID, "instanceColor"), instCols, true);
            pointLightSphere.SetAttribute(GL.GetAttribLocation(defPointLightShader.ProgramID, "instanceRadius"), instRadius, true);

            sphere.SetConstantUV(new Vector2(0, 0));
            mesh.Add(sphere.Transform(Transformation.Translation(new Vector3(0, 1f, 0))));
            geometryPhong = VAOLoader.FromMesh(mesh, phongShading);
            geometryDeferred = VAOLoader.FromMesh(mesh, deferredShading);
        }

        List<PointLight> GetPointLights()
        {
            List<PointLight> lightList = new List<PointLight>();
            
            return lightList;
        }

        public void RenderPhong()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            phongShading.Activate();
            phongShading.Uniform("camera", fCam.CalcMatrix());
            phongShading.Uniform("ambientColor", ambientColor);
            phongShading.Uniform("camPos", campos);
            phongShading.Uniform("dirLightDir", Vector3.Normalize(dirLightdir));
            phongShading.Uniform("dirLightCol", dirLightCol);
            phongShading.Uniform("dirSpecCol", dirSpecCol);
            phongShading.Uniform("specFactor", 90);
            geometryPhong.Draw();
            phongShading.Deactivate();
            
            
        }

        public void RenderDeferred()
        {

            //Render to Framebufferobject
            renderToTexture.Activate();
            DrawDeferredGeometry();
            renderToTexture.Deactivate();

            //TextureDebugger.Draw(renderToTexture.Textures[0]);
            //return;

            //DeferredLightning
            deferredPost.Activate();
            int position = GL.GetUniformLocation(deferredPost.ProgramID, "positionSampler");
            int albedo = GL.GetUniformLocation(deferredPost.ProgramID, "albedoSampler");
            int normal = GL.GetUniformLocation(deferredPost.ProgramID, "normalSampler");

            GL.Uniform1(position, 0);
            GL.Uniform1(albedo, 1);
            GL.Uniform1(normal, 2);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, renderToTexture.Textures[0].ID);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, renderToTexture.Textures[1].ID);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, renderToTexture.Textures[2].ID);

            //Pass Parameters
            deferredPost.Uniform("camPos", campos);

            deferredPost.Uniform("ambientColor", ambientColor);

            deferredPost.Uniform("dirLightDir", dirLightdir);
            deferredPost.Uniform("dirLightCol", dirLightCol);
            deferredPost.Uniform("dirSpecCol", dirSpecCol);
            deferredPost.Uniform("specFactor", 255);
            //PostProcessQuad
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            deferredPost.Deactivate();
            
        }

        public void DrawDeferredGeometry()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.BACK_SIDE));


            deferredShading.Activate();

            deferredShading.Uniform("camera", fCam.CalcMatrix());
            //Activate Textures of FBO
            int textAmount = renderToTexture.Textures.Count; //Number of Texture Channels of FBO
            for(int i = 0; i < textAmount; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                renderToTexture.Textures[i].Activate();
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
                renderToTexture.Textures[i].Deactivate();
            }


            deferredShading.Deactivate();
            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
        }

        public void Resize(int width, int height)
        {
            renderToTexture = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));
            renderToTexture.Attach(Texture2dGL.Create(width, height, 4, true));
            renderToTexture.Attach(Texture2dGL.Create(width, height, 4, true));
            foreach (ITexture text in renderToTexture.Textures)
            {
                text.WrapFunction = TextureWrapFunction.MirroredRepeat;
            }
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
        private IRenderSurface renderToTexture;
        IDrawable geometryDeferred;
        //Shading
        Vector4 ambientColor = new Vector4(0.1f, 0.10f, 0.074f, 1);
        Vector3 dirLightdir = new Vector3(0.1f, -0.2f, -1f);
        Vector4 dirLightCol = new Vector4(0.866f, 0.878f, 0.243f, 1);
        Vector4 dirSpecCol = new Vector4(1, 1, 1, 1);
        //Shadows
        //Shadowmap
        //ShadowRendering

        public void Update(float deltatime)
        {

            KeyboardState kstate = Keyboard.GetState();
            if (kstate.IsKeyDown(Key.A))
            {
                MoveCam(new Vector3(-1 * deltatime, 0, 0));
            }
            if (kstate.IsKeyDown(Key.D))
            {
                MoveCam(new Vector3(1 * deltatime, 0, 0));
            }
            if (kstate.IsKeyDown(Key.W))
            {
                MoveCam(new Vector3(0, 0, 1 * deltatime));
            }
            if (kstate.IsKeyDown(Key.S))
            {
                MoveCam(new Vector3(0, 0, -1 * deltatime));
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
    }
}
