using Example.src.model.entitys;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Box2 = OpenTK.Box2;

namespace Example.src.model.terrain
{
    class TerrainLayer
    {
        entitys.Terrain terrain;
        Entity model;
        Bitmap layerMap;
        List<Vector3> positions;
        List<Vector3> rotations;
        List<Vector3> scales;
        List<List<Box2>> layerRanges;
        Dictionary<Box2, Vector3> placedPositions;
        float pixelWorldWidth;
        float pixelWorldHeight;

        public TerrainLayer(entitys.Terrain terrain)
        {
            layerRanges = new List<List<Box2>>();
            this.terrain = terrain;
        }

        void CreateTerrainLayer()
        {
            bool inRange = false;
            OpenTK.Box2 tmpRange;
            for (int i = 0; i < layerMap.Height; i++)
            {
                List<Box2> tmpList = new List<Box2>();
                inRange = false;
                float upY = 0;
                if (i > 0)
                {
                    upY = pixelWorldHeight * (i - 1);
                }
                float downY = upY + pixelWorldHeight;
                for(int j = 0; j <  layerMap.Width; j++)
                {
                    Color c = layerMap.GetPixel(i, j);
                    if(c.R > 0 && !inRange)
                    {
                        tmpRange = new Box2();
                        tmpRange.Top = upY;
                        tmpRange.Bottom = downY;
                        tmpRange.Left = j * pixelWorldWidth;
                        tmpList.Add(tmpRange);
                    }
                    if(c.R <= 0 && inRange || j == layerMap.Width - 1)
                    {
                        tmpRange.Right = (j + 1) * pixelWorldWidth;
                        inRange = false;
                    }
                    
                }
                if(tmpList.Count > 0)
                {
                    layerRanges.Add(tmpList);
                }
            }
        }

        void PlaceObjects()
        {
            for(int i = 0; i < layerRanges.Count; i++)
            {
                List<Box2> current = layerRanges[i];
            }
        }

        void CalculatePixelSizes()
        {
            int resX = layerMap.Width;
            int resY = layerMap.Height;
            pixelWorldWidth = terrain.sizeX / resX;
            pixelWorldHeight = terrain.sizeY / resY;
        }
    }
}
