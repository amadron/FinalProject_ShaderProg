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

namespace Example
{
    class TestVisual
    {
        public TestVisual(Zenseless.HLGL.IRenderState renderState, Zenseless.HLGL.IContentLoader contentLoader)
        {
            renderState.Set<DepthTest>(new DepthTest(true));
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            Vector3 pos = campos;
            fCam.Position = pos;
            fCam.Heading = camrot.Y;
            fCam.Tilt = camrot.X;
            fCam.NearClip = 0.1f;
            fCam.FarClip = 50.0f;
            phongShading = contentLoader.Load<IShaderProgram>("phong.*");
            deferredShading = contentLoader.Load<IShaderProgram>("deferred.*");
            deferredPost = contentLoader.LoadPixelShader("deferred_post");
            var mesh = Meshes.CreatePlane(5, 5, 1, 1);
            var sphere = Meshes.CreateSphere(1, 2);
            sphere.SetConstantUV(new Vector2(0, 0));
            mesh.Add(sphere.Transform(Transformation.Translation(new Vector3(0, 1f, 0))));
            geometryPhong = VAOLoader.FromMesh(mesh, phongShading);
            geometryDeferred = VAOLoader.FromMesh(mesh, deferredShading);
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
            phongShading.Uniform("specFactor", 255);
            geometryPhong.Draw();
            phongShading.Deactivate();
            
            
        }

        public void RenderDeferred()
        {

        }

        public void DrawDeferredGeometry()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            deferredShading.Activate();

            //Activate Textures of FBO
            int textAmount = renderToTexture.Textures.Count; //Number of Texture Channels of FBO
            for(int i = 0; i < textAmount; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                renderToTexture.Textures[i].Activate();
            }

            //Draw Gemetry


            //Deactivate Textures of FBO
            for(int i  = textAmount; i >= 0; i--)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                renderToTexture.Textures[i].Deactivate();
            }


            deferredShading.Deactivate();
        }

        public void Resize(int width, int height)
        {
            renderToTexture = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));
            renderToTexture.Attach(Texture2dGL.Create(width, height, 4, true));
            foreach(ITexture text in renderToTexture.Textures)
            {
                text.WrapFunction = TextureWrapFunction.MirroredRepeat;
            }
        }

        public void Update(float deltatime)
        {
            
            KeyboardState kstate = Keyboard.GetState();
            if(kstate.IsKeyDown(Key.A))
            {
                MoveCam(new Vector3(-1 * deltatime, 0, 0));
            }
            if(kstate.IsKeyDown(Key.D))
            {
                MoveCam(new Vector3(1 * deltatime, 0, 0));
            }
            if(kstate.IsKeyDown(Key.W))
            {
                MoveCam(new Vector3(0, 0, 1 * deltatime));
            }
            if(kstate.IsKeyDown(Key.S))
            {
                MoveCam(new Vector3(0, 0, -1 * deltatime));
            }
            if(kstate.IsKeyDown(Key.Q))
            {
                MoveCam(new Vector3(0, 1 * deltatime, 0));
            }
            if(kstate.IsKeyDown(Key.E))
            {
                MoveCam(new Vector3(0, -1 * deltatime, 0));
            }
            if(kstate.IsKeyDown(Key.Up))
            {
                RotateCam(new Vector2(-rotSpeedX * deltatime, 0));
            }
            if(kstate.IsKeyDown(Key.Down))
            {
                RotateCam(new Vector2(rotSpeedX * deltatime, 0));
            }
            if(kstate.IsKeyDown(Key.Left))
            {
                RotateCam(new Vector2(0, -rotSpeedY * deltatime));
            }
            if(kstate.IsKeyDown(Key.Right))
            {
                RotateCam(new Vector2(0, rotSpeedY * deltatime));
            }
            MouseState mstate = Mouse.GetState();
            
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
        //Phong
        IShaderProgram phongShading;
        IDrawable geometryPhong;
        //PostProcessing
        //Deferred
        IShaderProgram deferredShading;
        IShaderProgram deferredPost;
        private IRenderSurface renderToTexture;
        IDrawable geometryDeferred;
        //Shading
        Vector4 ambientColor = new Vector4(0.419f, 0.4f, 0.341f, 1);
        Vector3 dirLightdir = new Vector3(0.1f, -0.2f, -1f);
        Vector4 dirLightCol = new Vector4(0.866f, 0.878f, 0.243f, 1);
        Vector4 dirSpecCol = new Vector4(1, 1, 1, 1);
        //Shadows
        //Shadowmap
        //ShadowRendering
    }
}
