using PBR.src.controller;
using PBR.src.model.rendering;
using OpenTK.Input;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using System.Numerics;
using System.Collections.Generic;

namespace PBR
{
    class View
    {
        PBRRenderer renderer;
        VAO geometry;
        PBRMaterial mat;
        Camera cam;
        Camera<FirstPerson, Perspective> fCam;
        public View(IRenderState renderState, IContentLoader contentLoader)
        {
            
            renderer = new PBRRenderer(renderState, contentLoader);
            DefaultMesh sphere = Meshes.CreateSphere(0.2f, 10);
            sphere = Meshes.CreateCubeWithNormals();
            geometry = VAOLoader.FromMesh(sphere, renderer.GetShader());
            mat = new PBRMaterial();
            mat.albedoColor = new Vector3(1);
            mat.roughness = 1f;
            mat.metal = 0.5f;
            cam = new Camera();
            
            cam.position = new Vector3(0, 0, 1);
            cam.clippingNear = 0.01f;
            cam.clippingFar = 10000.0f;
            cam.fov = 90;
            fCam = new Camera<FirstPerson, Perspective>(new FirstPerson(new Vector3(0, 0, 1)), new Perspective());
            MouseState mState = Mouse.GetState();
            lastMousePos = new Vector2(mState.X, mState.Y);
            keyStates = new Dictionary<Key, bool>();
            keyStates.Add(Key.A, false);
            keyStates.Add(Key.D, false);
            keyStates.Add(Key.S, false);
            keyStates.Add(Key.W, false);
            keyStates.Add(Key.Q, false);
            keyStates.Add(Key.E, false);
        }

        Dictionary<Key, bool> keyStates;

        float lastScroll = 0;
        Vector2 lastMousePos;
        bool mouseButtonDown = false;
        public void Update(float deltatime)
        {
            MouseState mState = Mouse.GetState();
            MouseScroll scroll = mState.Scroll;
            float scrollDelta = lastScroll - scroll.Y;
            /*
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
            */
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
            if (mouseButtonDown)
            {
                float vert = mDelta.Y * deltatime;
                float hor = mDelta.X * deltatime;
                /*
                cam.View.Elevation += vert;
                cam.View.Azimuth += hor;
                */
                fCam.View.Tilt += vert;
                fCam.View.Heading += hor;
                cam.rotation.X += vert;
                cam.rotation.Y += hor;
            }
            lastMousePos = new Vector2(mState.X, mState.Y);
            lastScroll = scroll.Y;
            FirstPersonMovement(cam, deltatime);
        }

        public void Event_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if(!keyStates.ContainsKey(e.Key))
            {
                keyStates.Add(e.Key, true);
            }
            keyStates[e.Key] = true;
        }

        public void Event_KeyRelease(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            //keyStates.Add(e.Key, false);
            keyStates[e.Key] = false;

        }

        private void Event_KeyPress(object sender, OpenTK.KeyPressEventArgs e)
        {
            
        }

        void FirstPersonMovement(Camera cam, float deltatime)
        {

            OpenTK.Vector4 forward = new OpenTK.Vector4(0, 0, 1, 1);
            OpenTK.Vector4 right = new OpenTK.Vector4(1, 0, 0, 1);
            OpenTK.Matrix4 rotX = OpenTK.Matrix4.CreateRotationX(cam.rotation.X);
            OpenTK.Matrix4 rotY = OpenTK.Matrix4.CreateRotationY(cam.rotation.Y);
            OpenTK.Matrix4 rot = rotY * rotX;
            //forward = rot * forward;
            //right = rot * right;
            KeyboardState ks = Keyboard.GetState();
            OpenTK.Vector4 move = new OpenTK.Vector4(0);
            float moveSpeed = 1.0f;
            right *= moveSpeed * deltatime;
            forward *= moveSpeed * deltatime;
            OpenTK.Vector4 vertical = new OpenTK.Vector4(0, 1, 0, 1);
            vertical *= moveSpeed * deltatime;
            if (keyStates[Key.A])
            {
                move += right;
            }
            if (keyStates[Key.D])
            {
                move -= right;
            }
            if (keyStates[Key.W])
            {
                move += forward;
            }
            if (keyStates[Key.S])
            {
                move -= forward;
            }
            if(keyStates[Key.Q])
            {
                move += vertical;
            }
            if(keyStates[Key.E])
            {
                move -= vertical;
            }
            Vector3 camPos = fCam.View.Position;
            camPos.X += move.X;
            camPos.Y += move.Y;
            camPos.Z += move.Z;
            cam.position = camPos;
            fCam.View.Position = camPos;
        }

        public void Render()
        {
            renderer.Render(fCam.Matrix, fCam.View.Position, geometry, mat);
        }
    }
}
