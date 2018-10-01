using Example.src.model.entitys;
using Example.src.model.graphics.rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.terrain
{
    interface ITerrainSpawner
    {
        List<Vector3> GetPositions();
        List<Vector3> GetScales();
        List<Vector4> GetRotations();
        void SpawnElements(Terrain terrain);
    }
}
