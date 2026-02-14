using System.Collections.Generic;
using System.Windows.Forms;

namespace _3Dvishok.Engine.Input
{
    public class InputState
    {
        private readonly HashSet<Keys> pressedKeys = new HashSet<Keys>();

        public bool SetDown(Keys key)
        {
            return pressedKeys.Add(key);
        }

        public void SetUp(Keys key)
        {
            pressedKeys.Remove(key);
        }

        public bool IsDown(Keys key)
        {
            return pressedKeys.Contains(key);
        }

        public bool IsAnyShiftDown()
        {
            return pressedKeys.Contains(Keys.ShiftKey)
                   || pressedKeys.Contains(Keys.LShiftKey)
                   || pressedKeys.Contains(Keys.RShiftKey);
        }
    }
}
