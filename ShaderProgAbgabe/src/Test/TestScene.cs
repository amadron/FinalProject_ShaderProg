﻿using Example.src.model;
using Example.src.model.entitys;
using Example.src.model.graphics.rendering;
using Example.src.model.lightning;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace Example.src.Test
{
    class TestScene : Scene
    {
        IContentLoader contentLoader;
        public TestScene(IContentLoader contentLoader, DeferredRenderer renderer)
        {
            this.contentLoader = contentLoader;
            pointLightList = GetPointLights();
            ambientColor = new Vector4(0.1f, 0.10f, 0.074f, 1);
            geometryList = GetGeometry(renderer);
            directionalLight = new DirectionalLight(new Vector4(1f, 0.968f, 0.878f, 1), new Vector3(0.1f, -0.5f, 1f), 0.5f, new Vector4(1, 1, 1, 1), 255, 0f);
            directionalLightCamera = new Camera<Orbit, Perspective>(new Orbit(4.3f, 180, 45), new Perspective(farClip: 50));
        }

        List<PointLight> GetPointLights()
        {
            List<PointLight> lightList = new List<PointLight>();
            PointLight l = new PointLight(new Vector3(0, 0.4f, 0), new Vector4(Color.Green.ToVector3(), 1), 1f, 3f, new Vector4(1), 255, 0f);
            PointLight l2 = new PointLight(new Vector3(0.8f, 0.4f, 0.5f), new Vector4(Color.Red.ToVector3(), 1), 1f, 3f);
            lightList.Add(l);
            lightList.Add(l2);
            return lightList;
        }

        private List<Renderable> GetGeometry(DeferredRenderer renderer)
        {
            List<Renderable> res = new List<Renderable>();
            var mesh = Meshes.CreatePlane(5, 5, 10, 10);
            var sphere = Meshes.CreateSphere(1, 2);
            var sphere2 = Meshes.CreateSphere(1, 2);
            mesh.Add(sphere.Transform(Transformation.Translation(new Vector3(1f, 0.5f, -1f))));
            mesh.Add(sphere2.Transform(Transformation.Translation(new Vector3(-1f, 0.5f, 1f))));
            var cube = Meshes.CreateCubeWithNormals(1);
            mesh.Add(cube.Transform(Transformation.Translation(new Vector3(-0.5f, 0.5f, -1.5f))));


            IDrawable planeDraw = renderer.GetDrawable(mesh, DeferredRenderer.DrawableType.defaultMesh);
            Renderable planeRend = new Renderable();
            planeRend.mesh = planeDraw;
            ITexture2D text = contentLoader.Load<ITexture2D>("testTexture.png");
            ITexture2D normal = contentLoader.Load<ITexture2D>("normalTest.jpg");
            ITexture2D height = contentLoader.Load<ITexture2D>("heightmap.png");
            planeRend.SetAlbedoTexture(text);
            planeRend.SetNormalMap(normal);
            
            
            Renderable sphereRend = new Renderable();
            var nsphere = Meshes.CreateSphere(1, 2);
            IDrawable sphereDraw = renderer.GetDrawable(nsphere, DeferredRenderer.DrawableType.defaultMesh);
            sphereRend.mesh = sphereDraw;
            
            var mesh2 = Meshes.CreatePlane(5, 5, 30, 30);

            Renderable plane2Rend = new Renderable();
            IDrawable plane2Draw = renderer.GetDrawable(mesh2, DeferredRenderer.DrawableType.defaultMesh);
            plane2Rend.mesh = plane2Draw;
            plane2Rend.SetAlbedoTexture(text);
            plane2Rend.SetNormalMap(normal);
            plane2Rend.SetHeightMap(height);
            plane2Rend.heightScaleFactor = 20;

            var suzanne = contentLoader.Load<DefaultMesh>("suzanne.obj").Transform(Transformation.Translation(0,1,0));
            suzanne.Transform(Transformation.Scale(0.05f));
            Renderable suzRend = new Renderable();
            IDrawable suzanneDraw = renderer.GetDrawable(suzanne, DeferredRenderer.DrawableType.defaultMesh);
            suzRend.mesh = suzanneDraw;
            suzRend.SetNormalMap(normal);
            //res.Add(sphereRend);
            //res.Add(planeRend);
            //res.Add(sphereRend);
            res.Add(plane2Rend);
            //res.Add(suzRend);
            return res;
        }
    }
}
