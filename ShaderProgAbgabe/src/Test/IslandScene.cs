﻿using Example.src.model;
using Example.src.model.entitys;
using Example.src.model.graphics.camera;
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
    class IslandScene : Scene
    {
        IContentLoader contentLoader;
        public IslandScene(IContentLoader contentLoader, DeferredRenderer renderer)
        {
            this.contentLoader = contentLoader;
            pointLightList = GetPointLights();
            ambientColor = new Vector4(0.1f, 0.10f, 0.074f, 1);
            geometryList = GetGeometry(renderer);
            directionalLight = new DirectionalLight(new Vector4(1f, 0.968f, 0.878f, 1), new Vector3(0.1f, -0.5f, 1f), 1f, new Vector4(1, 1, 1, 1), 255, 0f);
            directionalLightCamera = new FirstPersonCamera(new Vector3(0, 1, 5f), 25, 180, Camera.ProjectionType.Orthographic, fov: 1f, width: 20, height: 20); ;
        }

        List<PointLight> GetPointLights()
        {
            List<PointLight> lightList = new List<PointLight>();
            PointLight l = new PointLight(new Vector3(-0.5f, 0.4f, -0.5f), new Vector4(Color.Green.ToVector3(), 1), 1f, 4f, new Vector4(1), 255, 0f);
            PointLight l2 = new PointLight(new Vector3(0.8f, 0.4f, 0.5f), new Vector4(Color.Red.ToVector3(), 1), 1f, 3f);
            lightList.Add(l);
            lightList.Add(l2);
            return lightList;
        }

        private List<Renderable> GetGeometry(DeferredRenderer renderer)
        {
            List<Renderable> res = new List<Renderable>();
            Renderable isle = new Renderable();
            var islePlane = Meshes.CreatePlane(10, 10, 60, 60);
            IDrawable isleDrawable = renderer.GetDrawable(islePlane, DeferredRenderer.DrawableType.defaultMesh);
            isle.mesh = isleDrawable;
            ITexture2D isleAlbedo = contentLoader.Load<ITexture2D>("testTexture.png");
            isle.SetAlbedoTexture(isleAlbedo);
            ITexture2D isleHeightmap = contentLoader.Load<ITexture2D>("heightmap.png");
            isle.SetHeightMap(isleHeightmap);
            isle.heightScaleFactor = 55f;

            res.Add(isle);

            Renderable water = new Renderable();
            var waterplane = Meshes.CreatePlane(20, 20, 70, 70).Transform(Transformation.Translation(0, 0.5f, 0));
            IDrawable waterDrawable = renderer.GetDrawable(waterplane, DeferredRenderer.DrawableType.water);
            water.mesh = waterDrawable;
            water.SetAlbedoTexture(isleAlbedo);

            res.Add(water);
            return res;
        }
    }
}
