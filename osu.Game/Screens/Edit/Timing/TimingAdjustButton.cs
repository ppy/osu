// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Timing
{
    /// <summary>
    /// A button with variable constant output based on hold position and length.
    /// </summary>
    public partial class TimingAdjustButton : CompositeDrawable
    {
        public Action<double>? Action;

        private readonly double adjustAmount;

        private const int max_multiplier = 10;
        private const int adjust_levels = 4;

        public Container Content { get; set; }

        private readonly Box background;

        private readonly OsuSpriteText text;

        public LocalisableString Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        private readonly RepeatingButtonBehaviour repeatBehaviour;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        public TimingAdjustButton(double adjustAmount)
        {
            this.adjustAmount = adjustAmount;

            CornerRadius = 5;
            Masking = true;

            AddInternal(Content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Depth = float.MaxValue
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.Default.With(weight: FontWeight.SemiBold),
                        Padding = new MarginPadding(5),
                        Depth = float.MinValue
                    }
                }
            });

            AddInternal(repeatBehaviour = new RepeatingButtonBehaviour(this)
            {
                RepeatBegan = () => editorBeatmap.BeginChange(),
                RepeatEnded = () => editorBeatmap.EndChange()
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colourProvider.Background3;

            for (int i = 1; i <= adjust_levels; i++)
            {
                Content.Add(new IncrementBox(i, adjustAmount));
                Content.Add(new IncrementBox(-i, adjustAmount));
            }
        }

        protected override bool OnHover(HoverEvent e) => true;

        protected override bool OnClick(ClickEvent e)
        {
            var hoveredBox = Content.OfType<IncrementBox>().FirstOrDefault(d => d.IsHovered);
            if (hoveredBox == null)
                return false;

            Action?.Invoke(adjustAmount * hoveredBox.Multiplier);

            hoveredBox.Flash();

            repeatBehaviour.SampleFrequencyModifier = (hoveredBox.Multiplier / max_multiplier) * 0.2;
            return true;
        }

        private partial class IncrementBox : CompositeDrawable
        {
            public readonly float Multiplier;

            private readonly Box box;
            private readonly OsuSpriteText text;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public IncrementBox(int index, double amount)
            {
                Multiplier = Math.Sign(index) * convertMultiplier(index);

                float ratio = (float)index / adjust_levels;

                RelativeSizeAxes = Axes.Both;

                Width = 0.5f * Math.Abs(ratio);

                Anchor direction = index < 0 ? Anchor.x2 : Anchor.x0;

                Origin |= direction;

                Depth = Math.Abs(index);

                Anchor = Anchor.TopCentre;

                InternalChildren = new Drawable[]
                {
                    box = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Blending = BlendingParameters.Additive
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = direction,
                        Origin = direction,
                        Font = OsuFont.Default.With(size: 10, weight: FontWeight.Bold),
                        Text = $"{(index > 0 ? "+" : "-")}{Math.Abs(Multiplier * amount)}",
                        Padding = new MarginPadding(2),
                        Alpha = 0,
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                box.Colour = colourProvider.Background1;
                box.Alpha = 0.1f;
            }

            private float convertMultiplier(int m)
            {
                switch (Math.Abs(m))
                {
                    default: return 1;

                    case 2: return 2;

                    case 3: return 5;

                    case 4:
                        return max_multiplier;
                }
            }

            protected override bool OnHover(HoverEvent e)
            {
                box.Colour = colourProvider.Colour0;

                box.FadeTo(0.2f, 100, Easing.OutQuint);
                text.FadeIn(100, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                box.Colour = colourProvider.Background1;

                box.FadeTo(0.1f, 500, Easing.OutQuint);
                text.FadeOut(100, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            public void Flash()
            {
                box
                    .FadeTo(0.4f, 20, Easing.OutQuint)
                    .Then()
                    .FadeTo(0.2f, 400, Easing.OutQuint);

                text
                    .MoveToY(-5, 20, Easing.OutQuint)
                    .Then()
                    .MoveToY(0, 400, Easing.OutQuint);
            }
        }
    }
}
