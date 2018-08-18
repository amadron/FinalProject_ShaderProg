using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Example.src.model;
using Example.src.model.graphics.camera;
using Example.src.model.graphics.rendering;
using Example.src.Test;
using OpenTK.Input;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace Example.src.controller
{
    class Game
    {
        DeferredRenderer renderer;
        IContentLoader contentLoader;
        IRenderState renderState;

        Camera activeCam;
        Scene activeScene;

        Vector3 campos = new Vector3(0, 3, 5f);
        Vector2 camrot = new Vector2(0, 0);
        //Vector3 campos = new Vector3(0, 1, 5f);
        //Vector2 camrot = new Vector2(25, 180);
        float rotSpeedY = 40;
        float rotSpeedX = 40;
        

        public Game(IContentLoader contentManager, IRenderState renderState)
        {
            this.renderState = renderState;
            this.contentLoader = contentManager;
            renderer = new DeferredRenderer(contentManager, renderState);
            activeScene = new TestScene(contentManager, renderer);
            renderer.SetPointLights(activeScene.getPointLights());
            activeCam = new FirstPersonCamera(campos, camrot.X, camrot.Y, Camera.ProjectionType.Perspective, fov:1f, width:10, height:10);

            /*
            activeCam = new CameraFirstPerson();
            activeCam.Position = campos;
            activeCam.Heading = camrot.Y;
            activeCam.Tilt = camrot.X;
            activeCam.NearClip = 0.1f;
            activeCam.FarClip = 50.0f;
            */
        }



        public void Update(float deltatime)
        {
            OpenTK.Vector4 dirVec = new OpenTK.Vector4(0, 0, 1, 1);
            OpenTK.Vector4 rightVec = new OpenTK.Vector4(1, 0, 0, 1);
            Matrix4x4 rot = activeCam.GetRotationMatrix();

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

        private void MoveCam(Vector3 move)
        {
            campos += move;
            activeCam.SetPosition(campos);
        }

        private void RotateCam(Vector2 rot)
        {
            camrot += rot;
            activeCam.SetRotation(new Vector3(camrot, 0));
        }

        public void Render()
        {
            RenderDeferred();
        }
        
        public void Resize(int width, int height)
        {
            renderer.Resize(width, height);
            activeCam.Resize(width, height);
            activeScene.Resize(width, height);
        }

        private Matrix4x4 GetCameraMatrix()
        {
            return activeCam.GetMatrix();
        }

        private void RenderDeferred()
        {
            Renderable[] geometry = activeScene.getGeometry();
            
            renderer.StartLightViewPass();
            for(int i = 0; i < geometry.Length; i++)
            {
                renderer.DrawShadowLightView(activeScene.GetDirectionalLightCamera(), geometry[i]);
            }
            renderer.FinishLightViewPass();
            
            renderer.StartShadowMapPass();
            for (int i = 0; i < geometry.Length; i++)
            {
                renderer.CreateShadowMap(GetCameraMatrix(), activeScene.GetDirectionalLightCamera(), geometry[i], activeScene.getDirectionalLight().direction);
            }
            renderer.FinishShadowMassPass();
            
            
            renderer.StartGeometryPass();
            for (int i = 0; i < geometry.Length; i++)
            {
                renderer.DrawDeferredGeometry(geometry[i], GetCameraMatrix(), campos);
            }
            renderer.FinishGeometryPass();
            
            renderer.PointLightPass(GetCameraMatrix(), campos);
            //TextureDebugger.Draw(renderer.lightViewFBO.Textures[0]);
            //TextureDebugger.Draw(renderer.shadowMapFBO.Textures[0]);
            //TextureDebugger.Draw(renderer.pointLightFBO.Textures[0]);
            //TextureDebugger.Draw(renderer.mainFBO.Textures[2]);
            renderer.FinalPass(campos, activeScene.GetAmbientColor(), activeScene.getDirectionalLight());
        }
    }
}
