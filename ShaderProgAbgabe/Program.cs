﻿using Zenseless.ExampleFramework;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var window = new ExampleWindow();
            //var view = new View();
            var visual = new TestVisual(window.RenderContext.RenderState, window.ContentLoader);
            window.Render += () => visual.RenderPhong();
            window.Update += (dt) => visual.Update(dt);
            window.Run();

        }

        private static void Window_Update(float updatePeriod)
        {
            throw new System.NotImplementedException();
        }
    }
}
