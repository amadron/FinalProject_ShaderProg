using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Example.src.controller.rendering;
using Example.src.model;
using Example.src.model.entitys;
using Example.src.model.graphics.camera;
using Example.src.model.graphics.rendering;
using Example.src.model.graphics.ui;
using Example.src.Test;
using OpenTK.Input;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace Example.src.controller
{
    class Game
    {
        DeferredRenderer renderer;
        UIRenderer uiRenderer;
        IContentLoader contentLoader;
        IRenderState renderState;

        Camera activeCam;
        Scene activeScene;

        Vector3 campos = new Vector3(0, 10, -5f);
        Vector2 camrot = new Vector2(0, 180);
        //Vector3 campos = new Vector3(0, 1, 10f);
        //Vector2 camrot = new Vector2(25, 180);
        float rotSpeedY = 40;
        float rotSpeedX = 40;
        DateTime startTime;
        Water water;
        Entity waterEntity;
        UI ui;
        Vector2 windowRes;
        Vector2 mousePos;

        public Game(IContentLoader contentLoader, IRenderState renderState)
        {
            ui = new IslandUI(contentLoader);
            this.renderState = renderState;
            this.contentLoader = contentLoader;
            renderer = new DeferredRenderer(contentLoader, renderState);
            uiRenderer = new UIRenderer(contentLoader, renderState);
            activeScene = new IslandScene(contentLoader, renderer);
            waterEntity = activeScene.GetEntityByName("water");
            if(waterEntity != null)
                waterEntity.renderable.heightScaleFactor = 0.03f;
            renderer.SetPointLights(activeScene.getPointLights());
            activeCam = new FirstPersonCamera(campos, camrot.X, camrot.Y, Camera.ProjectionType.Perspective, fov:1f, width:20, height:20, zPlaneFar: 100f);
            startTime = DateTime.Now;
            water = new Water(this.contentLoader);
        }

        public void Update(float deltatime)
        {
            UpdateControl(deltatime);
            activeScene.Update(deltatime);
        }

        bool mouseClicked = false;
        Vector2 mouseClickedPos;
        Vector2 mouseDelta;
        Vector2 mouseSpeed = new Vector2(0.5f, 0.5f);
        Vector3 movementSpeed = new Vector3(1f, 1f, 1f);
        float shiftSpeedFactor = 3f;

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
            if (mouseClicked)
            {
                Vector2 mDiff = (mousePos - mouseClickedPos);
                RotateCam(new Vector2(mDiff.Y, mDiff.X) * deltatime * mouseSpeed);
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
                Debug.WriteLine("Camera Positon: " + campos + "\nCamera Rotation: " + camrot);
            }
            if (kstate.IsKeyDown(Key.F4))
            {
                renderer.currentRenderMode = DeferredRenderer.RenderMode.deferred;
            }
            if (kstate.IsKeyDown(Key.F5))
            {
                renderer.currentRenderMode = DeferredRenderer.RenderMode.postion;
            }
            if (kstate.IsKeyDown(Key.F6))
            {
                renderer.currentRenderMode = DeferredRenderer.RenderMode.color;
            }
            if(kstate.IsKeyDown(Key.F7))
            {
                renderer.currentRenderMode = DeferredRenderer.RenderMode.normal;
            }
            if (kstate.IsKeyDown(Key.F8))
            {
                renderer.currentRenderMode = DeferredRenderer.RenderMode.shadow;
            }
            if(kstate.IsKeyDown(Key.F9))
            {
                renderer.currentRenderMode = DeferredRenderer.RenderMode.directional;
            }
            if(kstate.IsKeyDown(Key.F10))
            {
                renderer.currentRenderMode = DeferredRenderer.RenderMode.pointlight;
            }
        }

        public void GameWindow_MouseMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            mousePos = new Vector2(e.X, e.Y);
            mouseDelta.X = e.XDelta;
            mouseDelta.Y = e.YDelta;
        }

        public void GameWindow_MouseDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            mouseClicked = true;
            mouseClickedPos = new Vector2(e.X, e.Y);
            Vector2 glPos = ScreenPositionToGLViewport(e.X, e.Y);
            ui.Click(glPos.X, glPos.Y);
        }

        public void GameWindow_MouseUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            mouseClicked = false;
        }

        private void MoveCam(Vector3 move)
        {
            campos += move;
            activeCam.SetPosition(campos);
            //Vector3 nCamPos = new Vector3(campos.X, campos.Y + 1, campos.Z - 5f);
            //activeScene.GetDirectionalLightCamera().SetPosition(nCamPos);
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
            if (waterEntity != null)
            {
                waterEntity.renderable.SetHeightMap(water.GetTexture());
                //waterEntity.renderable.SetNormalMap(water.GetNormalMap());
            }
            //TextureDebugger.Draw(water.GetNormalMap());
            RenderDeferred();
            RenderUI(renderer.GetFinalPassTexture(), ui);

        }

        public Vector2 ScreenPositionToGLViewport(int x, int y)
        {
            float posX = (2 / windowRes.X) * x - 1;
            float posY = (2 / windowRes.Y) * -y + 1;
            return new Vector2(posX, posY) ;
        }
        
        public void Resize(int width, int height)
        {
            renderer.Resize(width, height);
            activeCam.Resize(width, height);
            activeScene.Resize(width, height);
            windowRes.X = width;
            windowRes.Y = height;
        }

        private void RenderDeferred()
        {
            List<Entity> geometry = activeScene.getGeometry();
            List<Entity> renderables = new List<Entity>();
            List<Entity> alphaGeometry = new List<Entity>();
            foreach(Entity e in geometry)
            {
                if(e.renderable != null)
                {
                    if(e.renderable.alphaMap == null)
                    {
                        renderables.Add(e);
                    }
                    else
                    {
                        alphaGeometry.Add(e);
                    }
                }
            }
            List<ParticleSystem> particleSystem = activeScene.GetParticleSystems();
            Vector3 camDir = activeCam.GetDirection();
            renderer.StartLightViewPass();
            for(int i = 0; i < renderables.Count; i++)
            {
                renderer.DrawShadowLightView(activeScene.GetDirectionalLightCamera(), renderables[i].renderable);
            }
            for (int i = 0; i < alphaGeometry.Count; i++)
            {
                renderer.DrawShadowLightView(activeScene.GetDirectionalLightCamera(), alphaGeometry[i].renderable);
            }
            for (int j = 0; j < particleSystem.Count; j++)
            {
                renderer.DrawShadowLightViewParticle(activeScene.GetDirectionalLightCamera(), particleSystem[j].GetShadowRenderable(), particleSystem[j]);
            }
            renderer.FinishLightViewPass();
            
            renderer.StartShadowMapPass();
            for (int i = 0; i < renderables.Count; i++)
            {
                renderer.CreateShadowMap(activeCam.GetMatrix(), activeScene.GetDirectionalLightCamera(), renderables[i].renderable, activeScene.getDirectionalLight().direction);
            }
            for (int i = 0; i < alphaGeometry.Count; i++)
            {
                renderer.CreateShadowMap(activeCam.GetMatrix(), activeScene.GetDirectionalLightCamera(), alphaGeometry[i].renderable, activeScene.getDirectionalLight().direction);
            }
            for (int j = 0; j < particleSystem.Count; j++)
            {
                renderer.CreateShadowMapParticle(activeCam, activeScene.GetDirectionalLightCamera(), particleSystem[j].GetShadowRenderable(), activeScene.getDirectionalLight().direction, particleSystem[j]);
            }
            renderer.FinishShadowMassPass();
            
            
            renderer.StartGeometryPass();
            for (int i = 0; i < renderables.Count; i++)
            {
                renderer.DrawDeferredGeometry(renderables[i].renderable, activeCam.GetMatrix(), campos, camDir);
            }
            for (int i = 0; i < alphaGeometry.Count; i++)
            {
                renderer.DrawDeferredGeometry(alphaGeometry[i].renderable, activeCam.GetMatrix(), campos, camDir);
            }
            for (int j = 0; j < particleSystem.Count; j++)
            {
                renderer.DrawDeferredParticle(particleSystem[j].GetDeferredRenderable(), activeCam, campos, camDir, particleSystem[j]);
            }

            renderer.FinishGeometryPass();
            
            renderer.PointLightPass(activeCam.GetMatrix(), campos, camDir);
            renderer.FinalPass(campos, activeScene.GetAmbientColor(), activeScene.getDirectionalLight(), camDir, true);

            
        }

        private void RenderUI(ITexture2D texture, UI ui)
        {
            uiRenderer.Render(texture, ui.GetUIElements());
        }
    }
}
