using Example.src.controller;
using Zenseless.ExampleFramework;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var window = new ExampleWindow();
            //var view = new View();
            var visual = new TestVisual(window.RenderContext.RenderState, window.ContentLoader);
            var game = new Game(window.ContentLoader, window.RenderContext.RenderState);
            //window.Render += () => visual.RenderDeferred();
            window.Render += () => game.Render();
            //window.Render += () => visual.RenderPhong();
            //window.Update += (dt) => visual.Update(dt);
            window.Update += (dt) => game.Update(dt);
            //window.Resize += (width, height) => visual.Resize(width, height);
            window.Resize += (width, height) => game.Resize(width, height);
            window.Run();

        }

        private static void Window_Update(float updatePeriod)
        {
            throw new System.NotImplementedException();
        }
    }
}
