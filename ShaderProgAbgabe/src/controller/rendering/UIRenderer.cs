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
        IRenderState renderstate;
        IContentLoader contentLoader;
        IShaderProgram sceneShader;
        IShaderProgram uiShader;

        List<UIElement> elements;

        public UIRenderer(IContentLoader contentLoader, IRenderState renderstate)
        {
            this.contentLoader = contentLoader;
            this.renderstate = renderstate;
            sceneShader = contentLoader.Load<IShaderProgram>("FullQuad.*");
            uiShader = contentLoader.Load<IShaderProgram>("UIElement.*");
        }

        public void Render(ITexture2D sceneTexture, List<UIElement> elements)
        {
            RenderScene(sceneTexture);
            for(int i = 0; i < elements.Count; i++)
            {
                RenderUIElement(elements[i]);
            }
        }

        void RenderScene(ITexture2D scene)
        {
            sceneShader.Activate();
            scene.Activate();

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            scene.Deactivate();
            sceneShader.Deactivate();
        }
        
        void RenderUIElement(UIElement element)
        {
            renderstate.Set(new DepthTest(true));
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            uiShader.Activate();
            int hasTexture = 0;
            if(element.texture != null)
            {
                element.texture.Activate();
                hasTexture = 1;
            }

            uiShader.Uniform("hasTexture", hasTexture);
            uiShader.Uniform("scale", element.transform.scale);
            uiShader.Uniform("position", element.transform.position);
            uiShader.Uniform("elementColor", element.color);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            if (element.texture != null)
            {
                element.texture.Deactivate();
            }
            uiShader.Deactivate();
            GL.Disable(EnableCap.Blend);
        }
        
    }
}
