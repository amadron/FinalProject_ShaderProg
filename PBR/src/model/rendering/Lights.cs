using System.Numerics;

namespace PBR.src.model.rendering
{
    class Light
    {
        public Light()
        {
            color = new Vector3(1f);
            position = new Vector3(0, 1, 0);
        }

        public Vector3 color;
        public Vector3 position;
    }

    class DirectionalLight : Light
    {
        public DirectionalLight() : base()
        {
            direction = new Vector3(0, 1, 1);
        }
        public Vector3 direction;
    }
}
