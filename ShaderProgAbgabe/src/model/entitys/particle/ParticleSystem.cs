using Example.src.model.graphics.rendering;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenseless.Geometry;
using OpenTK.Graphics.OpenGL4;
using Zenseless.OpenGL;
using Zenseless.HLGL;
using Example.src.util;
using Example.src.model.entitys.particle;

namespace Example.src.model.entitys
{
    class ParticleSystem : Entity
    {
        public ParticleSystem(DeferredRenderer renderer, IContentLoader contentLoader)
        {
            random = new Random();
            //Rendering
            this.renderer = renderer;
            deferredRenderable = new Renderable();
            var plane = Meshes.CreatePlane(1, 1, 1, 1).Transform(Transformation.Rotation(-90,Axis.X));
            IShaderProgram shader = renderer.GetShader(DeferredRenderer.DrawableType.particleMesh);
            VAO planeVao = renderer.GetDrawable(plane, DeferredRenderer.DrawableType.particleMesh);
            deferredRenderable.SetDeferredGeometryMesh(planeVao, shader);
            ITexture2D defaultAlpha = contentLoader.Load<ITexture2D>("particleDefault.png");
            deferredRenderable.SetAlbedoTexture(defaultAlpha);
            deferredRenderable.SetAlphaMap(defaultAlpha);
            deferredRenderable.faceCullingMode = FaceCullingMode.NONE;
            VAO shadowPlaneVao = renderer.GetDrawable(plane ,DeferredRenderer.DrawableType.particleShadowLightView);
            IShaderProgram shadowShader = renderer.GetShader(DeferredRenderer.DrawableType.particleShadowLightView);
            shadowRenderable = new Renderable();
            shadowRenderable.SetDeferredGeometryMesh(shadowPlaneVao, shadowShader);
            shadowRenderable.SetAlbedoTexture(defaultAlpha);
            shadowRenderable.SetAlphaMap(defaultAlpha);
            shadowRenderable.faceCullingMode = FaceCullingMode.NONE;
            //ParticleSystemSetup
            globalModules = new List<ParticleModule>();
            perParticleModules = new List<ParticleModule>();
            particleColor = new Range3D(new Vector3(1, 1, 1));
            keepScaleRatio = true;
            scaleAspect = new AspectRatio3D(AspectRatio3D.Axis.XAxis, 1);
            particlePoolList = new List<Particle>();
            spawnedParticleList = new List<Particle>();
            particleRemoveList = new List<Particle>();
            SetMaxParticles(100);
            spawnArea = new Range3D(new Vector3(-0.1f, 0, -0.1f), new Vector3(0.1f, 0, 0.1f));
            spawnScale = new Range3D(new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));
            spawnAcceleration = new Range3D(new Vector3(0, 0.1f, 0), new Vector3(0, 5f, 0));
            spawnIntervallRange = new Range(0f, 1f);
            spawnIntervall = spawnIntervallRange.GetRandomValue(random);
            spawnRate = new Range(1, 1);
            lifeTimeRange = new Range(10.0f);
        }

        DeferredRenderer renderer;
        Random random;
        Renderable shadowRenderable;
        Renderable deferredRenderable;

        //Parameters
        public bool billboardRendering;
        public Range spawnRate;
        float spawnIntervall;
        public Range spawnIntervallRange;
        private int maxParticles;
        public Range3D spawnArea;
        public Range3D spawnScale;
        public bool keepScaleRatio;
        public AspectRatio3D scaleAspect;
        public Range3D spawnAcceleration;
        public Range3D particleColor;
        public Range lifeTimeRange;
        public float lastSpawn;

        List<ParticleModule> globalModules;
        List<ParticleModule> perParticleModules;

        ParticleParameters currentParameters;

        public void AddParticleGlobalModule(ParticleModule module)
        {
            globalModules.Add(module);
        }

        public void RemoveParticleGlobalModule(ParticleModule module)
        {
            globalModules.Remove(module);
        }

        public void AddPerParticleModule(ParticleModule module)
        {
            perParticleModules.Add(module);
        }

        public void RemovePerParticleModule(ParticleModule module)
        {
            perParticleModules.Remove(module);
        }

        public struct ParticleParameters
        {
            public static ParticleParameters GetWithAmount(int amount)
            {
                ParticleParameters parameter = new ParticleParameters();
                parameter.position = new Vector3[amount];
                parameter.scale = new Vector3[amount];
                parameter.rotation = new Vector4[amount];
                parameter.color = new Vector3[amount];
                return parameter;
            }
            public Vector3[] color;
            public Vector3[] position;
            public Vector3[] scale;
            public Vector4[] rotation;
        }

        List<Particle> particlePoolList;
        List<Particle> spawnedParticleList;
        //Complete Particle List
        Particle[] allParticleArray;
        //List which contains all Particles to remove after update
        List<Particle> particleRemoveList;


        private void InitParticle(ref Particle p)
        {
            p.lifeTime = GetRandomRangeValue(lifeTimeRange);
            p.acceleration = spawnAcceleration.GetRandomValue(random);
            p.startAcceleration = p.acceleration;
            p.scale = spawnScale.GetRandomValue(random);
            if(keepScaleRatio)
            {
                p.scale = scaleAspect.GetAspectRatio(p.scale);
            }
            p.position = spawnArea.GetRandomValue(random);
            p.position += transform.position;
            p.startPosition = p.position;
            
            p.color = particleColor.GetRandomValue(random);
            p.ClearModules();
            for(int i = 0; i < perParticleModules.Count; i++)
            {
                ParticleModule module = perParticleModules[i].Clone();
                module.InstanceInit(ref p);
                p.AddModule(module);
            }
        }

