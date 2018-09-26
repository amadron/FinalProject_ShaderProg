using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Zenseless.HLGL;

namespace Example.src.model.entitys
{
    class Terrain : Entity
    {
        public Terrain(Bitmap texture)
        {
            this.texture = texture;
        }

        Bitmap texture;
        public float pixelWorldWidth;
        public float pixelWorldHeight;
        public float sizeX;
        public float sizeY;
        public float scale;

        void CalculatePixelSizes()
        {
            int resX = texture.Width;
            int resY = texture.Height;
            pixelWorldWidth = sizeX / resX;
            pixelWorldHeight = sizeY / resY;
        }

        float GetHeightInTerrain(Vector2 pos)
        {
            int x = (int)(pos.X / pixelWorldWidth);
            int y = (int)(pos.Y / pixelWorldHeight);
            float textVal = texture.GetPixel(x, y).R;
            return textVal * scale;
        }

    }
}
