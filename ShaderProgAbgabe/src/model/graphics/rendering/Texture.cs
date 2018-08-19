using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenseless.HLGL;

namespace Example.src.model.graphics.rendering
{
    class Texture
    {
        private int textureId = 0;
        public ITexture2D text;
        private float uvResX;
        private float uvResY;
        private int elemX;
        private int elemY;
        private int currentElement;
        private Texture()
        {

        }

        public Texture(ITexture2D texture)
        {
            text = texture;
            
            SetAmount(1, 1);
            SetCurrentElement(0);
        }

        public int GetId()
        {
            return textureId;
        }
        public void Activate()
        {
            text.Activate();
        }

        public void Deactivate()
        {
            text.Deactivate();
        }

        public void SetAmount(int x, int y)
        {
            SetAmountX(x);
            SetAmountY(y);
        }

        public Box2 GetUvOfElement(int x, int y)
        {
            float left = x * uvResX;
            float right = left + uvResX;
            float top = y * uvResY;
            float bottom = top + uvResY;
            Box2 res = new Box2(left, bottom, right, top);
            return res;
        }

        public Box2 GetUvOfElement(int no)
        {
            int x = no % elemX;
            int y = no / elemX;
            return GetUvOfElement(x, y);
        }

        public void SetAmountY(int amount)
        {
            uvResY = 1 / (float)amount;
            elemY = amount;
        }

        public void SetAmountX(int amount)
        {
            uvResX = 1 / (float)amount;
            elemX = amount;
        }

        public void SetElementResX(int res)
        {
            int texResX = text.Width;
            int amountX = texResX / res;
            SetAmountX(amountX);
        }

        public void SetElementResY(int res)
        {
            int texResY = text.Height;
            int amountY = texResY / res;
            SetAmountY(amountY);
        }

        public Box2 GetCurrentUvBounds()
        {
            return GetUvOfElement(currentElement);
        }

        public int GetCurrentElementNumber()
        {
            return currentElement;
        }

        public bool SetCurrentElement(int element)
        {
            if (element <= GetAmountOfElements())
            {
                currentElement = element;
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetAmountOfElements()
        {
            return elemX * elemY;
        }

        public void SetNextElement()
        {
            int nextElement = currentElement;
            nextElement++;
            if (nextElement >= GetAmountOfElements())
            {
                nextElement = 0;
            }
            SetCurrentElement(nextElement);
        }

        public void SetPreviousElement()
        {
            int previousElement = currentElement;
            previousElement--;
            if (previousElement < 0)
            {
                previousElement = GetAmountOfElements();
            }
            SetCurrentElement(previousElement);
        }

        public Texture Clone()
        {
            Texture text = new Texture();
            text.text = this.text;
            text.textureId = textureId;
            text.uvResX = uvResX;
            text.uvResY = uvResY;
            text.elemX = elemX;
            text.elemY = elemY;
            text.currentElement = currentElement;
            return text;
        }
    }
}
