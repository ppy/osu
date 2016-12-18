//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Configuration;

namespace osu.Game.Graphics.UserInterface
{
    public class KeyCounterMouse : KeyCounter
    {
        public MouseButton Button { get; }
        public KeyCounterMouse(string name, MouseButton button) : base(name)
        {
            Button = button;
        }
        private Bindable<bool> mouseButtonsDisabled;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            mouseButtonsDisabled = config.GetBindable<bool>(OsuConfig.MouseDisableButtons);
        }

        public override bool Contains(Vector2 screenSpacePos) => true;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (args.Button == this.Button && !mouseButtonsDisabled.Value) IsLit = true;
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (args.Button == this.Button && !mouseButtonsDisabled.Value) IsLit = false;
            return base.OnMouseUp(state, args);
        }
    }
}
