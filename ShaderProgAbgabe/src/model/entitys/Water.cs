using Example.src.model.graphics.rendering;
using Example.src.util;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace Example.src.model.entitys
{
    class Water
    {
        IRenderSurface mapFBO;
        IShaderProgram waterMapShader;
        Random random;
        int numberOfWaves;
        Wave[] waves;
        BufferObject waveBuffer = new BufferObject(BufferTarget.ShaderStorageBuffer);
        Renderable renderable;
        public Water(IContentLoader contentLoader)
        {
            random = new Random();
            waterMapShader = contentLoader.Load<IShaderProgram>("WaterMap.*");
            mapFBO = new FBO(Texture2dGL.Create(256, 256, 4, true));
            mapFBO.Textures[0].WrapFunction = TextureWrapFunction.MirroredRepeat;
            numberOfWaves = 12;
            SetSteepness(new Range(1));
            SetWaveToWaveDistance(new Range(0.05f, 0.05f));
            SetSpeed(new Range(1));
            SetAmplitude(new Range(2f));
            SetDirection(new Vector2(1, 0));
            InitWaves();
        }

        void InitWaves()
        {
            waves = new Wave[numberOfWaves];
            for(int i = 0; i < numberOfWaves; i++)
            {
                float waveToWaveValue = waveToWaveDistance.GetRandomValue(random);
                waves[i].wavelength = (float)Math.Sqrt(gravity * ((2 * Math.PI) / waveToWaveValue));
                waves[i].steepness = steepness.GetRandomValue(random);
                waves[i].speed = speed.GetRandomValue(random);
                waves[i].direction = direction;
                waves[i].amplitude = amplitude.GetRandomValue(random);
            }
            waveBuffer.Set(waves, BufferUsageHint.StaticCopy);
        }

        public void CreateMaps(float time)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            mapFBO.Activate();

            waterMapShader.Activate();
            waterMapShader.Uniform("time", time);
            waterMapShader.Uniform("numberOfWaves", numberOfWaves);
            int buffer = waterMapShader.GetResourceLocation(ShaderResourceType.RWBuffer, "WavesBuffer");

            waveBuffer.ActivateBind(buffer);
            
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            waveBuffer.Deactivate();

            waterMapShader.Deactivate();

            mapFBO.Deactivate();
        }

        public ITexture2D GetTexture()
        {
            return mapFBO.Texture;
        }

        public void SetAmplitude(Range value)
        {
            this.amplitude = value;
        }

        public void SetDirection(Vector2 dir)
        {
            this.direction = dir;
        }

        public void SetSpeed(Range value)
        {
            speed = value;
        }

        public void SetWaveToWaveDistance(Range value)
        {
            this.waveToWaveDistance = value;
        }

        public void SetSteepness(Range value)
        {
            this.steepness = value;
        }

        private float gravity = 9.8f;
        private Range amplitude;
        private Vector2 direction;
        private Range speed;
        private Range waveToWaveDistance;
        private Range steepness;
    }
}
