using Example.src.model.graphics.ui;
using Example.src.Test.ui;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Zenseless.HLGL;

namespace Example.src.Test
{
    class IslandUI : UI
    {
        public IslandUI(IContentLoader contentLoader)
        {
            UIElement logo = new UIElement();
            logo.texture = contentLoader.Load<ITexture2D>("logo.*");
            logo.transform.scale = new Vector3(0.5f, 0.5f, 1);
            logo.transform.position = new Vector3(0.75f, -0.75f, 1);
            logo.clickable = true;
            logo.action = new ButtonTestAction();
            UIElement element2 = new UIElement();
            //element2.texture = contentLoader.Load<ITexture2D>("testTexture.*");
            element2.color = new Vector4(Color.Aqua.R, Color.Aqua.G, Color.Aqua.B, 0.1f);
            element2.transform.scale = new Vector3(0.5f, 0.5f, 1);
            element2.transform.position = new Vector3(0.75f, -0.75f, 0);
            AddElement(element2);
            AddElement(logo);
        }
    }
}
