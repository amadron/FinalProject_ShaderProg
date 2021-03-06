﻿using PBR.src.controller;
using PBR.src.model.rendering;
using OpenTK.Input;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using System.Numerics;
using System.Collections.Generic;
using PBR.src.model;
using System;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PBR
{
    class View
    {
        PBRRenderer renderer;
        List<GameObject> currScene = new List<GameObject>();
        List<List<GameObject>> sceneList = new List<List<GameObject>>();
        Camera cam;
        IContentLoader contentLoader;
        public View(IRenderState renderState, IContentLoader contentLoader)
        {
            this.contentLoader = contentLoader;
            renderer = new PBRRenderer(renderState, contentLoader);
            cam = new Camera();
            currScene = new List<GameObject>();
            GameObject weapon = GetPBRModelWithMapPostfix("ceberus", "Cerberus", ".jpg");
            currScene.Add(weapon);
            sceneList.Add(currScene);
            sceneList.Add(GetSphereSampleScene());
            List<GameObject> lighterList = new List<GameObject>();
            GameObject lighter = GetPBRModelWithMapPostfix("lighterModel", "Lighter", ".png");
            lighter.transform.scale = new Vector3(0.05f);
            lighter.transform.rotation = new Vector3(-90, 0, 0);
            lighterList.Add(lighter);
            sceneList.Add(lighterList);
            renderer.AddIBLMap("Content/Textures/hdr/Alexs_Apt_2k.hdr");
            renderer.AddIBLMap("Content/Textures/hdr/Arches_E_PineTree_3k.hdr");
            renderer.AddIBLMap("Content/Textures/hdr/BasketballCourt_3k.hdr");
            renderer.SetIBLMap(0);
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
        }

        internal void Resize(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
            float aspectRatio = (float)width / (float)height;
            if(cam != null)
            {
                cam.aspectRatio = aspectRatio;
            }
        }

        struct TangentSpaceData
        {
            public Vector3[] tangent;
            public Vector3[] biTangent;
        }

        //Calculation taken from: https://learnopengl.com/Advanced-Lighting/Normal-Mapping
        TangentSpaceData GetTangentSpaceData(List<Vector3> positions, List<Vector2> uvs)
        {
            if(positions.Count != uvs.Count)
            {
                throw new Exception("Position Data does not match the amount of uv data");
            }
            Vector3[] tangents = new Vector3[positions.Count];
            Vector3[] biTangents = new Vector3[positions.Count];

            for(int i = 0; i < positions.Count - 2; i+=2)
            {
                int idx1 = i;
                int idx2 = i + 1;
                int idx3 = i + 2;

                Vector3 pos1 = positions[idx1];
                Vector3 pos2 = positions[idx2];
                Vector3 pos3 = positions[idx3];

                Vector3 edge1 = pos2 - pos1;
                Vector3 edge2 = pos3 - pos1;

                Vector2 uv1 = uvs[idx1];
                Vector2 uv2 = uvs[idx2];
                Vector2 uv3 = uvs[idx3];

                Vector2 deltaUV1 = uv2 - uv1;
                Vector2 deltaUV2 = uv3 - uv1;

                float f = 1.0f / (deltaUV1.Y * deltaUV2.X - deltaUV1.X * deltaUV2.Y);
                
                Vector3 tangent;
                tangent.X = f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X);
                tangent.Y = f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y);
                tangent.Z = f * (deltaUV2.Y * edge1.Z + deltaUV1.Y * edge2.Z);
                tangent = Vector3.Normalize(tangent);

                Vector3 biTangent;
                biTangent.X = f * (-deltaUV2.X * edge1.X + deltaUV1.X * edge2.X);
                biTangent.Y = f * (-deltaUV2.X * edge1.Y + deltaUV1.X * edge2.Y);
                biTangent.Z = f * (-deltaUV2.X * edge1.Z + deltaUV1.X * edge2.Z);
                biTangent = Vector3.Normalize(biTangent);

                tangents[idx1] = tangent;
                biTangents[idx1] = biTangent;
                tangents[idx2] = tangent;
                biTangents[idx2] = biTangent;
                tangents[idx3] = tangent;
                biTangents[idx3] = biTangent;
            }
            TangentSpaceData result = new TangentSpaceData();
            result.tangent = tangents;
            result.biTangent = biTangents;
            
            return result;
        }

        GameObject GetPBRModelWithMapPostfix(string meshFileName, string textureMainName, string textureFormat)
        {
            string albedoMap = textureMainName + "_A" + textureFormat;
            string normalMap = textureMainName + "_N" + textureFormat;
            string metallicMap = textureMainName + "_M" + textureFormat;
            string roughnessMap = textureMainName + "_R" + textureFormat;
            return GetPBRModel(meshFileName, albedoMap, normalMap, metallicMap, roughnessMap);
        }

        GameObject GetPBRModel(string meshFileName, string albedoMapFile, string normalMapFile, string metallicMapFile, string roughnessMapFile)
        {
            DefaultMesh mesh = contentLoader.Load<DefaultMesh>(meshFileName);
            VAO geom = VAOLoader.FromMesh(mesh, renderer.GetPBRShader());
            GameObject go = new GameObject();
            PBRMaterial mat = new PBRMaterial();
            go.mesh = geom;
            go.material = mat;
            //mat.metal = 1.0f;
            //mat.metal = 0f;
            mat.roughness = 0;
            if(albedoMapFile != null)
            {
                ITexture2D albedoMap = contentLoader.Load<ITexture2D>(albedoMapFile);
                mat.albedoMap = albedoMap;
            }
            if(normalMapFile != null && normalMapFile != "")
            {
                ITexture2D normalMap = contentLoader.Load<ITexture2D>(normalMapFile);
                mat.normalMap = normalMap;
            }
            if(metallicMapFile != null)
            {
                ITexture2D metallicMap = contentLoader.Load<ITexture2D>(metallicMapFile);
                mat.metallicMap = metallicMap;
            }
            if(roughnessMapFile != null)
            {
                ITexture2D roughnessMap = contentLoader.Load<ITexture2D>(roughnessMapFile);
                mat.roughnessMap = roughnessMap;
            }
            return go;
        }

        List<GameObject> GetSphereSampleScene()
        {
            List<GameObject> goList = new List<GameObject>();
            DefaultMesh plane = Meshes.CreatePlane(10, 10, 10, 10).Transform(Transformation.Translation(new Vector3(0,-1f,0)));
            VAO planeMesh = VAOLoader.FromMesh(plane, renderer.GetPBRShader());
            GameObject planeGO = new GameObject();
            PBRMaterial planemat = new PBRMaterial();
            planemat.albedoColor = new Vector3(1);
            planemat.roughness = 1;
            planemat.metal = 0;
            planeGO.mesh = planeMesh;
            planeGO.material = planemat;
            //goList.Add(planeGO);
            //return goList;
            int gridSize = 7;
            float sphereSize = 0.1f;
            float spacing = sphereSize + sphereSize * 2f;
            float startX = gridSize / 2 * spacing;
            Vector3 startVector = new Vector3(startX, startX, 0);
            float paramSteps = 1.0f / gridSize;
            string texPrefix = "rustediron2_";
            ITexture2D albedoText = contentLoader.Load<ITexture2D>(texPrefix + "basecolor.png");
            ITexture2D metallicText = contentLoader.Load<ITexture2D>(texPrefix + "metallic.png");
            ITexture2D normalText = contentLoader.Load<ITexture2D>(texPrefix + "normal.png");
            ITexture2D roughnessText = contentLoader.Load<ITexture2D>(texPrefix + "roughness.png");
            
            DefaultMesh mesh = contentLoader.Load<DefaultMesh>("uvSphere").Transform(Transformation.Scale(0.1f));
            //DefaultMesh mesh = Meshes.CreateSphere(sphereSize, 2);
            VAO geom = VAOLoader.FromMesh(mesh, renderer.GetPBRShader());
            
            
            TangentSpaceData tangentData = GetTangentSpaceData(mesh.Position, mesh.TexCoord);
            int tangentLocation = renderer.GetPBRShader().GetResourceLocation(ShaderResourceType.Attribute, "tangent");
            int biTangentLocation = GL.GetAttribLocation(renderer.GetPBRShader().ProgramID, "biTangent");
            geom.SetAttribute(tangentLocation, tangentData.tangent);
            geom.SetAttribute(biTangentLocation, tangentData.biTangent);
            

            for (int i = 0; i < gridSize; i++)
            {
                Vector3 tmpStart = startVector;
                for (int j = 0; j < gridSize; j++)
                {
                    
                    
                    GameObject go = new GameObject();
                    go.transform.position = tmpStart;
                    go.material.metal = i * paramSteps;
                    go.material.roughness = j  * paramSteps;
                    go.material.albedoColor = new Vector3(1, 0, 0);
                    go.mesh = geom;
                    
                    /*
                    go.material.albedoMap = albedoText;
                    go.material.metallicMap = metallicText;
                    go.material.normalMap = normalText;
                    go.material.roughnessMap = roughnessText;
                    */

                    goList.Add(go);
                    tmpStart.X -= spacing;
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

        public void Event_KeyPress(object sender, OpenTK.KeyPressEventArgs e)
        {
            
        }

        public void GameWindow_MouseDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            mouseButtonDown = true;
        }

        public void GameWindow_MouseUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            mouseButtonDown = false;
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
            if (CheckKey(Key.A))
            {
                move -= right;
            }
            if (CheckKey(Key.D))
            {
                move += right;
            }
            if (CheckKey(Key.W))
            {
                move -= forward;
            }
            if (CheckKey(Key.S))
            {
                move += forward;
            }
            if(CheckKey(Key.Q))
            {
                move += vertical;
            }
            if(CheckKey(Key.E))
            {
                move -= vertical;
            }
            Vector3 camPos = cam.transform.position;//fCam.View.Position;
            camPos.X += move.X;
            camPos.Y += move.Y;
            camPos.Z += move.Z;
            cam.transform.position = camPos;
            for(int i = 0; i < 9; i++)
            {
                Key currKey = Key.Number1 + i;
                if(i > 8)
                {
                    currKey = Key.Number0;
                }
                if(CheckKey(currKey) && sceneList.Count >= i+1)
                {
                    currScene = sceneList[i];
                }
            }
            for(int i = 0; i < 12; i++)
            {
                Key currKey = Key.F1 + i;
                if(CheckKey(currKey))
                {
                    renderer.SetIBLMap(i);
                }
            }
            //fCam.View.Position = camPos;
        }

        bool CheckKey(Key key)
        {
            if(!keyStates.ContainsKey(key))
            {
                keyStates.Add(key, false);
                return false;
            }
            else
            {
                return keyStates[key];
            }
        }

        public void Render()
        {
            renderer.StartRendering();
            

            foreach(GameObject go in currScene)
            {
                //renderer.Render(fCam.Matrix, fCam.View.Position, go.mesh, go.material);
                renderer.Render(cam.Matrix, cam.transform.position, go);
            }

            renderer.RenderSkybox(cam.GetTransformationMatrix(), cam.GetProjectionMatrix());
            /*
            renderer.StartRendering();
            renderer.ShowTexture2D(0);
            */
        }
    }
}
