// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.BottomBar.Buttons
{
    public class BottomBarSwitchButton : BottomBarButton
    {
        public BindableBool ToggleableValue = new BindableBool();
        private SkinnableSprite off;
        private SkinnableSprite on;
        public bool DefaultValue { get; set; }

        protected override string BackgroundTextureName => "MButtonSwitchOff-background";
        protected virtual string SwitchOnBgTextureName => "MButtonSwitchOn-background";
        protected virtual ConfineMode TextureConfineMode => ConfineMode.ScaleToFit;

        protected Color4 ActivateColor => ColourProvider.Highlight1;
        protected Color4 InActivateColor => ColourProvider.Background3;

        protected override Drawable CreateBackgroundDrawable => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                off = new SkinnableSprite(BackgroundTextureName, confineMode: TextureConfineMode)
                {
                    CentreComponent = false,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0
                },
                on = new SkinnableSprite(SwitchOnBgTextureName, confineMode: TextureConfineMode)
                {
                    CentreComponent = false,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0
                },
            }
        };

        protected override void LoadComplete()
        {
            ToggleableValue.Value = DefaultValue;
            ToggleableValue.BindValueChanged(_ => updateVisuals(true));

            updateVisuals();

            ColourProvider.HueColour.BindValueChanged(_ => updateVisuals());

            base.LoadComplete();
        }

        protected override bool OnClick(Framework.Input.Events.ClickEvent e)
        {
            Toggle();
            return base.OnClick(e);
        }

        public void Toggle() =>
            ToggleableValue.Toggle();

        private void updateVisuals(bool animate = false)
        {
            var duration = animate ? 500 : 0;

            switch (ToggleableValue.Value)
            {
                case true:
                    BgBox.FadeColour(ActivateColor, duration, Easing.OutQuint);
                    ContentFillFlow.FadeColour(Colour4.Black, duration, Easing.OutQuint);
                    off?.FadeOut(duration, Easing.OutQuint);
                    on?.FadeIn(duration, Easing.OutQuint);
                    if (animate)
                        OnToggledOnAnimation();
                    break;

                case false:
                    off?.FadeIn(duration, Easing.OutQuint);
                    on?.FadeOut(duration, Easing.OutQuint);
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
