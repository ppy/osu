// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public abstract partial class CreateRoomButton : ShearedButtonWithIcon, IKeyBindingHandler<PlatformAction>
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Width = 220;

            DarkerColour = colours.Blue3;
            LighterColour = colours.Blue1;

            Icon = OsuIcon.Add;
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            if (e.Repeat)
                return false;

            if (!Enabled.Value)
                return false;

            switch (e.Action)
            {
                case PlatformAction.DocumentNew:
                // might as well also handle new tab. it's a bit of an undefined flow on this screen.
                case PlatformAction.TabNew:
                    TriggerClick();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }
    }
}
