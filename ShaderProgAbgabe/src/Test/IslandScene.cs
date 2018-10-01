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
using Example.src.util;
using Example.src.model.entitys.particle.modules.global;
using Example.src.model.entitys.particle.modules.particlewise;
using Example.src.model.terrain.TerrainSpawner;
using Example.src.controller.content;

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
            entityList = GetGeometry(renderer);
            directionalLight = new DirectionalLight(new Vector4(1f, 0.968f, 0.878f, 1), new Vector3(0.1f, -0.5f, 1f), 1f, new Vector4(1, 1, 1, 1), 255, 0f);
            directionalLightCamera = new FirstPersonCamera(new Vector3(0, 5f, 5f), 25, 180, Camera.ProjectionType.Orthographic, fov: 1f, width: 70, height: 70, zPlaneNear:0.1f, zPlaneFar:500);
            
            ITexture2D particleText = contentLoader.Load<ITexture2D>("smoke.jpg");
            ParticleSystem system = new ParticleSystem(renderer, contentLoader);
            //system.SetMaxParticles(1);
            system.GetDeferredRenderable().SetAlbedoTexture(particleText);
            system.GetDeferredRenderable().SetAlphaMap(particleText);
            system.GetShadowRenderable().SetAlbedoTexture(particleText);
            system.GetShadowRenderable().SetAlphaMap(particleText);
            system.transform.position = new Vector3(-7.288709f, 12f, 5.872631f);
            system.spawnIntervallRange = new Range(0.5f,0.5f);
            system.lifeTimeRange = new Range(8, 10);
            system.spawnArea = new Range3D(Vector3.Zero); //new Range3D(new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, 0.5f));
            system.spawnAcceleration = new Range3D(new Vector3(0, 1.5f, 0), new Vector3(0, 2f, 0));
            system.spawnScale = new Range3D(new Vector3(3f, 3f, 3f), new Vector3(5f, 5f, 5));
            PModuleAddScale scaleModule = new PModuleAddScale(1f);
            system.AddParticleGlobalModule(scaleModule);
            PModuleApplyWind windModule = new PModuleApplyWind(2f, new Vector3(0, 0.3f, 1f), 2);
            system.AddPerParticleModule(windModule);
            AddParticleSystem(system);
        }

        List<PointLight> GetPointLights()
        {
            List<PointLight> lightList = new List<PointLight>();
            PointLight l = new PointLight(new Vector3(-0.5f, 10f, -0.5f), new Vector4(Color.Green.ToVector3(), 1), 2f, 1f, new Vector4(1), 80, 0.1f);
            PointLight l2 = new PointLight(new Vector3(-7.288709f, 12f, 6f), new Vector4(Color.Yellow.ToVector3(), 1), 3f,5f);
            PointLight l3 = new PointLight(new Vector3(-7.288709f, 12f, 6f), new Vector4(Color.Red.ToVector3(), 1), 3f, 5f);
            //lightList.Add(l);
            lightList.Add(l2);
            lightList.Add(l3);
            return lightList;
        }

        private List<Entity> GetGeometry(DeferredRenderer renderer)
        {
            IShaderProgram defaultShader = renderer.GetShader(DeferredRenderer.DrawableType.deferredDefaultMesh);
            List<Entity> res = new List<Entity>();
            float islandScale = 30f;
            var islePlane = Meshes.CreatePlane(30, 30, 120, 120).Transform(Transformation.Translation(0, islandScale / 2, 0));
            Renderable isle = ContentFactory.GetDefaultRenderable(renderer, islePlane);


            ITexture2D isleAlbedo = contentLoader.Load<ITexture2D>("terrain.png");
            isleAlbedo.Filter = TextureFilterMode.Linear;
            //ITexture2D isleNormal = contentLoader.Load<ITexture2D>("normalTest1.jpg");
            isle.SetAlbedoTexture(isleAlbedo);
            //isle.SetNormalMap(isleNormal);
            ITexture2D isleHeightmap = contentLoader.Load<ITexture2D>("hmapUnity.png");
            isle.SetHeightMap(isleHeightmap);
            isle.heightScaleFactor = islandScale;
            Entity isleEntity = new Entity();
            isleEntity.name = "isle";
            isleEntity.renderable = isle;

            Terrain isleTerrain = new Terrain(contentLoader, "hmapUnity.png", islandScale, 30, 30);
            isleTerrain.transform.position = new Vector3(0, islandScale / 2, 0);



            var waterplane = Meshes.CreatePlane(50, 50, 225, 225).Transform(Transformation.Translation(0, 1f, 0));
            VAO waterDrawable = renderer.GetDrawable(waterplane, DeferredRenderer.DrawableType.deferredDefaultMesh);
            Renderable water = ContentFactory.GetDefaultRenderable(renderer, waterplane);
            ITexture2D waterEnvironment = contentLoader.Load<ITexture2D>("sky1.jpg");
            water.SetEnvironmentMap(waterEnvironment);
            water.reflectivity = 1;
            //water.SetAlbedoTexture(isleAlbedo);
            Entity waterEntity = new Entity();
            waterEntity.name = "water";
            waterEntity.renderable = water;

            var grassPlane = Meshes.CreatePlane(1, 1, 2, 2).Transform(Transformation.Rotation(-90, Axis.X));
            Renderable grass = ContentFactory.GetDefaultRenderable(renderer, grassPlane);
            grass.faceCullingMode = FaceCullingMode.NONE;
            ITexture2D grassAlbedo = contentLoader.Load<ITexture2D>("Grass_512_albedo.tif");
            ITexture2D grassAlpha = contentLoader.Load<ITexture2D>("tGrass_512_alpha.tif");
            grass.SetAlbedoTexture(grassAlbedo);
            grass.SetAlphaMap(grassAlpha);
            //Entity grassEntity = new Entity();
            //grassEntity.renderable = grass;
            Vector3[] spawnPositions = { new Vector3(-0.1f, 7.1f, 2.5f), new Vector3(-3.5f, 7.1f, -1.5f),
            new Vector3(6f, 7.1f, 2.5f), new Vector3(6f, 7.1f, -1.5f), new Vector3(5f, 7.1f, -6f), new Vector3(1f, 7.1f, -8f), new Vector3(-3f, 7.1f, -8f)};
            float[] radius = { 2f, 1.9f, 2f, 2f, 2f, 2f, 2f };
            int[] amountGrass = { 50, 60, 50, 60, 60, 60, 60 };
            Range3D scaleRange = new Range3D(new Vector3(0.5f), new Vector3(1.5f));
            TerrainLayer layer = new TerrainLayer(isleTerrain, grass);
            for(int i = 0; i < spawnPositions.Length; i++)
            {
                SphericalTerrainSpawner grassSpawner = new SphericalTerrainSpawner( spawnPositions[i], radius[i], amountGrass[i]);
                grassSpawner.randomScaleRange = scaleRange;
                layer.AddSpawner(grassSpawner);
            }
            layer.SpawnElements();
            res.Add(layer);
            var skysphere = Meshes.CreateSphere(40, 2);
            skysphere.SwitchTriangleMeshWinding();
            Renderable skydome = ContentFactory.GetDefaultRenderable(renderer, skysphere);
            skydome.faceCullingMode = FaceCullingMode.FRONT_SIDE;
            skydome.unlit = 1;
            skydome.SetAlbedoTexture(waterEnvironment);
            Entity skyEntity = new Entity();
            skyEntity.renderable = skydome;
            skyEntity.name = "skydome";

            var msphere = Meshes.CreateSphere(1, 2).Transform(Transformation.Translation(0,2,0));
            Renderable sphere = ContentFactory.GetDefaultRenderable(renderer, msphere);
            VAO spVao = renderer.GetDrawable(msphere, DeferredRenderer.DrawableType.deferredDefaultMesh);
            sphere.SetEnvironmentMap(waterEnvironment);
            Entity spEntity = new Entity();
            spEntity.name = "sphere";
            spEntity.renderable = sphere;

            //res.Add(grassEntity);
            res.Add(isleEntity);
            res.Add(waterEntity);
            //res.Add(spEntity);
            res.Add(skyEntity);
            return res;
        }
    }
}
