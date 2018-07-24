﻿namespace Heightfield
{
	using Zenseless.ExampleFramework;
	using Zenseless.OpenGL;

	class Heightfield
	{
		static void Main(string[] args)
		{
			var window = new ExampleWindow();
			var camera = window.GameWindow.CreateOrbitingCameraController(1f, 70f, 0.01f, 20f);
			camera.View.Elevation = 30;

			//var movementState = window.GameWindow.AddFirstPersonCameraEvents(visual.Camera); //TODO: implement
			//window.Update += (dt) => movementState.Update(visual.Camera, dt);

			var visual = new MainVisual(window.RenderContext, window.ContentLoader);
			window.Render += () => visual.Render(camera);
			window.Run();

		}
	}
}
