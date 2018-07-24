﻿namespace ExampleBrowser
{
	using OpenTK.Graphics.OpenGL;
	using System.ComponentModel.Composition;
	using System.Drawing;
	using Zenseless.Patterns;
	using Zenseless.Geometry;
	using Zenseless.HLGL;
	using Zenseless.OpenGL;

	/// <summary>
	/// Compares texture minification filter methods
	/// </summary>
	[ExampleDisplayName]
	[Export(typeof(IExample))]
	class TextureMinFilterExample : IExample
	{
		[ImportingConstructor]
		public TextureMinFilterExample([Import] IContentLoader contentLoader, [Import] ITime time)
		{
			texBackground = contentLoader.Load<ITexture2D>("mountains");
			texBackground.WrapFunction = TextureWrapFunction.Repeat;
			GL.Enable(EnableCap.Texture2D); //todo: only for non shader pipeline relevant -> remove at some point
			this.time = time;
		}

		public void Render()
		{
			GL.Clear(ClearBufferMask.ColorBufferBit);
			//color is multiplied with texture color white == no change
			GL.Color3(Color.White);
			//draw with different filter modes - defines how texture colors are mapped to pixel colors
			texBackground.Filter = TextureFilterMode.Linear; //filter by taking the nearest texel's color as a pixels color
			DrawTexturedRect(new Box2D(-1, -1, 1, 2), texBackground, texCoord);
			texBackground.Filter = TextureFilterMode.Mipmap; //filter by calculating the pixels color as a weighted average of the neighboring texel's colors
			DrawTexturedRect(new Box2D(0, -1, 1, 2), texBackground, texCoord);
		}

		public void Update()
		{
			if (texCoord.SizeX > 100f || texCoord.SizeX < 1f) scaleFactor = -scaleFactor;
			float factor = scaleFactor * time.DeltaTime;
			texCoord.SizeX *= 1 + factor;
			texCoord.SizeY *= 1 + factor;
			//texCoord.CenterX += factor * 0.1f;
		}

		private ITexture texBackground;
		private Box2D texCoord = new Box2D(0, 0, 1, 1);
		private float scaleFactor = 1f;
		private readonly ITime time;

		private static void DrawTexturedRect(IReadOnlyBox2D rect, ITexture tex, IReadOnlyBox2D texCoords)
		{
			tex.Activate();
			rect.DrawTexturedRect(texCoords);
			tex.Deactivate();
		}
	}
}