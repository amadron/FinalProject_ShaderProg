using Example.src.controller;
using Zenseless.ExampleFramework;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var window = new ExampleWindow();
            var game = new Game(window.ContentLoader, window.RenderContext.RenderState);
            window.Render += () => game.Render();
            window.Update += (dt) => game.Update(dt);
            window.Resize += (width, height) => game.Resize(width, height);
            window.GameWindow.MouseMove += game.GameWindow_MouseMove;
            window.GameWindow.MouseDown += game.GameWindow_MouseDown;
            window.GameWindow.MouseUp += game.GameWindow_MouseUp;
            window.Run();

        }
    }
}
