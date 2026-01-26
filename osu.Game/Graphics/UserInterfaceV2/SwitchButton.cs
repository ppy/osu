// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class SwitchButton : Checkbox
    {
        public const float WIDTH = 60;

        private readonly Box fill;
        private readonly CircularContainer content;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private Sample? sampleChecked;
        private Sample? sampleUnchecked;

        public SwitchButton()
        {
            Size = new Vector2(WIDTH, 20);

            InternalChild = content = new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                BorderColour = Color4.White,
                BorderThickness = 4,
                Masking = true,
                Children = new Drawable[]
                {
                    fill = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                    },
                }
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio)
        {
            sampleChecked = audio.Samples.Get(@"UI/check-on");
            sampleUnchecked = audio.Samples.Get(@"UI/check-off");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindDisabledChanged(_ => updateState());
            Current.BindValueChanged(_ => updateState(), true);

            FinishTransforms(true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        protected override void OnUserChange(bool value)
        {
            base.OnUserChange(value);
            PlaySample(value);
        }

        public void PlaySample(bool value)
        {
            if (value)
                sampleChecked?.Play();
            else
                sampleUnchecked?.Play();
        }

        private void updateState()
        {
            Color4 fillColour = colourProvider.Background6;
            Color4 borderColour = colourProvider.Light4;

            if (IsHovered)
                borderColour = colourProvider.Highlight1;
            else if (Current.Value)
                borderColour = colourProvider.Highlight1.Darken(0.1f);

            if (Current.Value)
                fillColour = borderColour;

            if (Current.Disabled)
            {
                fillColour = fillColour.Darken(0.4f);
                borderColour = borderColour.Darken(0.4f);
            }

            fill.FadeColour(fillColour, 250, Easing.OutQuint);

            content.TransformTo(nameof(BorderColour), (ColourInfo)borderColour, 250, Easing.OutQuint);
            content.ResizeWidthTo(Current.Value ? 1 : 0.75f, 250, Easing.OutQuint);
        }
    }
}
