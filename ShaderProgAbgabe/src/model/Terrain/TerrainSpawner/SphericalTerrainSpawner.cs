using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Example.src.model.graphics.rendering;
using Example.src.model.entitys;
using System.Numerics;
using Example.src.util;

namespace Example.src.model.terrain.TerrainSpawner
{
    class SphericalTerrainSpawner : Entity , ITerrainSpawner
    {
        Terrain terrain;
        float radius;
        int amountOfElements;
        Random random;
        Range circleRange;

        public SphericalTerrainSpawner(Terrain terrain, Renderable renderable)
        {
            this.terrain = terrain;
            this.renderable = renderable;
            this.radius = 0.05f;
            amountOfElements = 10;
            circleRange = new Range(-radius, radius);
            this.transform.position = Vector3.Zero;
            amountOfElements = 5;
        }

        public SphericalTerrainSpawner(Terrain terrain, Renderable renderable, Vector3 position, float radius, int amountOfElements)
        {
            this.radius = radius;
            this.transform.position = position;
            this.terrain = terrain;
            this.renderable = renderable;
            this.amountOfElements = amountOfElements;
            random = new Random();
            circleRange = new Range(-radius, radius);
        }

        public void SpawnElements()
        {
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> rotation = new List<Vector3>();
            List<Vector3> scale = new List<Vector3>();
            for(int i = 0; i < amountOfElements; i++)
            {
                Vector3 tmpPos = transform.position;
                tmpPos.X += circleRange.GetRandomValue(random);
                tmpPos.Z += circleRange.GetRandomValue(random);
                Vector2 posInTerrain = terrain.GetInTerrainBounds(tmpPos);
                tmpPos.Y = terrain.GetHeightInTerrain(posInTerrain) + terrain.transform.position.Y + 0.4f;
                positions.Add(tmpPos);
                rotation.Add(Vector3.Zero);
                scale.Add(Vector3.One);
            }
            renderable.instances = amountOfElements;
            renderable.SetInstancePositions(positions.ToArray());
            renderable.GetMesh().SetAttribute(renderable.GetShader().GetResourceLocation(Zenseless.HLGL.ShaderResourceType.Attribute, "instanceRotation"), rotation.ToArray(), true);
            renderable.GetMesh().SetAttribute(renderable.GetShader().GetResourceLocation(Zenseless.HLGL.ShaderResourceType.Attribute, "instanceScale"), scale.ToArray(), true);
        }
    }
}
