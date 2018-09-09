using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.entitys.particle
{
    interface ParticleModule
    {

        void Update(float deltatime, ref Particle particle);

        void InstanceInit(ref Particle particle);
        //In Case, that per Particle Modules are used
        ParticleModule Clone();
    }
}
