using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.entitys
{
    class Entity
    {
        public Entity()
        {
            enabled = true;
            visible = true;
            position = Vector3.Zero;
            rotation = Vector3.Zero;
            scale = Vector3.One;
        }
        bool enabled;
        bool visible;
        Vector3 position;
        Vector3 rotation;
        Vector3 scale;
    }
}
