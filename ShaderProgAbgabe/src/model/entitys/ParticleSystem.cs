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

namespace Example.src.model.entitys
{
    class ParticleSystem : Entity
    {
        public ParticleSystem(DeferredRenderer renderer)
        {
            random = new Random();
            Renderable defaultRenderer = new Renderable();
            var plane = Meshes.CreatePlane(1, 1, 1, 1);
            IShaderProgram shader = renderer.GetShader(DeferredRenderer.DrawableType.defaultMesh);
            VAO planeVao = renderer.GetDrawable(plane, DeferredRenderer.DrawableType.defaultMesh);
            defaultRenderer.SetMesh(planeVao, shader);
            defaultRenderer.faceCullingMode = FaceCullingMode.NONE;
            renderable = defaultRenderer;

            particlePoolList = new List<Particle>();
            spawnedParticleList = new List<Particle>();
            particleRemoveList = new List<Particle>();
            SetMaxParticles(100);
            spawnIntervall = 1f;
            lifeTimeRange.min = 10.0f;
            lifeTimeRange.max = 10.0f;
            initAcceleration = new Vector3(0, 1f, 0);
        }

        Random random;
        Renderable renderable;

        //Parameters
        int spawnRate;
        float spawnIntervall;
        int maxParticles;
        Vector3 spawnArea;
        Vector3 minimumBounds;
        Vector3 spawnScale;
        Range lifeTimeRange;
        float lastSpawn;
        Vector3 initAcceleration;

        struct Range
        {
            public float min;
            public float max;
        }

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
            p.position = GetPositionInRange();
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

        private Vector3 GetPositionInRange()
        {
            double x = random.NextDouble() * spawnArea.X;
            double y = random.NextDouble() * spawnArea.Y;
            double z = random.NextDouble() * spawnArea.Z;
            x -= minimumBounds.X;
            y -= minimumBounds.Y;
            z -= minimumBounds.Z;
            return new Vector3((float)x,(float) y, (float)z);
        }

        public void SetSpawnArea(Vector3 area)
        {
            spawnArea = area;
            minimumBounds = area / 2;
        }

        public void Update(float deltatime)
        {
            lastSpawn += deltatime;
            if(lastSpawn >= spawnIntervall)
            {
                AddParticle();
            }
            int spawnedListLength = spawnedParticleList.Count;
            particleRemoveList.Clear();
            for(int i = 0; i < spawnedListLength; i++)
            {
                UpdateParticle(deltatime, spawnedParticleList[i], ref particleRemoveList);
            }
            int toRemoveAmount = particleRemoveList.Count;
            for(int j = 0; j < toRemoveAmount; j++)
            {
                Particle p = particleRemoveList[j];
                RemoveParticle(p);
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

        private void UpdateParticle(float deltatime, Particle particle, ref List<Particle> removeList)
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
                renderable.GetMesh().SetAttribute(renderable.GetShader().GetResourceLocation(ShaderResourceType.Attribute, "instancePosition"), particleParams.position, true);
                renderable.GetMesh().SetAttribute(renderable.GetShader().GetResourceLocation(ShaderResourceType.Attribute, "instanceScale"), particleParams.scale, true);
                renderable.GetMesh().SetAttribute(renderable.GetShader().GetResourceLocation(ShaderResourceType.Attribute, "instanceRotation"), particleParams.rotation, true);
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
