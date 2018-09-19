using System;
using System.Collections.Generic;
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

    }
}