        public void SetMaxParticles(int amount)
        {
            ClearParticleLists();
            allParticleArray = new Particle[amount];
            for(int i = 0; i < allParticleArray.Length; i++)
            {
                Particle tmpPart = new Particle();
                allParticleArray[i] = tmpPart;
                particlePoolList.Add(tmpPart);
            }
        }

        private void ClearParticleLists()
        {
            particlePoolList.Clear();
            spawnedParticleList.Clear();
        }

        public void SetSpawnArea(Range3D area)
        {
            spawnArea = area;
        }

        public void Update(float deltatime)
        {
            lastSpawn += deltatime;
            if(lastSpawn >= spawnIntervall)
            {
                spawnIntervall = spawnIntervallRange.GetRandomValue(random);
                int toSpawn = (int) spawnRate.GetRandomValue(random);
                for (int i = 0; i < toSpawn; i++)
                {
                    AddParticle();
                }
            }
            int spawnedListLength = spawnedParticleList.Count;
            particleRemoveList.Clear();
            for(int i = 0; i < spawnedListLength; i++)
            {
                Particle tmpP1 = spawnedParticleList[i];
                UpdateParticle(deltatime, ref tmpP1, ref particleRemoveList);
                spawnedParticleList[i] = tmpP1;
            }
            int toRemoveAmount = particleRemoveList.Count;
            for(int j = 0; j < toRemoveAmount; j++)
            {
                Particle tmpP2 = particleRemoveList[j];
                RemoveParticle(tmpP2);
            }
            UpdateParticleData();
        }

        public void AddParticle()
        {
            int poolCount = particlePoolList.Count;
            if(poolCount > 0)
            {
                lastSpawn = 0;
                Particle p = particlePoolList[poolCount - 1];
                particlePoolList.RemoveAt(poolCount - 1);
                InitParticle(ref p);
                spawnedParticleList.Add(p);
            }
        }

        private void RemoveParticle(Particle particle)
        {
            int spawnedCount = spawnedParticleList.Count;
            if(spawnedCount > 0)
            {
                spawnedParticleList.Remove(particle);
                particlePoolList.Add(particle);
            }
        }

        private void UpdateParticle(float deltatime, ref Particle particle, ref List<Particle> removeList)
        {
            particle.lifeTime -= deltatime;
            if(particle.lifeTime < 0)
            {
                removeList.Add(particle);
            }
            else
            {
                Vector3 vel = GetVelocity(particle, deltatime);
                particle.position += vel;
                particle.movementAmount += vel;
                particle.Update(deltatime);
                for(int i = 0; i < globalModules.Count; i++)
                {
                    globalModules[i].Update(deltatime, ref particle);
                }
            }
        }

        private Vector3 GetVelocity(Particle particle, float deltatime)
        {
            Vector3 a = particle.acceleration;
            return a * deltatime;
        }

        public ParticleParameters GetParticleParameters()
        {
            int count = spawnedParticleList.Count;
            ParticleParameters parameters = ParticleParameters.GetWithAmount(count);
            for(int i = 0; i < count; i++)
            {
                Particle tmp = spawnedParticleList[i];
                parameters.position[i] = tmp.position;
                parameters.rotation[i] = tmp.rotation;
                parameters.scale[i] = tmp.scale;
                parameters.color[i] = tmp.color;
            }
            return parameters;
        }

        private void UpdateParticleData()
        {
            if (spawnedParticleList.Count > 0)
            {
                currentParameters = GetParticleParameters();
                RegisterParticleData();
            }
        }

        public void RegisterParticleData()
        {
            if(spawnedParticleList.Count > 0)
            {
                
                deferredRenderable.GetMesh().SetAttribute(deferredRenderable.GetShader().GetResourceLocation(ShaderResourceType.Attribute, "instancePosition"), currentParameters.position, true);
                shadowRenderable.GetMesh().SetAttribute(shadowRenderable.GetShader().GetResourceLocation(ShaderResourceType.Attribute, "instancePosition"), currentParameters.position, true);
                deferredRenderable.GetMesh().SetAttribute(deferredRenderable.GetShader().GetResourceLocation(ShaderResourceType.Attribute, "instanceScale"), currentParameters.scale, true);
                shadowRenderable.GetMesh().SetAttribute(shadowRenderable.GetShader().GetResourceLocation(ShaderResourceType.Attribute, "instanceScale"), currentParameters.scale, true);
                deferredRenderable.GetMesh().SetAttribute(deferredRenderable.GetShader().GetResourceLocation(ShaderResourceType.Attribute, "instanceRotation"), currentParameters.rotation, true);
                shadowRenderable.GetMesh().SetAttribute(shadowRenderable.GetShader().GetResourceLocation(ShaderResourceType.Attribute, "instanceRotation"), currentParameters.rotation, true);
                deferredRenderable.GetMesh().SetAttribute(deferredRenderable.GetShader().GetResourceLocation(ShaderResourceType.Attribute, "instanceColor"), currentParameters.color, true);
                //shadowRenderable.GetMesh().SetAttribute(shadowRenderable.GetShader().GetResourceLocation(ShaderResourceType.Attribute, "instanceColor"), currentParameters.rotation, true);

            }
        }
        

        public int NumberOfSpawnedParticles()
        {
            return spawnedParticleList.Count;
        }

        public Renderable GetDeferredRenderable()
        {
            return deferredRenderable;
        }

        public Renderable GetShadowRenderable()
        {
            return shadowRenderable;
        }

        private float GetRandomRangeValue(Range range)
        {
            if (range.min != range.max)
            {
                float diff = range.max - range.min;
                float rand = range.min + (float)(random.NextDouble() * diff);
                return rand;
            }
            else
            {
                return range.min;
            }
        }
    }
}
