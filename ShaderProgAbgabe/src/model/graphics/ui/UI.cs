using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.model.graphics.ui
{
    class UI
    {
        List<UIElement> elements;
        
        public UI()
        {
            elements = new List<UIElement>();
        }

        public List<UIElement> GetUIElements()
        {
            return elements;
        }

        public void AddElement(UIElement element)
        {
            elements.Add(element);
        }

        public void Click(float x, float y)
        {
            Vector2 position = new Vector2(x, y);
            for(int i = 0; i < elements.Count; i++)
            {
                UIElement tmp = elements[i];
                if (tmp.enabled && tmp.clickable && tmp.action != null)
                {
                    CheckIfClicked(position, tmp);
                }
            }
        }

        private void CheckIfClicked(Vector2 mousePos, UIElement element)
        {
            Box2 bounds = element.GetBounds();
            if(bounds.Contains(mousePos))
            {
                element.action.Execute();
            }
        }
    }
}
