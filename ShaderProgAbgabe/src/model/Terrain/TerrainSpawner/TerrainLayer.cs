using Example.src.model.entitys;
using Example.src.model.graphics.rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.terrain.TerrainSpawner
{
    class TerrainLayer : Entity
    {
        public TerrainLayer(Terrain terrain, Renderable renderable)
        {
            this.terrain = terrain;
            this.renderable = renderable;
            spawnerList = new List<ITerrainSpawner>();
        }
        Terrain terrain;
        List<ITerrainSpawner> spawnerList;
        public void AddSpawner(ITerrainSpawner spawner)
        {
            spawnerList.Add(spawner);
        }

        public void SpawnElements()
        {
            List<Vector3> positions = new List<Vector3>();
            List<Vector4> rotations = new List<Vector4>();
            List<Vector3> scales = new List<Vector3>();
            for(int i = 0; i < spawnerList.Count; i++)
            {
                spawnerList[i].SpawnElements(terrain);
                positions.AddRange(spawnerList[i].GetPositions());
                rotations.AddRange(spawnerList[i].GetRotations());
                scales.AddRange(spawnerList[i].GetScales());
            }
            renderable.SetInstancePositions(positions.ToArray());
            renderable.SetInstanceRotations(rotations.ToArray());
            renderable.SetInstanceScales(scales.ToArray());
            renderable.instances = positions.Count();
        }
    }
}
