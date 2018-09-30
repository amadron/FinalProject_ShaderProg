using Example.src.model.graphics.rendering;
using Example.src.util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace Example.src.model.entitys
{
    class Terrain : Entity
    {

        public Terrain(IContentLoader contentLoader, string heightMapPath, float scale, float sizeX, float sizeY)
        {
            this.scale = scale;
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            Texture2dGL htext = contentLoader.Load<Texture2dGL>(heightMapPath);
            heightMap = htext.SaveToBitmap();
            CalculatePixelSizes();

        }
        Bitmap heightMap;
        public float pixelWorldWidth;
        public float pixelWorldHeight;
        public float sizeX;
        public float sizeY;
        public float scale;

        public void CalculatePixelSizes()
        {
            int resX = heightMap.Width;
            int resY = heightMap.Height;
            pixelWorldWidth = resX / sizeX;
            pixelWorldHeight = resY / sizeY;
        }

        public Vector2 GetInTerrainBounds(Vector3 position)
        {
            float minX = transform.position.X - sizeX/2;
            float maxX = transform.position.X + sizeX/2;
            float minZ = transform.position.Z - sizeY/2;
            float maxZ = transform.position.Z + sizeY/2;
            float posX = position.X - minX;
            float posZ = maxZ - position.Z;

            posX = MathUtil.ClampF(posX, 0, sizeX);
            posZ = MathUtil.ClampF(posZ, 0, sizeY);
            return new Vector2(posX, posZ);
        }

        public float GetHeightInTerrain(Vector2 pos)
        {
            int x = (int)(pos.X * pixelWorldWidth);
            int y = (int)(pos.Y * pixelWorldHeight);
            float textVal = heightMap.GetPixel(x, y).R;
            textVal /= 255;
            textVal -= 0.5f;
            return textVal * scale;
        }

    }
}
