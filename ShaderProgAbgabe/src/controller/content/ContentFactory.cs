using Example.src.model.graphics.rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace Example.src.controller.content
{
    class ContentFactory
    {
        public static Renderable GetDefaultRenderable(DeferredRenderer renderer, DefaultMesh mesh)
        {
            IShaderProgram geometryShader = renderer.GetShader(DeferredRenderer.DrawableType.deferredDefaultMesh);
            IShaderProgram lightViewShader = renderer.GetShader(DeferredRenderer.DrawableType.lightViewMesh);
            IShaderProgram shadowMapShader = renderer.GetShader(DeferredRenderer.DrawableType.shadowMapMesh);
            Renderable renderable = new Renderable();
            VAO geometry = renderer.GetDrawable(mesh, DeferredRenderer.DrawableType.deferredDefaultMesh);
            VAO lightViewMesh = renderer.GetDrawable(mesh, DeferredRenderer.DrawableType.lightViewMesh);
            VAO shadowMapMesh = renderer.GetDrawable(mesh, DeferredRenderer.DrawableType.shadowMapMesh);
            renderable.SetDeferredGeometryMesh(geometry, geometryShader);
            renderable.SetLightViewMesh(geometry, lightViewShader);
            renderable.SetShadowMapMesh(geometry, shadowMapShader);
            return renderable;
        }
    }
}
