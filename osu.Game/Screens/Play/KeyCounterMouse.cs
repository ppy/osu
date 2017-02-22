// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Input;

namespace osu.Game.Screens.Play
{
    public class KeyCounterMouse : KeyCounter
    {
        public MouseButton Button { get; }
        public KeyCounterMouse(string name, MouseButton button) : base(name)
        {
            Button = button;
        }

        public override bool Contains(Vector2 screenSpacePos) => true;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (args.Button == Button) IsLit = true;
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (args.Button == Button) IsLit = false;
            return base.OnMouseUp(state, args);
        }
    }
}
