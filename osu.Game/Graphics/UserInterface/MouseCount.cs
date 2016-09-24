//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Input;

namespace osu.Game.Graphics.UserInterface
{
    public class MouseCount : Count
    {
        public MouseButton Button { get; }
        public MouseCount(string name, MouseButton button) : base(name)
        {
            Button = button;
        }

        public override bool Contains(Vector2 screenSpacePos) => true;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (args.Button == this.Button) IsLit = true;
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (args.Button == this.Button) IsLit = false;
            return base.OnMouseUp(state, args);
        }
    }
}
