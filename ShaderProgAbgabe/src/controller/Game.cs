using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Example.src.model;
using Example.src.model.entitys;
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

        Vector3 campos = new Vector3(0, 3, -5f);
        Vector2 camrot = new Vector2(0, 180);
        //Vector3 campos = new Vector3(0, 1, 10f);
        //Vector2 camrot = new Vector2(25, 180);
        float rotSpeedY = 40;
        float rotSpeedX = 40;
        DateTime startTime;
        Water water;
        Entity waterEntity;

        public Game(IContentLoader contentManager, IRenderState renderState)
        {
            this.renderState = renderState;
            this.contentLoader = contentManager;
            renderer = new DeferredRenderer(contentManager, renderState);
            activeScene = new IslandScene(contentManager, renderer);
            waterEntity = activeScene.GetEntityByName("water");
            waterEntity.renderable.heightScaleFactor = 0.03f;
            renderer.SetPointLights(activeScene.getPointLights());
            activeCam = new FirstPersonCamera(campos, camrot.X, camrot.Y, Camera.ProjectionType.Perspective, fov:1f, width:20, height:20);
            startTime = DateTime.Now;
            water = new Water(contentLoader);
        }



        public void Update(float deltatime)
        {
            UpdateControl(deltatime);
            activeScene.Update(deltatime);
        }

        bool mouseClicked = false;
        Vector2 mouseClickedPos;
        Vector2 mouseSpeed = new Vector2(0.5f, 0.5f);
        Vector3 movementSpeed = new Vector3(1f, 1f, 1f);
        RenderMode currentRenderMode = RenderMode.deferred;
        float shiftSpeedFactor = 3f;
        enum RenderMode { deferred, postion, color, normal, pointlight, shadow, directional };

        private void UpdateControl(float deltatime)
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

            MouseState mstate = Mouse.GetState();
            Vector2 mpos = new Vector2(mstate.Y, mstate.X);
            if (mstate.IsButtonDown(MouseButton.Left))
            {
                if (!mouseClicked)
                {
                    mouseClicked = true;
                    mouseClickedPos = new Vector2(mstate.Y, mstate.X);
                }
                Vector2 mDiff = mpos - mouseClickedPos;
                RotateCam(mDiff * deltatime * mouseSpeed);
            }
            else
            {
                if(mouseClicked)
                {
                    mouseClicked = false;
                }
            }

            KeyboardState kstate = Keyboard.GetState();
            Vector3 tmpMovementSpeed = movementSpeed;
            if(kstate.IsKeyDown(Key.ShiftLeft))
            {
                tmpMovementSpeed *= shiftSpeedFactor;
            }
            if (kstate.IsKeyDown(Key.A))
            {
                MoveCam(rightVecConvert * deltatime * -1 * tmpMovementSpeed.X);
            }
            if (kstate.IsKeyDown(Key.D))
            {
                MoveCam(rightVecConvert * deltatime * 1 * tmpMovementSpeed.X);
            }
            if (kstate.IsKeyDown(Key.W))
            {
                MoveCam(dirVecConvert * -1 * deltatime * tmpMovementSpeed.Z );
            }
            if (kstate.IsKeyDown(Key.S))
            {
                MoveCam(dirVecConvert * 1 * deltatime * tmpMovementSpeed.Z);
            }
            if (kstate.IsKeyDown(Key.Q))
            {
                MoveCam(new Vector3(0, 1 * deltatime * tmpMovementSpeed.Y, 0));
            }
            if (kstate.IsKeyDown(Key.E))
            {
                MoveCam(new Vector3(0, -1 * deltatime * tmpMovementSpeed.Y, 0));
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
            if (kstate.IsKeyDown(Key.F1))
            {
                Console.WriteLine("Camera Positon: " + campos + "\nCamera Rotation: " + camrot);
            }
            if (kstate.IsKeyDown(Key.F4))
            {
                currentRenderMode = RenderMode.deferred;
            }
            if (kstate.IsKeyDown(Key.F5))
            {
                currentRenderMode = RenderMode.postion;
            }
            if (kstate.IsKeyDown(Key.F6))
            {
                currentRenderMode = RenderMode.color;
            }
            if(kstate.IsKeyDown(Key.F7))
            {
                currentRenderMode = RenderMode.normal;
            }
            if (kstate.IsKeyDown(Key.F8))
            {
                currentRenderMode = RenderMode.shadow;
            }
            if(kstate.IsKeyDown(Key.F9))
            {
                currentRenderMode = RenderMode.directional;
            }
            if(kstate.IsKeyDown(Key.F10))
            {
                currentRenderMode = RenderMode.pointlight;
            }
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
            DateTime dtCurr = DateTime.Now;
            TimeSpan diff = dtCurr - startTime;
            float diffsecs = (float)(diff.TotalMilliseconds) / (float)(1000);
            water.CreateMaps(diffsecs);
            waterEntity.renderable.SetHeightMap(water.GetTexture());
            //TextureDebugger.Draw(water.GetTexture());
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
            List<Entity> geometry = activeScene.getGeometry();
            List<ParticleSystem> particleSystem = activeScene.GetParticleSystems();
            Vector3 camDir = activeCam.GetDirection();
            renderer.StartLightViewPass();
            for(int i = 0; i < geometry.Count; i++)
            {
                if (geometry[i].renderable != null)
                {
                    renderer.DrawShadowLightView(activeScene.GetDirectionalLightCamera(), geometry[i].renderable);
                }
            }
            for (int j = 0; j < particleSystem.Count; j++)
            {
                renderer.DrawShadowLightViewParticle(activeScene.GetDirectionalLightCamera(), particleSystem[j].GetShadowRenderable(), particleSystem[j]);
            }
            renderer.FinishLightViewPass();
            
            renderer.StartShadowMapPass();
            for (int i = 0; i < geometry.Count; i++)
            {
                if (geometry[i].renderable != null)
                {
                    renderer.CreateShadowMap(GetCameraMatrix(), activeScene.GetDirectionalLightCamera(), geometry[i].renderable, activeScene.getDirectionalLight().direction);
                }
            }
            for (int j = 0; j < particleSystem.Count; j++)
            {
                renderer.CreateShadowMapParticle(GetCameraMatrix(), activeScene.GetDirectionalLightCamera(), particleSystem[j].GetShadowRenderable(), activeScene.getDirectionalLight().direction, particleSystem[j]);
            }
            renderer.FinishShadowMassPass();
            
            
            renderer.StartGeometryPass();
            for (int i = 0; i < geometry.Count; i++)
            {
                if (geometry[i].renderable != null)
                {
                    renderer.DrawDeferredGeometry(geometry[i].renderable, GetCameraMatrix(), campos, camDir);
                }
            }
            for(int j = 0; j < particleSystem.Count; j++)
            {
                renderer.DrawDeferredParticle(particleSystem[j].GetDeferredRenderable(), GetCameraMatrix(), campos, camDir, particleSystem[j]);
            }
            renderer.FinishGeometryPass();
            
            renderer.PointLightPass(GetCameraMatrix(), campos, camDir);
            if (currentRenderMode == RenderMode.deferred)
            {
                renderer.FinalPass(campos, activeScene.GetAmbientColor(), activeScene.getDirectionalLight(), camDir);
            }
            if (currentRenderMode == RenderMode.color)
            {
                TextureDebugger.Draw(renderer.mainFBO.Textures[1]);
            }
            if (currentRenderMode == RenderMode.postion)
            {
                TextureDebugger.Draw(renderer.mainFBO.Textures[0]);
            }
            if (currentRenderMode == RenderMode.normal)
            {
                TextureDebugger.Draw(renderer.mainFBO.Textures[2]);
            }
            if (currentRenderMode == RenderMode.pointlight)
            {
                TextureDebugger.Draw(renderer.pointLightFBO.Textures[0]);
            }
            if (currentRenderMode == RenderMode.shadow)
            {
                TextureDebugger.Draw(renderer.shadowMapFBO.Textures[0]);
            }
            if (currentRenderMode == RenderMode.directional)
            {
                TextureDebugger.Draw(renderer.lightViewFBO.Textures[0]);
            }
            //TextureDebugger.Draw(renderer.mainFBO.Textures[1]);
        }
    }
}
