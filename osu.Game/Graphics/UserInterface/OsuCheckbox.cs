// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuCheckbox : Checkbox
    {
        public Color4 CheckedColor { get; set; } = Color4.Cyan;
        public Color4 UncheckedColor { get; set; } = Color4.White;
        public int FadeDuration { get; set; }

        /// <summary>
        /// Whether to play sounds when the state changes as a result of user interaction.
        /// </summary>
        protected virtual bool PlaySoundsOnUserChange => true;

        public string LabelText
        {
            set
            {
                if (labelText != null)
                    labelText.Text = value;
            }
        }

        public MarginPadding LabelPadding
        {
            get => labelText?.Padding ?? new MarginPadding();
            set
            {
                if (labelText != null)
                    labelText.Padding = value;
            }
        }

        protected readonly Nub Nub;

        private readonly OsuTextFlowContainer labelText;
        private Sample sampleChecked;
        private Sample sampleUnchecked;

        public OsuCheckbox(bool nubOnRight = true)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            const float nub_padding = 5;

            Children = new Drawable[]
            {
                labelText = new OsuTextFlowContainer(ApplyLabelParameters)
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                },
                Nub = new Nub(),
                new HoverSounds()
            };

            if (nubOnRight)
            {
                Nub.Anchor = Anchor.CentreRight;
                Nub.Origin = Anchor.CentreRight;
                Nub.Margin = new MarginPadding { Right = nub_padding };
                labelText.Padding = new MarginPadding { Right = Nub.EXPANDED_SIZE + nub_padding * 2 };
            }
            else
            {
                Nub.Anchor = Anchor.CentreLeft;
                Nub.Origin = Anchor.CentreLeft;
                Nub.Margin = new MarginPadding { Left = nub_padding };
                labelText.Padding = new MarginPadding { Left = Nub.EXPANDED_SIZE + nub_padding * 2 };
            }

            Nub.Current.BindTo(Current);

            Current.DisabledChanged += disabled => labelText.Alpha = Nub.Alpha = disabled ? 0.3f : 1;
        }

        /// <summary>
        /// A function which can be overridden to change the parameters of the label's text.
        /// </summary>
        protected virtual void ApplyLabelParameters(SpriteText text)
        {
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleChecked = audio.Samples.Get(@"UI/check-on");
            sampleUnchecked = audio.Samples.Get(@"UI/check-off");
        }

        protected override bool OnHover(HoverEvent e)
        {
            Nub.Glowing = true;
            Nub.Expanded = true;
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            Nub.Glowing = false;
            Nub.Expanded = false;
            base.OnHoverLost(e);
        }

        protected override void OnUserChange(bool value)
        {
            base.OnUserChange(value);

            if (PlaySoundsOnUserChange)
            {
                if (value)
                    sampleChecked?.Play();
                else
                    sampleUnchecked?.Play();
            }
        }
    }
}
