using Example.src.controller.logic;
using Example.src.controller.states;
using Example.src.model.entitys;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.graphics.ui
{
    class UIElement : Entity
    {
        public UIElement()
        {
            color = new System.Numerics.Vector4(1);
            texture = null;
            clickable = false;
        }

        public bool clickable;
        public System.Numerics.Vector4 color;
        public Zenseless.HLGL.ITexture2D texture;
        public IAction action;
        public IState state;
        public Box2 GetBounds()
        {
            float posX = transform.position.X;
            float posY = transform.position.Y;
            float halfX = 0.5f * transform.scale.X;
            float halfY = 0.5f * transform.scale.Y;
            Box2 box = new OpenTK.Box2(posX - halfX, posY + halfY, posX + halfX, posY - halfY);
            return box;
        }
    }
}
