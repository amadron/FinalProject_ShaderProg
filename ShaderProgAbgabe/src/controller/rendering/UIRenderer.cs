using Example.src.model.graphics.ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenseless.HLGL;
using OpenTK.Graphics.OpenGL4;

namespace Example.src.controller.rendering
{
    class UIRenderer
    {
        IShaderProgram sceneShader;
        IShaderProgram uiShader;

        List<UIElement> elements;

        public void Render(ITexture2D sceneTexture)
        {

        }

        void RenderScene(ITexture2D scene)
        {
            sceneShader.Activate();
            scene.Activate();
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            scene.Deactivate();
            sceneShader.Deactivate();
        }

        void RenderUIElements()
        {
            uiShader.Activate();

            uiShader.Deactivate();
        }
        
    }
}
