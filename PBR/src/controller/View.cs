using PBR.src.controller;
using PBR.src.model.rendering;
using OpenTK.Input;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using System.Numerics;

namespace PBR
{
    class View
    {
        PBRRenderer renderer;
        VAO geometry;
        PBRMaterial mat;
        Camera<Orbit, Perspective> cam;
        public View(IRenderState renderState, IContentLoader contentLoader)
        {
            renderer = new PBRRenderer(renderState, contentLoader);
            DefaultMesh sphere = Meshes.CreateSphere(0.2f, 10);
            sphere = Meshes.CreateCubeWithNormals();
            geometry = VAOLoader.FromMesh(sphere, renderer.GetShader());
            mat = new PBRMaterial();
            mat.albedoColor = new Vector3(1);
            mat.roughness = 0.0f;
            mat.metal = 0.5f;
            cam = new Camera<Orbit, Perspective>(new Orbit(), new Perspective(nearClip: 0.1f,farClip: 1000));
            MouseState mState = Mouse.GetState();
            lastMousePos = new Vector2(mState.X, mState.Y);
        }

        float lastScroll = 0;
        Vector2 lastMousePos;
        bool mouseButtonDown = false;

        public void Update(float deltatime)
        {
            MouseState mState = Mouse.GetState();
            MouseScroll scroll = mState.Scroll;
            float scrollDelta = lastScroll - scroll.Y;
            if(scrollDelta > 0)
            {
                cam.View.Distance += deltatime;
            }
            if(scrollDelta < 0)
            {
                cam.View.Distance -= deltatime;
            }
            if(cam.View.Distance < 0)
            {
                cam.View.Distance = 0;
            }
            //cam.View.Distance += deltatime * 0.1f;
            Vector2 mDelta = new Vector2(lastMousePos.X - mState.X, lastMousePos.Y - mState.Y);
            if(!mouseButtonDown && mState.LeftButton == ButtonState.Pressed)
            {
                mouseButtonDown = true;
            }
            if(mouseButtonDown && mState.LeftButton == ButtonState.Released)
            {
                mouseButtonDown = false;
            }
            if(mouseButtonDown)
            {
                float vert = mDelta.Y * deltatime;
                float hor = mDelta.X * deltatime;
                cam.View.Elevation += vert;
                cam.View.Azimuth += hor;
            }

            lastMousePos = new Vector2(mState.X, mState.Y);
            lastScroll = scroll.Y;
        }

        public void Render()
        {
            renderer.Render(cam, geometry, mat);
        }
    }
}
