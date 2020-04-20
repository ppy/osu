// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Screens.Mvis.Buttons
{
    public class ToggleableButton : BottomBarButton
    {
        public Bindable<bool> ToggleableValue = new Bindable<bool>();
        protected readonly bool defaultValue;

        [Resolved]
        private OsuColour colour { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            ToggleableValue.Value = defaultValue;
            this.Action = () =>
            {
                Toggle();
            };
        }

        public void Toggle()
        {
            switch ( ToggleableValue.Value )
            {
                case true:
                    ToggleableValue.Value = false;
                    bgBox.FadeColour( Color4Extensions.FromHex("#5a5a5a"), 500, Easing.OutQuint );
                    break;

                case false:
                    ToggleableValue.Value = true;
                    bgBox.FadeColour( colour.Green, 500, Easing.OutQuint );
                    break;
            }
        }
    }
}
