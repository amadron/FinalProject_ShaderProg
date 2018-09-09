using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.entitys.particle.modules.particlewise
{
    class PModuleApplyWind : ParticleModule
    {
        private float minHeight;
        private Vector3 windDir;
        private Particle particle;
        private Vector3 dist;
        private Vector3 changeSteps;
        private float changeTime;
        private float timer;
        public bool active;

        public PModuleApplyWind(float height, Vector3 windDir, float changeTime)
        {
            this.windDir = windDir;
            minHeight = height;
            this.changeTime = changeTime;
        }

        public ParticleModule Clone()
        {
            return new PModuleApplyWind(minHeight, windDir, changeTime);
        }

        public void InstanceInit(ref Particle particle)
        {
            this.particle = particle;
            dist = windDir - particle.acceleration;
            changeSteps = dist / changeTime;
            active = true;
        }

        public void Update(float deltatime, ref Particle particle)
        {
            if(!active)
                return;
            float cHeight = particle.movementAmount.Y;
            if(cHeight > minHeight)
            {
                particle.acceleration += changeSteps * deltatime;
                timer += deltatime;
                if(timer >= changeTime)
                {
                    active = false;
                }
            }
        }
    }
}
