using Example.src.model.graphics.rendering;
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
        public Transform transform;

        public Entity()
        {
            transform = new Transform();
            enabled = true;
            visible = true;
        }

        public string name;
        public bool enabled;
        public bool visible;
        public Renderable renderable;
    }
}
