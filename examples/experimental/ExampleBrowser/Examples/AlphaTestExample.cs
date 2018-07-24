﻿namespace ExampleBrowser
{
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;
	using System.ComponentModel.Composition;
	using Zenseless.Geometry;
	using Zenseless.HLGL;

	[ExampleDisplayName]
	[Export(typeof(IExample))]
	class AlphaTestExample : IExample
	{
		[ImportingConstructor]
		public AlphaTestExample([Import] IRenderState renderState)
		{
			//background clear color
			renderState.Set(new ClearColorState(1, 1, 1, 1));
			GL.Enable(EnableCap.AlphaTest);
		}

		public float Threshold { get; set; } = 0.25f;

		public void Render()
		{
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.AlphaFunc(AlphaFunction.Less, 0.1f * Threshold);

			var rect = new Box2D(-.75f, -.75f, 1.5f, 1.5f);
			var colorA = new Color4(1f, 1f, 0f, 1f);
			var colorB = new Color4(0f, 1f, 1f, 0f);

			DrawRect(rect, colorA, colorB);
		}

		public void Update()
		{
		}

		private void DrawRect(IReadOnlyBox2D rectangle, Color4 colorA, Color4 colorB)
		{
			GL.Begin(PrimitiveType.Quads);
			GL.Color4(colorA);
			GL.Vertex2(rectangle.MinX, rectangle.MinY);
			GL.Vertex2(rectangle.MaxX, rectangle.MinY);
			GL.Color4(colorB);
			GL.Vertex2(rectangle.MaxX, rectangle.MaxY);
			GL.Vertex2(rectangle.MinX, rectangle.MaxY);
			GL.End();
		}
	}
}