using PBR.src.controller;
using PBR.src.model.rendering;
using OpenTK.Input;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using System.Numerics;
using System.Collections.Generic;
using PBR.src.model;
using System;

namespace PBR
{
    class View
    {
        PBRRenderer renderer;
        List<GameObject> objects = new List<GameObject>();
        Camera cam;
        Camera<FirstPerson, Perspective> fCam;
        IContentLoader contentLoader;
        public View(IRenderState renderState, IContentLoader contentLoader)
        {
            this.contentLoader = contentLoader;
            renderer = new PBRRenderer(renderState, contentLoader);
            cam = new Camera();
            objects = GetSampleScene();
            cam.transform.position = new Vector3(0, 0, 1);
            cam.clippingNear = 0.01f;
            cam.clippingFar = 10000.0f;
            cam.fov = 90;
            cam.projectionMode = Camera.ProjectionMode.Perspective;
            //fCam = new Camera<FirstPerson, Perspective>(new FirstPerson(new Vector3(0, 0, 1)), new Perspective(farClip:1000.0f));
            //fCam.Projection.FieldOfViewY = 60;
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


        List<GameObject> GetSampleScene()
        {
            List<GameObject> goList = new List<GameObject>();
            DefaultMesh plane = Meshes.CreatePlane(10, 10, 10, 10).Transform(Transformation.Translation(new Vector3(0,-1f,0)));
            VAO planeMesh = VAOLoader.FromMesh(plane, renderer.GetShader());
            GameObject planeGO = new GameObject();
            PBRMaterial planemat = new PBRMaterial();
            planemat.albedoColor = new Vector3(1);
            planemat.roughness = 1;
            planemat.metal = 0;
            planeGO.mesh = planeMesh;
            planeGO.material = planemat;
            goList.Add(planeGO);
            //return goList;
            int gridSize = 7;
            float sphereSize = 0.1f;
            float spacing = sphereSize + sphereSize * 2f;
            float startX = gridSize / 2 * spacing;
            Vector3 startVector = new Vector3(-startX, startX, 0);
            float paramSteps = 1.0f / gridSize;
            string texPrefix = "rustediron2_";
            /*
            ITexture2D albedoText = contentLoader.Load<ITexture2D>(texPrefix + "basecolor.png");
            ITexture2D metallicText = contentLoader.Load<ITexture2D>(texPrefix + "metallic.png");
            ITexture2D normalText = contentLoader.Load<ITexture2D>(texPrefix + "normal.png");
            ITexture2D roughnessText = contentLoader.Load<ITexture2D>(texPrefix + "roughness.png");
            */
            for(int i = 0; i < gridSize; i++)
            {
                Vector3 tmpStart = startVector;
                for (int j = 0; j < gridSize; j++)
                {
                    DefaultMesh mesh = Meshes.CreateSphere(sphereSize, 2);
                    VAO geom = VAOLoader.FromMesh(mesh, renderer.GetShader());
                    
                    GameObject go = new GameObject();
                    go.transform.position = tmpStart;
                    go.material.metal = i * paramSteps;
                    go.material.roughness = j * paramSteps;
                    go.material.albedoColor = new Vector3(1, 0, 0);
                    go.mesh = geom;
                    goList.Add(go);
                    tmpStart.X += spacing;
                }
                startVector.Y -= spacing;
            }
            return goList;
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
            float mouseSpeed = 2;
            if (mouseButtonDown)
            {
                float vert = mDelta.Y * deltatime * mouseSpeed;
                float hor = mDelta.X * deltatime * mouseSpeed;
                
                cam.transform.rotation.X += vert * 0.5f;
                cam.transform.rotation.Y += hor * 0.5f;
                /*
                if (cam.transform.rotation.X > 90)
                {
                    cam.transform.rotation.X = 90;
                }
                if (cam.transform.rotation.X < -90)
                {
                    cam.transform.rotation.X = -90;
                }
                */
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

            Vector3 forward;// = new OpenTK.Vector4(0, 0, 1, 1);
            Vector3 right; // = new OpenTK.Vector4(1, 0, 0, 1);
            right = cam.GetRightVector();
            forward = cam.GetForwardVector();
            //forward = rot * forward;
            //right = rot * right;
            KeyboardState ks = Keyboard.GetState();
            Vector3 move = Vector3.Zero;
            float moveSpeed = 1.0f;
            right *= moveSpeed * deltatime;
            forward *= moveSpeed * deltatime;
            Vector3 vertical = new Vector3(0,1,0);
            vertical *= moveSpeed * deltatime;
            if (keyStates[Key.A])
            {
                move -= right;
            }
            if (keyStates[Key.D])
            {
                move += right;
            }
            if (keyStates[Key.W])
            {
                move -= forward;
            }
            if (keyStates[Key.S])
            {
                move += forward;
            }
            if(keyStates[Key.Q])
            {
                move += vertical;
            }
            if(keyStates[Key.E])
            {
                move -= vertical;
            }
            Vector3 camPos = cam.transform.position;//fCam.View.Position;
            camPos.X += move.X;
            camPos.Y += move.Y;
            camPos.Z += move.Z;
            cam.transform.position = camPos;
            //fCam.View.Position = camPos;
        }

        

        public void Render()
        {
            renderer.StartRendering();
            foreach(GameObject go in objects)
            {
                //renderer.Render(fCam.Matrix, fCam.View.Position, go.mesh, go.material);
                renderer.Render(cam.Matrix, cam.transform.position, go);
            }
        }
    }
}
