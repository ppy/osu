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

        private readonly Box background;

        private readonly RepeatingButtonBehaviour repeatBehaviour;

        private OsuSpriteText incrementText;

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

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue
                },
                incrementText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Default.With(weight: FontWeight.SemiBold),
                    Padding = new MarginPadding(5),
                    Depth = float.MinValue
                },
                repeatBehaviour = new RepeatingButtonBehaviour(this)
                {
                    RepeatBegan = () => editorBeatmap.BeginChange(),
                    RepeatEnded = () => editorBeatmap.EndChange()
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colourProvider.Dark2;

            for (int i = 1; i <= adjust_levels; i++)
            {
                AddInternal(new IncrementBox(i, baseIncrement));
                AddInternal(new IncrementBox(-i, baseIncrement));
            }

            AddInternal(incrementText = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Default.With(size: 14, weight: FontWeight.Bold),
                Padding = new MarginPadding(4),
                Alpha = 0,
            });
        }

        private IncrementBox? hoveredBox => InternalChildren.OfType<IncrementBox>().FirstOrDefault(d => d.IsHovered);

        private IncrementBox? lastHovered;

        protected override void Update()
        {
            base.Update();

            if (hoveredBox == lastHovered)
                return;

            lastHovered = hoveredBox;

            if (lastHovered != null)
            {
                incrementText
                    .MoveToX(Math.Sign(lastHovered.Multiplier) * Math.Abs(lastHovered.Index), 400, Easing.OutQuint)
                    .FadeTo(0.8f, 200, Easing.OutQuint);

                incrementText.Text = $"{(lastHovered.Multiplier > 0 ? "+" : "")}{(lastHovered.Amount * T.CreateTruncating(lastHovered.Multiplier)).ToStandardFormattedString(maxDecimalDigits: 2)}";
            }
            else
            {
                incrementText
                    .MoveToX(0, 200, Easing.OutQuint)
                    .FadeOut(200, Easing.OutQuint);
            }
        }

        protected override bool OnHover(HoverEvent e) => true;

        protected override bool OnClick(ClickEvent e)
        {
            if (hoveredBox is IncrementBox b)
            {
                Action?.Invoke(baseIncrement * T.CreateTruncating(b.Multiplier));

                b.Flash();

                incrementText
                    .MoveToX(Math.Sign(b.Multiplier) * (Math.Abs(b.Index) * 5))
                    .MoveToX(Math.Sign(b.Multiplier) * Math.Abs(b.Index), 700, Easing.OutQuint);

                incrementText.FadeTo(1).FadeTo(0.8f, 1400, Easing.OutQuint);

                repeatBehaviour.SampleFrequencyModifier = ((double)b.Multiplier / max_multiplier) * 0.2;
                return true;
            }

            return false;
        }

        private partial class IncrementBox : CompositeDrawable
        {
            public readonly T Amount;
            public readonly int Multiplier;
            public readonly float Index;

            private readonly Box box;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public IncrementBox(int index, T amount)
            {
                Index = index;
                Amount = amount;
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
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                box.Colour = colourProvider.Background1;
                box.FadeTo(0.1f, 500, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            public void Flash()
            {
                box
                    .FadeTo(0.4f, 40, Easing.OutQuint)
                    .Then()
                    .FadeTo(0.2f, 700, Easing.OutPow10);
            }
        }
    }
}
