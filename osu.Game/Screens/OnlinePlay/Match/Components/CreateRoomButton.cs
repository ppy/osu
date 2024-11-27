// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public abstract partial class CreateRoomButton : PurpleRoundedButton, IKeyBindingHandler<PlatformAction>
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            SpriteText.Font = SpriteText.Font.With(size: 14);
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
