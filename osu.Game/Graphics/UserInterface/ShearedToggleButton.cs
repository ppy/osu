// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public class ShearedToggleButton : OsuClickableContainer
    {
        public BindableBool Active { get; } = new BindableBool();

        public LocalisableString Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        private readonly Box background;
        private readonly OsuSpriteText text;

        private Sample? sampleOff;
        private Sample? sampleOn;

        private const float shear = 0.2f;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        /// <summary>
        /// Creates a new <see cref="ShearedToggleButton"/>
        /// </summary>
        /// <param name="width">
        /// The width of the button.
        /// <list type="bullet">
        /// <item>If a non-<see langword="null"/> value is provided, this button will have a fixed width equal to the provided value.</item>
        /// <item>If a <see langword="null"/> value is provided (or the argument is omitted entirely), the button will autosize in width to fit the text.</item>
        /// </list>
        /// </param>
        public ShearedToggleButton(float? width = null)
        {
            Height = 50;
            Padding = new MarginPadding { Horizontal = shear * 50 };

            Content.CornerRadius = 7;
            Content.Shear = new Vector2(shear, 0);
            Content.Masking = true;
            Content.BorderThickness = 2;
            Content.Anchor = Content.Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                text = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.TorusAlternate.With(size: 17),
                    Shear = new Vector2(-shear, 0)
                }
            };

            if (width != null)
            {
                Width = width.Value;
            }
            else
            {
                AutoSizeAxes = Axes.X;
                text.Margin = new MarginPadding { Horizontal = 15 };
            }
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleOn = audio.Samples.Get(@"UI/check-on");
            sampleOff = audio.Samples.Get(@"UI/check-off");
        }

        protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new HoverSounds(sampleSet);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Active.BindValueChanged(_ =>
            {
                updateState();
                playSample();
            });
            Active.BindDisabledChanged(disabled =>
            {
                updateState();
                Action = disabled ? (Action?)null : Active.Toggle;
            }, true);

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

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Content.ScaleTo(0.8f, 2000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            Content.ScaleTo(1, 1000, Easing.OutElastic);
            base.OnMouseUp(e);
        }

        private void updateState()
        {
            var darkerColour = Active.Value ? colourProvider.Highlight1 : colourProvider.Background3;
            var lighterColour = Active.Value ? colourProvider.Colour0 : colourProvider.Background1;

            if (Active.Disabled)
            {
                darkerColour = darkerColour.Darken(0.3f);
                lighterColour = lighterColour.Darken(0.3f);
            }
            else if (IsHovered)
            {
                darkerColour = darkerColour.Lighten(0.3f);
                lighterColour = lighterColour.Lighten(0.3f);
            }

            background.FadeColour(darkerColour, 150, Easing.OutQuint);
            Content.TransformTo(nameof(BorderColour), ColourInfo.GradientVertical(darkerColour, lighterColour), 150, Easing.OutQuint);

            var textColour = Active.Value ? colourProvider.Background6 : colourProvider.Content1;
            if (Active.Disabled)
                textColour = textColour.Opacity(0.6f);

            text.FadeColour(textColour, 150, Easing.OutQuint);
        }

        private void playSample()
        {
            if (Active.Value)
                sampleOn?.Play();
            else
                sampleOff?.Play();
        }
    }
}
