using osu.Framework.Graphics;
using osu.Framework.Input;

namespace osu.Game.Graphics.KeyCounter
{
    /// <summary>
    /// A counter targeted toward a specic keyboard button
    /// </summary>
    class KeyboardCount : Count
    {
        private OpenTK.Input.Key eventKey;

        internal KeyboardCount(string keyName, OpenTK.Input.Key key)
        {
            base.name = keyName;
            eventKey = key;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == eventKey)
            {
                if (IsLit == false)
                    IsLit = true;
            }

            return false;
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            if (args.Key == eventKey)
            {
                if (IsLit == true)
                    IsLit = false;
            }

            return false;
        }
    }
}