using PBR.src.controller;
using PBR.src.model.rendering;
using OpenTK.Input;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using System.Numerics;
using System.Collections.Generic;
using PBR.src.model;

namespace PBR
{
    class View
    {
        PBRRenderer renderer;
        List<GameObject> objects = new List<GameObject>();
        Camera cam;
        Camera<FirstPerson, Perspective> fCam;
        IContentLoader contendLoader;
        public View(IRenderState renderState, IContentLoader contentLoader)
        {
            this.contendLoader = contendLoader;
            renderer = new PBRRenderer(renderState, contentLoader);
            cam = new Camera();
            objects = GetSampleScene();
            cam.position = new Vector3(0, 0, 1);
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
                /*
                cam.View.Elevation += vert;
                cam.View.Azimuth += hor;
                */
                //fCam.View.Tilt += vert;
                //fCam.View.Heading += hor;
                cam.rotation.X += vert * 0.5f;
                cam.rotation.Y += hor * 0.5f;
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
            Vector3 camPos = cam.position;//fCam.View.Position;
            camPos.X += move.X;
            camPos.Y += move.Y;
            camPos.Z += move.Z;
            cam.position = camPos;
            //fCam.View.Position = camPos;
        }

        

        public void Render()
        {
            renderer.StartRendering();
            foreach(GameObject go in objects)
            {
                //renderer.Render(fCam.Matrix, fCam.View.Position, go.mesh, go.material);
                renderer.Render(cam.Matrix, cam.position, go);
            }
        }
    }
}
