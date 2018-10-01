using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.entitys.particle
{
    class Particle
    {
        public Particle()
        {
            modules = new List<ParticleModule>();
        }

        public Vector3 color;
        public bool visible;
        public Vector3 position;
        public Vector3 startPosition;
        public Vector3 movementAmount;
        public Vector3 scale;
        public Vector4 rotation;
        public Vector3 acceleration;
        public Vector3 startAcceleration;
        public float lifeTime;

        private List<ParticleModule> modules;

        public void AddModule(ParticleModule module)
        {
            modules.Add(module);
        }

        public void RemoveModule(ParticleModule module)
        {
            modules.Remove(module);
        }

        public void ClearModules()
        {
            modules.Clear();
        }

        public void Update(float deltatime)
        {
            for(int i = 0; i < modules.Count; i++)
            {
                Particle tpart = this;
                modules[i].Update(deltatime, ref tpart);
            }
        }
    }
}
