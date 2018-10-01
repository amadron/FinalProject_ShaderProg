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
        float radius;
        int amountOfElements;
        Random random;
        Range circleRange;
        bool randomRotationY = true;
        public Range rotationYRange = new Range(0, 360);
        public Range3D randomScaleRange;

        List<Vector3> positions = new List<Vector3>();
        List<Vector4> rotations = new List<Vector4>();
        List<Vector3> scales = new List<Vector3>();

        public SphericalTerrainSpawner()
        {
            this.radius = 0.05f;
            amountOfElements = 10;
            circleRange = new Range(-radius, radius);
            this.transform.position = Vector3.Zero;
            amountOfElements = 5;
        }

        public SphericalTerrainSpawner(Vector3 position, float radius, int amountOfElements)
        {
            this.radius = radius;
            this.transform.position = position;
            this.amountOfElements = amountOfElements;
            random = new Random();
            circleRange = new Range(-radius, radius);
            randomScaleRange = new Range3D(Vector3.One);
        }

        public void SpawnElements(Terrain terrain)
        {
            for(int i = 0; i < amountOfElements; i++)
            {
                Vector3 tmpPos = transform.position;
                tmpPos.X += circleRange.GetRandomValue(random);
                tmpPos.Z += circleRange.GetRandomValue(random);
                Vector2 posInTerrain = terrain.GetInTerrainBounds(tmpPos);
                Vector3 tmpScale = randomScaleRange.GetRandomValue(random);
                scales.Add(tmpScale);
                tmpPos.Y = terrain.GetHeightInTerrain(posInTerrain) + terrain.transform.position.Y + (0.4f * tmpScale.Y);
                positions.Add(tmpPos);
                Vector3 tmpRot = new Vector3(0, 0, 0);
                if(randomRotationY)
                {
                    tmpRot.X = rotationYRange.GetRandomValue(random);
                }
                Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(tmpRot.X, tmpRot.Y, tmpRot.Z);
                rotations.Add(new Vector4(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W));
            }
        }

        public List<Vector3> GetPositions()
        {
            return positions;
        }

        public List<Vector3> GetScales()
        {
            return scales;
        }

        public List<Vector4> GetRotations()
        {
            return rotations;
        }
    }
}
