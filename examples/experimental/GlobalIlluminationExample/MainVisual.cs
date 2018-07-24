﻿using OpenTK.Graphics.OpenGL4;
using System.Numerics;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace Example
{
	public class MainVisual
	{
		public MainVisual(IRenderState renderState, IContentLoader contentLoader)
		{
			renderState.Set(new DepthTest(true));
			renderState.Set(new FaceCullingModeState(FaceCullingMode.BACK_SIDE));

			shaderProgram = contentLoader.Load<IShaderProgram>("shader.*");
			var mesh = Meshes.CreateCornellBox();
			geometry = VAOLoader.FromMesh(mesh, shaderProgram);
			bufferMaterials.Set(Meshes.CreateCornellBoxMaterial(), BufferUsageHint.StaticDraw);
		}

		public void Render(ITransformation camera, in Vector3 cameraPosition)
		{
			if (shaderProgram is null) return;
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			shaderProgram.Uniform("camera", camera);
			shaderProgram.Uniform("ambient", new Vector3(0.1f));
			shaderProgram.Uniform("lightPosition", new Vector3(0, 0.9f, -0.5f));
			shaderProgram.Uniform("lightColor", new Vector3(0.8f));
			shaderProgram.Uniform(nameof(cameraPosition), cameraPosition);
			var bindingIndex = shaderProgram.GetResourceLocation(ShaderResourceType.UniformBuffer, "bufferMaterials");
			bufferMaterials.ActivateBind(bindingIndex);
			shaderProgram.Activate();
			geometry.Draw();
			shaderProgram.Deactivate();
			bufferMaterials.Deactivate();
		}

		private BufferObject bufferMaterials = new BufferObject(BufferTarget.UniformBuffer);
		private IShaderProgram shaderProgram;
		private IDrawable geometry;
	}
}
