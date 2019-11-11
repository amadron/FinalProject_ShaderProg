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

        Vector3 color;
        Vector3 position;
    }
    
}
