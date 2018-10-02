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

namespace Example.src.model.entitys.WaterSim
{
    class Water
    {
        IRenderSurface mapFBO;
        IShaderProgram waterMapShader;
        List<WaveLayer> waveLayers;
        BufferObject waveBuffer = new BufferObject(BufferTarget.ShaderStorageBuffer);
        Renderable renderable;
        int numberOfWaves;
        public Water(IContentLoader contentLoader)
        {
            waterMapShader = contentLoader.Load<IShaderProgram>("WaterMap.*");
            mapFBO = new FBO(Texture2dGL.Create(256, 256, 4, true));
            mapFBO.Attach(Texture2dGL.Create(256, 256, 4, true));
            mapFBO.Textures[0].WrapFunction = TextureWrapFunction.MirroredRepeat;
            waveLayers = GetWaveLayer();
            List<Wave> waveList = GetWaves();
            numberOfWaves = waveList.Count;
            waveBuffer.Set(waveList.ToArray(), BufferUsageHint.StaticCopy);
        }

        List<WaveLayer> GetWaveLayer()
        {
            Random random = new Random();
            Vector2 dir = new Vector2(1, 0);
            List<WaveLayer> wLayer = new List<WaveLayer>();
            WaveLayer main1 = new WaveLayer();
            main1.direction = dir;
            main1.numberOfWaves = 12;
            main1.amplitude = new Range(1, 1f);
            main1.waveToWaveDistance = new Range(0.2f, 0.20f);
            main1.amplitude = new Range(2, 2.5f);

            float subScale = 1.5f;
            WaveLayer sub1 = new WaveLayer();
            sub1.numberOfWaves = 6;
            sub1.amplitude = new Range(main1.amplitude.min * subScale, main1.amplitude.max * subScale);//new Range(0.0005f, 0.001f);
            sub1.waveToWaveDistance = new Range(main1.waveToWaveDistance.min * 0.9f, main1.waveToWaveDistance.max * 0.9f);
            sub1.speed = main1.speed;
            sub1.direction = dir + new Vector2(0, 0.1f) ;

            float subScale2 = 1.1f;
            WaveLayer sub2 = new WaveLayer();
            sub2.numberOfWaves = 6;
            sub2.amplitude = new Range(main1.amplitude.min * subScale2, main1.amplitude.max * subScale2);
            sub2.waveToWaveDistance = new Range(main1.waveToWaveDistance.min * 0.5f, main1.waveToWaveDistance.max * 0.5f);
            sub2.speed = new Range(main1.speed.min * 2f, main1.speed.max * 2f);
            sub2.direction = dir + new Vector2(0, 0.2f);
            wLayer.Add(main1);
            wLayer.Add(sub1);
            wLayer.Add(sub2);
            return wLayer;
        }

        List<Wave> GetWaves()
        {
            List<Wave> tmpList = new List<Wave>();
            for(int i = 0; i < waveLayers.Count; i++)
            {
                waveLayers[i].GenerateWaves();
                tmpList.AddRange(waveLayers[i].GetWaves());
            }
            return tmpList;
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

            //Activate Textures of FBO
            int textAmount = mapFBO.Textures.Count; //Number of Texture Channels of FBO
            for (int i = 0; i < textAmount; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                mapFBO.Textures[i].Activate();
            }

            DrawBuffersEnum[] buffers = new DrawBuffersEnum[textAmount];
            for (int i = 0; i < textAmount; i++)
            {
                buffers[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }

            GL.DrawBuffers(textAmount, buffers);

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            waveBuffer.Deactivate();

            for (int i = textAmount - 1; i >= 0; i--)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                mapFBO.Textures[i].Deactivate();
            }

            waterMapShader.Deactivate();

            mapFBO.Deactivate();
        }

        public ITexture2D GetTexture()
        {
            return mapFBO.Textures[0];
        }

        public ITexture2D GetNormalMap()
        {
            return mapFBO.Textures[1];
        }

        public void SetAmplitude(Range value)
        {
            this.amplitude = value;
        }

        public void SetSubAmplitude(Range value)
        {
            this.subAmplitude = value;
        }

        public void SetDirection(Vector2 dir)
        {
            this.direction = dir;
        }

        public void SetSpeed(Range value)
        {
            speed = value;
        }

        public void SetSubSpeed(Range value)
        {
            subSpeed = value;
        }

        public void SetWaveToWaveDistance(Range value)
        {
            this.waveToWaveDistance = value;
        }

        public void SetSubWaveToWaveDistance(Range value)
        {
            this.subWaveToWaveDistance = value;
        }

        public void SetSteepness(Range value)
        {
            this.steepness = value;
        }

        public void SetSubSteepness(Range value)
        {
            subSteepness = value;
        }

        public void SetSubDirOffset(Range value)
        {
            subDirOffset = value;
        }



        private float gravity = 9.8f;
        private Range amplitude;
        private Vector2 direction;
        private Range speed;
        private Range waveToWaveDistance;
        private Range steepness;

        private Range subDirOffset;
        private Range subAmplitude;
        private Range subSpeed;
        private Range subWaveToWaveDistance;
        private Range subSteepness;
    }
}
