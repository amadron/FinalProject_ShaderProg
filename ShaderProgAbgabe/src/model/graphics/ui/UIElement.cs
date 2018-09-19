using Example.src.model.entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Zenseless.HLGL;

namespace Example.src.model.graphics.ui
{
    class UIElement : Entity
    {
        public UIElement()
        {
            clickable = false;
            color = new Vector4(1);
            texture = null;
        }

        public Vector4 color;
        public ITexture2D texture;
    }
}
