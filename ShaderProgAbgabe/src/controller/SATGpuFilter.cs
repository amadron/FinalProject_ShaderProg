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
        IRenderState renderState;
        IShaderProgram fullScreenQuad;
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

        public SATGpuFilter(IContentLoader contentLoader, IRenderState renderState,int amountBlocksX, int amountBlocksY, int resX, int resY, int kernelSizeY, int kernelSizeX)
        {
            this.contentLoader = contentLoader;
            this.renderState = renderState;
            shaderSumHorizontal = contentLoader.LoadPixelShader("SATSumHorizontal");   
            shaderSumVertical = contentLoader.LoadPixelShader("SATSumVertical");

            shaderAssemblyVertical = contentLoader.LoadPixelShader("SATAssemblyPassVertical");
            shaderAssemblyHorizontal = contentLoader.LoadPixelShader("SATAssemblyPassHorizontal");

            shaderSATFilter = contentLoader.LoadPixelShader("SATFiltering");
            fullScreenQuad = contentLoader.Load<IShaderProgram>("FullQuad.*");
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
            //return FBO1.Texture;
            return FBO2.Texture;
            //return testTexture;
        }

        public void Test()
        {
            FilterTexture(testTexture);
        }

        public void FilterTexture(ITexture2D sourceTexture)
        {
            DrawFullQuad(sourceTexture, FBO1);
            //Vertical sum Pass
            SumValues(FBO1.Texture, shaderSumVertical, FBO2);
            //Assembly Block Values Vertical
            SumValues(FBO2.Texture, shaderAssemblyVertical, FBO1);
            
            //Horizontal sum Pass
            SumValues(FBO1.Texture, shaderSumHorizontal, FBO2);
            //Assembly Block Values Horizontal
            SumValues(FBO2.Texture, shaderAssemblyHorizontal, FBO1);

            ProcessFilterPass(FBO1.Texture, FBO2);
        }

        private void DrawFullQuad(ITexture2D sourceTexture, IRenderSurface fbo)
        {
            int SATSampler = GL.GetUniformLocation(fullScreenQuad.ProgramID, "inputTexture");
            fbo.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderState.Set(new DepthTest(true));
            renderState.Set(new FaceCullingModeState(FaceCullingMode.BACK_SIDE));

            fullScreenQuad.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Uniform1(SATSampler, 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, sourceTexture.ID);

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            fullScreenQuad.Deactivate();

            renderState.Set(new FaceCullingModeState(FaceCullingMode.NONE));
            renderState.Set(new DepthTest(false));
            fbo.Deactivate();
        }
        
        private void SumValues(ITexture2D sourceTexture, IShaderProgram program, IRenderSurface fbo)
        {
            int SATSampler = GL.GetUniformLocation(fullScreenQuad.ProgramID, "sourceSampler");
            fbo.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            program.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Uniform1(SATSampler, 0);

            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindTexture(TextureTarget.Texture2D, sourceTexture.ID);
            program.Uniform("blockLengthX", blockSizeX);
            program.Uniform("blockLengthY", blockSizeX);
            program.Uniform("amountBlockX", amountBlocksX);
            program.Uniform("amountBlockY", amountBlocksX);
            //satFilter.GetFilterTexture().Activate();
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            //satFilter.GetFilterTexture().Deactivate();
            program.Deactivate();

            fbo.Deactivate();
        }
        
        //Filter FBO2 and Write result in FBO1[0]
        private void ProcessFilterPass(ITexture2D sourceTexture, IRenderSurface fbo)
        {
            int halfKernelX = kernelSizeX/2;
            int halfKernelY = kernelSizeY/2;

            fbo.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            shaderSATFilter.Activate();

            int SATSampler = GL.GetUniformLocation(shaderSATFilter.ProgramID, "sourceSampler");
            GL.Uniform1(SATSampler, 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, sourceTexture.ID);
            //sourceTexture.Activate();

            shaderSATFilter.Uniform("width", sourceTexture.Width);
            shaderSATFilter.Uniform("height", sourceTexture.Height);
            shaderSATFilter.Uniform("halfKernelX", halfKernelX);
            shaderSATFilter.Uniform("halfKernelY", halfKernelY);

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            //sourceTexture.Deactivate();

            shaderSATFilter.Deactivate();

            fbo.Deactivate();



            
        }

        
    }
}
