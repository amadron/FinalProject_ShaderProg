using PBR.src.model.rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenseless.OpenGL;

namespace PBR.src.model
{
    class GameObject
    {
        public GameObject()
        {
            transform = new Transform();
            name = "";
            material = new PBRMaterial();
        }
        public Transform transform;
        public string name;
        public VAO mesh;
        public PBRMaterial material;
    }
}
