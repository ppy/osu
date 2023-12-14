// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public partial class AdjustedAttributesTooltip : VisibilityContainer, ITooltip
    {
        private FillFlowContainer attributesFillFlow = null!;

        private Container content = null!;

        private BeatmapDifficulty? originalDifficulty;
        private BeatmapDifficulty? adjustedDifficulty;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                content = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Gray3,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Vertical = 10, Horizontal = 15 },
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "One or more values are being adjusted by mods that change speed.",
                                },
                                attributesFillFlow = new FillFlowContainer
                                {
                                    Direction = FillDirection.Vertical,
                                    AutoSizeAxes = Axes.Both
                                }
                            }
                        }
                    }
                },
            };

            updateDisplay();
        }

        public void UpdateAttributes(BeatmapDifficulty original, BeatmapDifficulty adjusted)
        {
            originalDifficulty = original;
            adjustedDifficulty = adjusted;

            if (IsLoaded)
                updateDisplay();
        }

        private void updateDisplay()
        {
            attributesFillFlow.Clear();

            if (originalDifficulty == null || adjustedDifficulty == null)
                return;

            attemptAdd("CS", bd => bd.CircleSize);
            attemptAdd("HP", bd => bd.DrainRate);
            attemptAdd("OD", bd => bd.OverallDifficulty);
            attemptAdd("AR", bd => bd.ApproachRate);

            if (attributesFillFlow.Any())
                content.Show();
            else
                content.Hide();

            void attemptAdd(string name, Func<BeatmapDifficulty, double> lookup)
            {
                double a = lookup(originalDifficulty);
                double b = lookup(adjustedDifficulty);

                if (!Precision.AlmostEquals(a, b))
                    attributesFillFlow.Add(new AttributeDisplay(name, a, b));
            }
        }

        public void SetContent(object content)
        {
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        public void Move(Vector2 pos) => Position = pos;

        private partial class AttributeDisplay : CompositeDrawable
        {
            public AttributeDisplay(string name, double original, double adjusted)
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new OsuSpriteText
                {
                    Font = OsuFont.Default.With(weight: FontWeight.Bold),
                    Text = $"{name}: {original:0.0#} → {adjusted:0.0#}"
                };
            }
        }
    }
}
