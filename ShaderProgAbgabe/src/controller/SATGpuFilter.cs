using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using OpenTK.Graphics.OpenGL4;
using System.Numerics;

namespace Example.src.controller
{
    class SATGpuFilter
    {
        IContentLoader contentLoader;
        IShaderProgram shaderSumHorizontal;
        IShaderProgram shaderSumVertical;
        IShaderProgram shaderAssemblyVertical;
        IShaderProgram shaderAssemblyHorizontal;
        IShaderProgram shaderSATFilter;

        ITexture2D testTexture;

        int amountBlocksX;
        int amountBlocksY;
        int blockSizeX;
        int blockSizeY;
        int kernelSizeX;
        int kernelSizeY;

        IRenderSurface FBO1;
        IRenderSurface FBO2;

        public SATGpuFilter(IContentLoader contentLoader, int amountBlocksX, int amountBlocksY, int resX, int resY, int kernelSizeY, int kernelSizeX)
        {
            this.contentLoader = contentLoader;
            shaderSumHorizontal = contentLoader.LoadPixelShader("SATSumHorizontal");   
            shaderSumVertical = contentLoader.LoadPixelShader("SATSumVertical");

            shaderAssemblyVertical = contentLoader.LoadPixelShader("SATAssemblyPassVertical");
            shaderAssemblyHorizontal = contentLoader.LoadPixelShader("SATAssemblyPassHorizontal");

            shaderSATFilter = contentLoader.LoadPixelShader("SATFiltering");
            testTexture = contentLoader.Load<ITexture2D>("testTexture.*");
            this.kernelSizeX = kernelSizeX;
            this.kernelSizeY = kernelSizeY;
            SetUpBlocksAndTextures(amountBlocksX, amountBlocksY, testTexture.Width, testTexture.Height);
        }

        private void SetUpBlocksAndTextures(int amountBlocksX,  int amountBlocksY, int resX, int resY)
        {
            this.amountBlocksX = amountBlocksX;
            this.amountBlocksY = amountBlocksY;
            this.blockSizeX = resX / amountBlocksX;
            this.blockSizeY = resY / amountBlocksY;
            FBO1 = new FBO(Texture2dGL.Create(resX, resY, 4, true));
            FBO1.Texture.WrapFunction = TextureWrapFunction.MirroredRepeat;
            FBO2 = new FBO(Texture2dGL.Create(resX, resY, 4, true));
            FBO2.Texture.WrapFunction = TextureWrapFunction.MirroredRepeat;
        }

        public ITexture2D GetFilterTexture()
        {
            return FBO2.Texture;
            //return testTexture;
        }

        public void Test()
        {
            FilterTexture(testTexture);
        }

        public void FilterTexture(ITexture2D sourceTexture)
        {
            //Vertical sum Pass
            SumValues(sourceTexture, shaderSumVertical, FBO1);
            //Assembly Block Values Vertical
            SumValues(FBO1.Texture, shaderAssemblyVertical, FBO2);
            
            //Horizontal sum Pass
            SumValues(FBO2.Texture, shaderSumHorizontal, FBO1);
            //Assembly Block Values Horizontal
            SumValues(FBO1.Texture, shaderAssemblyHorizontal, FBO2);

            ProcessFilterPass(FBO2.Texture, FBO1);
        }
        
        private void SumValues(ITexture2D sourceTexture, IShaderProgram program, IRenderSurface fbo)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            sourceTexture.Activate();
            fbo.Activate();
            program.Activate();
            //int SATSampler = GL.GetUniformLocation(program.ProgramID, "sourceSampler");
            //GL.Uniform1(SATSampler, 1);

            //GL.ActiveTexture(TextureUnit.Texture1);
            //GL.BindTexture(TextureTarget.Texture2D, sourceTexture.ID);

            program.Uniform("blockLengthX", blockSizeX);
            program.Uniform("blockLengthY", blockSizeY);
            program.Uniform("amountBlockX", amountBlocksX);
            program.Uniform("amountBlockY", amountBlocksY);

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            GL.ActiveTexture(TextureUnit.Texture1);
            program.Deactivate();

            fbo.Deactivate();
            sourceTexture.Deactivate();
        }
        
        //Filter FBO2 and Write result in FBO1[0]
        private void ProcessFilterPass(ITexture2D satTexture, IRenderSurface fbo)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            int halfKernelX = kernelSizeX/2;
            int halfKernelY = kernelSizeY/2;

            fbo.Activate();

            shaderSATFilter.Activate();
            satTexture.Activate();
            int SATSampler = GL.GetUniformLocation(shaderSATFilter.ProgramID, "sourceSampler");
            GL.Uniform1(SATSampler, 1);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, satTexture.ID);

            shaderSATFilter.Uniform("width", satTexture.Width);
            shaderSATFilter.Uniform("height", satTexture.Height);
            shaderSATFilter.Uniform("halfKernelX", halfKernelX);
            shaderSATFilter.Uniform("halfKernelY", halfKernelY);

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            satTexture.Deactivate();
            shaderSATFilter.Deactivate();

            fbo.Deactivate();

        }

        
    }
}
