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
            directionalLightCamera = new FirstPersonCamera(new Vector3(0, 2, 5f), 25, 180, Camera.ProjectionType.Orthographic, fov: 1f, width: 50, height: 50, zPlaneNear:0.1f, zPlaneFar:500);
            
            ITexture2D particleText = contentLoader.Load<ITexture2D>("smoke.jpg");
            ParticleSystem system = new ParticleSystem(renderer, contentLoader);
            //system.SetMaxParticles(1);
            system.GetDeferredRenderable().SetAlbedoTexture(particleText);
            system.GetDeferredRenderable().SetAlphaMap(particleText);
            system.GetShadowRenderable().SetAlbedoTexture(particleText);
            system.GetShadowRenderable().SetAlphaMap(particleText);
            system.transform.position = new Vector3(2f, 10f, 3f);
            system.spawnIntervallRange = new Range(0.1f,0.5f);
            system.lifeTimeRange = new Range(8, 10);
            system.spawnArea = new Range3D(Vector3.Zero); //new Range3D(new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, 0.5f));
            system.spawnAcceleration = new Range3D(new Vector3(0, 1.5f, 0), new Vector3(0, 2f, 0));
            system.spawnScale = new Range3D(new Vector3(2f, 2f, 1f), new Vector3(5f, 5f, 1));
            PModuleAddScale scaleModule = new PModuleAddScale(0.7f);
            system.AddParticleGlobalModule(scaleModule);
            PModuleApplyWind windModule = new PModuleApplyWind(2f, new Vector3(0, 0.3f, 1f), 2);
            system.AddPerParticleModule(windModule);
            AddParticleSystem(system);
        }

        List<PointLight> GetPointLights()
        {
            List<PointLight> lightList = new List<PointLight>();
            PointLight l = new PointLight(new Vector3(-0.5f, 10f, -0.5f), new Vector4(Color.Green.ToVector3(), 1), 2f, 0.8f, new Vector4(1), 80, 0.1f);
            PointLight l2 = new PointLight(new Vector3(1.5f, 9.5f, 3.5f), new Vector4(Color.Yellow.ToVector3(), 1), 4f,1f);
            PointLight l3 = new PointLight(new Vector3(1.5f, 9.5f, 3.5f), new Vector4(Color.Red.ToVector3(), 1), 4f, 1f);
            lightList.Add(l);
            lightList.Add(l2);
            lightList.Add(l3);
            return lightList;
        }

        private List<Entity> GetGeometry(DeferredRenderer renderer)
        {
            IShaderProgram defaultShader = renderer.GetShader(DeferredRenderer.DrawableType.deferredDefaultMesh);
            List<Entity> res = new List<Entity>();
            Renderable isle = new Renderable();
            var islePlane = Meshes.CreatePlane(30, 30, 120, 120);
            VAO isleDrawable = renderer.GetDrawable(islePlane, DeferredRenderer.DrawableType.deferredDefaultMesh);
            isle.SetMesh(isleDrawable, defaultShader);
            ITexture2D isleAlbedo = contentLoader.Load<ITexture2D>("testTexture.png");
            //ITexture2D isleNormal = contentLoader.Load<ITexture2D>("normalTest1.jpg");
            isle.SetAlbedoTexture(isleAlbedo);
            //isle.SetNormalMap(isleNormal);
            ITexture2D isleHeightmap = contentLoader.Load<ITexture2D>("heightmap.png");
            isle.SetHeightMap(isleHeightmap);
            isle.heightScaleFactor = 150f;
            Entity isleEntity = new Entity();
            isleEntity.name = "isle";
            isleEntity.renderable = isle;
            res.Add(isleEntity);

            Renderable water = new Renderable();
            var waterplane = Meshes.CreatePlane(50, 50, 225, 225).Transform(Transformation.Translation(0, 1f, 0));
            VAO waterDrawable = renderer.GetDrawable(waterplane, DeferredRenderer.DrawableType.deferredDefaultMesh);
            water.SetMesh(waterDrawable, defaultShader);
            ITexture2D waterEnvironment = contentLoader.Load<ITexture2D>("sky1.jpg");
            water.SetEnvironmentMap(waterEnvironment);
            water.reflectivity = 1;
            //water.SetAlbedoTexture(isleAlbedo);
            Entity waterEntity = new Entity();
            waterEntity.name = "water";
            waterEntity.renderable = water;

            Renderable grass = new Renderable();
            grass.faceCullingMode = FaceCullingMode.NONE;
            var grassPlane = Meshes.CreatePlane(1, 1, 2, 2).Transform(Transformation.Rotation(-90, Axis.X)).Transform(Transformation.Translation(0, 1.8f, -2f));
            VAO grassMesh = renderer.GetDrawable(grassPlane, DeferredRenderer.DrawableType.deferredDefaultMesh);
            grass.SetMesh(grassMesh, renderer.GetShader(DeferredRenderer.DrawableType.deferredDefaultMesh));
            ITexture2D grassAlbedo = contentLoader.Load<ITexture2D>("Grass_512_albedo.tif");
            ITexture2D grassAlpha = contentLoader.Load<ITexture2D>("tGrass_512_alpha.tif");
            grass.SetAlbedoTexture(grassAlbedo);
            grass.SetAlphaMap(grassAlpha);

            Renderable skydome = new Renderable();
            skydome.faceCullingMode = FaceCullingMode.FRONT_SIDE;
            var skysphere = Meshes.CreateSphere(40, 2);
            skysphere.SwitchTriangleMeshWinding();
            VAO skyMesh = renderer.GetDrawable(skysphere, DeferredRenderer.DrawableType.deferredDefaultMesh);
            skydome.SetMesh(skyMesh, renderer.GetShader(DeferredRenderer.DrawableType.deferredDefaultMesh));
            skydome.unlit = 1;
            skydome.SetAlbedoTexture(waterEnvironment);
            Entity skyEntity = new Entity();
            skyEntity.renderable = skydome;
            skyEntity.name = "skydome";

            Renderable sphere = new Renderable();
            var msphere = Meshes.CreateSphere(1, 2).Transform(Transformation.Translation(0,2,0));
            VAO spVao = renderer.GetDrawable(msphere, DeferredRenderer.DrawableType.deferredDefaultMesh);
            sphere.SetMesh(spVao, renderer.GetShader(DeferredRenderer.DrawableType.deferredDefaultMesh));
            sphere.SetEnvironmentMap(waterEnvironment);
            Entity spEntity = new Entity();
            spEntity.name = "sphere";
            spEntity.renderable = sphere;

            //res.Add(grass);
            res.Add(waterEntity);
            //res.Add(spEntity);
            res.Add(skyEntity);
            return res;
        }
    }
}
