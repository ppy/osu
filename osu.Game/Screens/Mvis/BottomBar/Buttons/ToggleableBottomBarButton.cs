// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Screens.Mvis.BottomBar.Buttons
{
    public class ToggleableBottomBarButton : BottomBarButton
    {
        public BindableBool ToggleableValue = new BindableBool();

        public bool DefaultValue { get; set; }

        [Resolved]
        private OsuColour colour { get; set; }

        protected override void LoadComplete()
        {
            ToggleableValue.Value = DefaultValue;
            ToggleableValue.ValueChanged += _ => UpdateVisuals();

            UpdateVisuals();

            base.LoadComplete();
        }

        protected override bool OnClick(Framework.Input.Events.ClickEvent e)
        {
            Toggle();
            return base.OnClick(e);
        }

        public void Toggle()
        {
            ToggleableValue.Toggle();
        }

        private void UpdateVisuals()
        {
            switch ( ToggleableValue.Value )
            {
                case true:
                    bgBox.FadeColour( colour.Green, 500, Easing.OutQuint );
                    break;

                case false:
                    bgBox.FadeColour( Color4Extensions.FromHex("#5a5a5a"), 500, Easing.OutQuint );
                    break;
            }
        }
    }
}
