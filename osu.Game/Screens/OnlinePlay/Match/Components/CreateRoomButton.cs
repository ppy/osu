// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public abstract class CreateRoomButton : PurpleTriangleButton, IKeyBindingHandler<PlatformAction>
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Triangles.TriangleScale = 1.5f;
        }

        public bool OnPressed(PlatformAction action)
        {
            if (!Enabled.Value)
                return false;

            switch (action)
            {
                case PlatformAction.DocumentNew:
                // might as well also handle new tab. it's a bit of an undefined flow on this screen.
                case PlatformAction.TabNew:
                    TriggerClick();
                    return true;
            }

            return false;
        }

        public void OnReleased(PlatformAction action)
        {
        }
    }
}
