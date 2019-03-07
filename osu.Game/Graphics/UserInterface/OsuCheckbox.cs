// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuCheckbox : Checkbox
    {
        private Bindable<bool> bindable;

        public Bindable<bool> Bindable
        {
            set
            {
                bindable = value;
                Current.BindTo(bindable);
            }
        }

        public Color4 CheckedColor { get; set; } = Color4.Cyan;
        public Color4 UncheckedColor { get; set; } = Color4.White;
        public int FadeDuration { get; set; }

        public string LabelText
        {
            get => labelSpriteText?.Text;
            set
            {
                if (labelSpriteText != null)
                    labelSpriteText.Text = value;
            }
        }

        public MarginPadding LabelPadding
        {
            get => labelSpriteText?.Padding ?? new MarginPadding();
            set
            {
                if (labelSpriteText != null)
                    labelSpriteText.Padding = value;
            }
        }

        protected readonly Nub Nub;

        private readonly SpriteText labelSpriteText;
        private SampleChannel sampleChecked;
        private SampleChannel sampleUnchecked;

        public OsuCheckbox()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                labelSpriteText = new OsuSpriteText(),
                Nub = new Nub
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding { Right = 5 },
                },
                new HoverClickSounds()
            };

            Nub.Current.BindTo(Current);

            Current.DisabledChanged += disabled =>
            {
                Alpha = disabled ? 0.3f : 1;
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.ValueChanged += enabled =>
            {
                if (enabled.NewValue)
                    sampleChecked?.Play();
                else
                    sampleUnchecked?.Play();
            };
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

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleChecked = audio.Sample.Get(@"UI/check-on");
            sampleUnchecked = audio.Sample.Get(@"UI/check-off");
        }
    }
}
