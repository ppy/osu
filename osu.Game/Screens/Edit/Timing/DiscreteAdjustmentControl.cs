// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Numerics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Timing
{
    /// <summary>
    /// A button with variable constant output based on hold position and length.
    /// </summary>
    public partial class DiscreteAdjustmentControl<T> : CompositeDrawable
        where T : struct, INumber<T>, IMinMaxValue<T>, IMultiplyOperators<T, T, T>
    {
        public Action<T>? Action;

        private readonly T baseIncrement;

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

        public DiscreteAdjustmentControl(T baseIncrement)
        {
            this.baseIncrement = baseIncrement;

            CornerRadius = 5;
            Masking = true;
            RelativeSizeAxes = Axes.Both;

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
            background.Colour = colourProvider.Dark2;

            for (int i = 1; i <= adjust_levels; i++)
            {
                Content.Add(new IncrementBox(i, baseIncrement));
                Content.Add(new IncrementBox(-i, baseIncrement));
            }
        }

        protected override bool OnHover(HoverEvent e) => true;

        protected override bool OnClick(ClickEvent e)
        {
            var hoveredBox = Content.OfType<IncrementBox>().FirstOrDefault(d => d.IsHovered);
            if (hoveredBox == null)
                return false;

            Action?.Invoke(baseIncrement * T.CreateTruncating(hoveredBox.Multiplier));

            hoveredBox.Flash();

            repeatBehaviour.SampleFrequencyModifier = ((double)hoveredBox.Multiplier / max_multiplier) * 0.2;
            return true;
        }

        private partial class IncrementBox : CompositeDrawable
        {
            public readonly int Multiplier;

            private readonly Box box;
            private readonly OsuSpriteText text;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public IncrementBox(int index, T amount)
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
                        Anchor = direction | Anchor.y1,
                        Origin = direction | Anchor.y1,
                        Font = OsuFont.Default.With(size: 14, weight: FontWeight.Bold),
                        Text = $"{(Multiplier > 0 ? "+" : "")}{(amount * T.CreateTruncating(Multiplier)).ToStandardFormattedString(maxDecimalDigits: 2)}",
                        Padding = new MarginPadding(4),
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

            private int convertMultiplier(int m)
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
                    .FadeTo(0.4f, 40, Easing.OutQuint)
                    .Then()
                    .FadeTo(0.2f, 700, Easing.OutPow10);

                text
                    .MoveToX(Multiplier > 0 ? 10 : -10, 40, Easing.OutQuint)
                    .Then()
                    .MoveToX(0, 700, Easing.OutBounce);
            }
        }
    }
}
