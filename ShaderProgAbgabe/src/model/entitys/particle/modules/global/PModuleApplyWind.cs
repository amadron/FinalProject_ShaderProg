using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.entitys.particle.modules.global
{
    class PModuleApplyWind : ParticleModule
    {
        private float minHeight;
        private Vector3 windDir;
        private Particle particle;


        public PModuleApplyWind(float height, Vector3 windDir)
        {
            this.windDir = windDir;
            minHeight = height;
        }

        public ParticleModule Clone()
        {
            return new PModuleApplyWind(minHeight, windDir);
        }

        public void InstanceInit(ref Particle particle)
        {
            this.particle = particle;   
        }

        public void Update(float deltatime, ref Particle particle)
        {
            float cHeight = particle.movementAmount.Y;
            if(cHeight > minHeight)
            {
               
            }
        }
    }
}
