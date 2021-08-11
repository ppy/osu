// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Screens.Mvis.Plugins.Types;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.Plugins.Internal.BottomBar.Buttons
{
    public class BottomBarSwitchButton : BottomBarButton
    {
        public BindableBool Value = new BindableBool();

        public bool Default { get; set; }

        protected Color4 ActivateColor => ColourProvider.Highlight1;
        protected Color4 InActivateColor => ColourProvider.Background3;

        public BottomBarSwitchButton(IFunctionProvider provider = null)
            : base(provider)
        {
            Value.Value = Default;

            if (provider is IToggleableFunctionProvider toggleableProvider)
            {
                Value.BindTo(toggleableProvider.Bindable);
            }
        }

        protected override void LoadComplete()
        {
            Value.BindValueChanged(_ => updateVisuals(true), true);
            Value.BindDisabledChanged(onDisabledChanged, true);

            ColourProvider.HueColour.BindValueChanged(_ => updateVisuals());

            base.LoadComplete();
        }

        private void onDisabledChanged(bool disabled)
        {
            if (disabled)
                this.ScaleTo(0.8f, 300, Easing.OutQuint).FadeColour(Color4.Gray, 300, Easing.OutQuint);
            else
                this.ScaleTo(1, 300, Easing.OutQuint).FadeColour(Color4.White, 300, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Toggle();
            return base.OnClick(e);
        }

        public void Toggle()
        {
            if (Value.Disabled)
                this.FlashColour(Color4.Red, 1000, Easing.OutQuint);
            else
                Value.Toggle();
        }

        private void updateVisuals(bool animate = false)
        {
            var duration = animate ? 500 : 0;

            switch (Value.Value)
            {
                case true:
                    BgBox.FadeColour(ActivateColor, duration, Easing.OutQuint);
                    ContentFillFlow.FadeColour(Colour4.Black, duration, Easing.OutQuint);
                    if (animate)
                        OnToggledOnAnimation();
                    break;

                case false:
                    BgBox.FadeColour(InActivateColor, duration, Easing.OutQuint);
                    ContentFillFlow.FadeColour(Colour4.White, duration, Easing.OutQuint);
                    break;
            }
        }

        protected virtual void OnToggledOnAnimation()
        {
        }
    }
}
