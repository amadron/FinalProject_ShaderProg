using Example.src.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.entitys.WaterSim
{
    class WaveLayer
    {
        public WaveLayer()
        {
            waveList = new List<Wave>();
            random = new Random();
            numberOfWaves = 12;
            steepness = new Range(1);
            waveToWaveDistance = new Range(0.05f, 0.05f);
            speed = new Range(1);
            amplitude = new Range(2f);
            direction = new Vector2(1, 0);
        }



        public void GenerateWaves()
        {
            waveList.Clear();
            for (int i = 0; i < numberOfWaves; i++)
            {
                float waveToWaveValue = waveToWaveDistance.GetRandomValue(random);
                Wave wave = new Wave();
                wave.wavelength = (float)Math.Sqrt(gravity * ((2 * Math.PI) / waveToWaveValue));
                wave.steepness = steepness.GetRandomValue(random);
                wave.speed = speed.GetRandomValue(random);
                wave.direction = direction;
                wave.amplitude = amplitude.GetRandomValue(random);
                waveList.Add(wave);
            }
        }

        public int numberOfWaves = 10;

        List<Wave> waveList;
        Random random;
        public float gravity = 9.8f;
        public Range amplitude;
        public Vector2 direction;
        public Range speed;
        public Range waveToWaveDistance;
        public Range steepness;

        public List<Wave> GetWaves()
        {
            return waveList;
        }

    }
}
