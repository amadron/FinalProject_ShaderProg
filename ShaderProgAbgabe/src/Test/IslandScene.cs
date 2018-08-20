using Example.src.model;
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
            directionalLightCamera = new FirstPersonCamera(new Vector3(0, 1, 5f), 25, 180, Camera.ProjectionType.Orthographic, fov: 1f, width: 20, height: 20);
            ParticleSystem system = new ParticleSystem(renderer);
            system.transform.position = new Vector3(0, 0.3f, 0);
            //AddParticleSystem(system);
        }

        List<PointLight> GetPointLights()
        {
            List<PointLight> lightList = new List<PointLight>();
            PointLight l = new PointLight(new Vector3(-0.5f, 2f, -0.5f), new Vector4(Color.Green.ToVector3(), 1), 3f, 0.8f, new Vector4(1), 80, 0.1f);
            PointLight l2 = new PointLight(new Vector3(0.7f, 4f, 1f), new Vector4(Color.Red.ToVector3(), 1), 3f, 0.8f);
            lightList.Add(l);
            lightList.Add(l2);
            return lightList;
        }

        private List<Renderable> GetGeometry(DeferredRenderer renderer)
        {
            IShaderProgram defaultShader = renderer.GetShader(DeferredRenderer.DrawableType.defaultMesh);
            List<Renderable> res = new List<Renderable>();
            Renderable isle = new Renderable();
            var islePlane = Meshes.CreatePlane(10, 10, 60, 60);
            VAO isleDrawable = renderer.GetDrawable(islePlane, DeferredRenderer.DrawableType.defaultMesh);
            isle.SetMesh(isleDrawable, defaultShader);
            ITexture2D isleAlbedo = contentLoader.Load<ITexture2D>("testTexture.png");
            ITexture2D isleNormal = contentLoader.Load<ITexture2D>("normalTest1.jpg");
            isle.SetAlbedoTexture(isleAlbedo);
            isle.SetNormalMap(isleNormal);
            ITexture2D isleHeightmap = contentLoader.Load<ITexture2D>("heightmap.png");
            isle.SetHeightMap(isleHeightmap);
            isle.heightScaleFactor = 55f;

            res.Add(isle);

            Renderable water = new Renderable();
            var waterplane = Meshes.CreatePlane(20, 20, 70, 70).Transform(Transformation.Translation(0, 0.5f, 0));
            VAO waterDrawable = renderer.GetDrawable(waterplane, DeferredRenderer.DrawableType.defaultMesh);
            water.SetMesh(waterDrawable, defaultShader);
            water.SetAlbedoTexture(isleAlbedo);

            Renderable grass = new Renderable();
            grass.faceCullingMode = FaceCullingMode.NONE;
            var grassPlane = Meshes.CreatePlane(1, 1, 2, 2).Transform(Transformation.Rotation(-90, Axis.X)).Transform(Transformation.Translation(0, 1.8f, -2f));
            VAO grassMesh = renderer.GetDrawable(grassPlane, DeferredRenderer.DrawableType.defaultMesh);
            grass.SetMesh(grassMesh, renderer.GetShader(DeferredRenderer.DrawableType.defaultMesh));
            ITexture2D grassAlbedo = contentLoader.Load<ITexture2D>("Grass_512_albedo.tif");
            ITexture2D grassAlpha = contentLoader.Load<ITexture2D>("tGrass_512_alpha.tif");
            grass.SetAlbedoTexture(grassAlbedo);
            grass.SetAlphaMap(grassAlpha);

            //res.Add(grass);
            //res.Add(water);
            return res;
        }
    }
}
