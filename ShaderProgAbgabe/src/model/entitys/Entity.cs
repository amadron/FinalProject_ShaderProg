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
            transform.position = Vector3.Zero;
            transform.rotation = Vector3.Zero;
            transform.scale = Vector3.One;
        }

        public string name;
        public bool enabled;
        public bool visible;
        public Renderable renderable;
    }
}
