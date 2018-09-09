using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.entitys.particle.modules.global
{
    class PModuleAddScale : ParticleModule
    {
        float scaleValue;

        public PModuleAddScale(float value)
        {
            this.scaleValue = value;
        }

        public ParticleModule Clone()
        {
            throw new NotImplementedException();
        }

        public void InstanceInit(ref Particle particle)
        {
            throw new NotImplementedException();
        }

        public void Update(float deltatime, ref Particle particle)
        {
            float addVal = scaleValue * deltatime;
            Vector3 cScale = particle.scale;
            cScale.X += addVal;
            cScale.Y += addVal;
            cScale.Z += addVal;
            particle.scale = cScale;
        }
    }
}
