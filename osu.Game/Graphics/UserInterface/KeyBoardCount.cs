//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Input;

namespace osu.Game.Graphics.UserInterface
{
    public class KeyBoardCount : Count
    {
        public Key Key { get; }
        public KeyBoardCount(string name, Key key) : base(name)
        {
            Key = key;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == this.Key) IsLit = true;
            return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            if (args.Key == this.Key) IsLit = false;
            return base.OnKeyUp(state, args);
        }
    }
}
