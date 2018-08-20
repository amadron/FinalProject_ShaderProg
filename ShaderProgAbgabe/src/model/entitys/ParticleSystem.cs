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
using Example.src.controller.util;

namespace Example.src.model.entitys
{
    class ParticleSystem : Entity
    {
        public ParticleSystem(DeferredRenderer renderer, IContentLoader contentLoader)
        {
            random = new Random();
            Renderable defaultRenderer = new Renderable();
            var plane = Meshes.CreatePlane(1, 1, 1, 1).Transform(Transformation.Rotation(-90,Axis.X));
            IShaderProgram shader = renderer.GetShader(DeferredRenderer.DrawableType.particle);
            VAO planeVao = renderer.GetDrawable(plane, DeferredRenderer.DrawableType.particle);
            defaultRenderer.SetMesh(planeVao, shader);
            ITexture2D defaultAlpha = contentLoader.Load<ITexture2D>("particleDefault.png");
            defaultRenderer.SetAlphaMap(defaultAlpha);
            defaultRenderer.faceCullingMode = FaceCullingMode.NONE;
            renderable = defaultRenderer;
            keepScaleRatio = true;
            scaleAspect = new AspectRatio3D(AspectRatio3D.Axis.XAxis, 1);
            particlePoolList = new List<Particle>();
            spawnedParticleList = new List<Particle>();
            particleRemoveList = new List<Particle>();
            SetMaxParticles(100);
            spawnArea = new Range3D(new Vector3(-0.1f, 0, -0.1f), new Vector3(0.1f, 0, 0.1f));
            spawnScale = new Range3D(new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0.25f, 0.25f, 0.25f));
            spawnAcceleration = new Range3D(new Vector3(0, 0.1f, 0), new Vector3(0, 5f, 0));
            spawnIntervallRange = new Range(0.1f, 0.5f);
            spawnIntervall = spawnIntervallRange.GetRandomValue(random);
            spawnRate = new Range(1, 3);
            lifeTimeRange.min = 10.0f;
            lifeTimeRange.max = 10.0f;
            initAcceleration = spawnAcceleration.GetRandomValue(random);
        }

        Random random;
        Renderable renderable;

        //Parameters
        Range spawnRate;
        float spawnIntervall;
        Range spawnIntervallRange;
        int maxParticles;
        Range3D spawnArea;
        Range3D spawnScale;
        bool keepScaleRatio;
        AspectRatio3D scaleAspect;
        Range3D spawnAcceleration;
        Range lifeTimeRange;
        float lastSpawn;
        Vector3 initAcceleration;

        struct Particle
        {
            public bool visible;
            public Vector3 position;
            public Vector3 scale;
            public Vector3 rotation;
            public Vector3 acceleration;
            public float lifeTime;
        }

        public struct ParticleParameters
        {
            public static ParticleParameters GetWithAmount(int amount)
            {
                ParticleParameters parameter = new ParticleParameters();
                parameter.position = new Vector3[amount];
                parameter.scale = new Vector3[amount];
                parameter.rotation = new Vector3[amount];
                return parameter;
            }
            public Vector3[] position;
            public Vector3[] scale;
            public Vector3[] rotation;
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
            p.acceleration = initAcceleration;
            p.scale = spawnScale.GetRandomValue(random);
            if(keepScaleRatio)
            {
                p.scale = scaleAspect.GetAspectRatio(p.scale);
            }
            p.position = spawnArea.GetRandomValue(random);
            p.position += transform.position;
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
            }
            return parameters;
        }

        private void UpdateParticleData()
        {
            if (spawnedParticleList.Count > 0)
            {
                ParticleParameters particleParams = GetParticleParameters();
                IShaderProgram shader = renderable.GetShader();
                renderable.GetMesh().SetAttribute(shader.GetResourceLocation(ShaderResourceType.Attribute, "instancePosition"), particleParams.position, true);
                renderable.GetMesh().SetAttribute(shader.GetResourceLocation(ShaderResourceType.Attribute, "instanceScale"), particleParams.scale, true);
                renderable.GetMesh().SetAttribute(shader.GetResourceLocation(ShaderResourceType.Attribute, "instanceRotation"), particleParams.rotation, true);
            }
        }

        public int GetSpawnedParticles()
        {
            return spawnedParticleList.Count;
        }

        public Renderable GetRenderable()
        {
            return renderable;
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
