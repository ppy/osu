// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Mvis.BottomBar.Buttons
{
    public class BottomBarSwitchButton : BottomBarButton
    {
        public BindableBool ToggleableValue = new BindableBool();
        public bool DefaultValue { get; set; }
        protected override void LoadComplete()
        {
            ToggleableValue.Value = DefaultValue;
            ToggleableValue.BindValueChanged(_ => UpdateVisuals(true));

            UpdateVisuals();

            ColourProvider.HueColour.BindValueChanged(_ => UpdateVisuals());

            base.LoadComplete();
        }

        protected override bool OnClick(Framework.Input.Events.ClickEvent e)
        {
            Toggle();
            return base.OnClick(e);
        }

        public void Toggle() =>
            ToggleableValue.Toggle();

        private void UpdateVisuals(bool animate = false)
        {
            var duration = animate ? 500 : 0;
            switch (ToggleableValue.Value)
            {
                case true:
                    bgBox.FadeColour(ColourProvider.Highlight1, duration, Easing.OutQuint);
                    contentFillFlow.FadeColour(Colour4.Black, duration, Easing.OutQuint);
                    if ( animate )
                        OnToggledOnAnimation();
                    break;

                case false:
                    bgBox.FadeColour(ColourProvider.Background3, duration, Easing.OutQuint);
                    contentFillFlow.FadeColour(Colour4.White, duration, Easing.OutQuint);
                    break;
            }
        }

        protected virtual void OnToggledOnAnimation()
        {
        }
    }
}
