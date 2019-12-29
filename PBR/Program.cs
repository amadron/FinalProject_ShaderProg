using Zenseless.ExampleFramework;
using OpenTK.Graphics.OpenGL;

namespace PBR
{
    class Program
    {
        static void Main(string[] args)
        {
            var window = new ExampleWindow();
            var view = new View(window.RenderContext.RenderState, window.ContentLoader);
            window.Render += () => view.Render();
            window.Update += (dt) => view.Update(dt);
            window.GameWindow.KeyDown += view.Event_KeyDown;
            window.GameWindow.KeyUp += view.Event_KeyRelease;
            window.Resize += (width, height) => view.Resize(width, height);
            window.GameWindow.MouseDown += (sender, e) => view.GameWindow_MouseDown(sender, e);
            window.GameWindow.MouseUp += (sender, e) => view.GameWindow_MouseUp(sender, e);
            //window.Render += () => Draw();
            window.Run();
        }



        private static void GameWindow_MouseLeave(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        static void Draw()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(1, 0, 0);
            GL.Vertex2(0.0f, 0.0f);
            GL.Vertex2(0.5f, 0.0f);
            GL.Vertex2(0.5f, 0.5f);
            GL.Vertex2(0.0f, 0.5f);
            GL.End();
        }
    }
}
