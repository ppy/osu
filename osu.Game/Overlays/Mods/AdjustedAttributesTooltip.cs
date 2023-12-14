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
    public partial class AdjustedAttributesTooltip : VisibilityContainer, ITooltip<AdjustedAttributesTooltip.Data?>
    {
        private FillFlowContainer attributesFillFlow = null!;

        private Container content = null!;

        private Data? data;

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

        private void updateDisplay()
        {
            attributesFillFlow.Clear();

            if (data != null)
            {
                attemptAdd("CS", bd => bd.CircleSize);
                attemptAdd("HP", bd => bd.DrainRate);
                attemptAdd("OD", bd => bd.OverallDifficulty);
                attemptAdd("AR", bd => bd.ApproachRate);
            }

            if (attributesFillFlow.Any())
                content.Show();
            else
                content.Hide();

            void attemptAdd(string name, Func<BeatmapDifficulty, double> lookup)
            {
                double originalValue = lookup(data.OriginalDifficulty);
                double adjustedValue = lookup(data.AdjustedDifficulty);

                if (!Precision.AlmostEquals(originalValue, adjustedValue))
                    attributesFillFlow.Add(new AttributeDisplay(name, originalValue, adjustedValue));
            }
        }

        public void SetContent(Data? data)
        {
            if (this.data == data)
                return;

            this.data = data;
            updateDisplay();
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        public void Move(Vector2 pos) => Position = pos;

        public class Data
        {
            public BeatmapDifficulty OriginalDifficulty { get; }
            public BeatmapDifficulty AdjustedDifficulty { get; }

            public Data(BeatmapDifficulty originalDifficulty, BeatmapDifficulty adjustedDifficulty)
            {
                OriginalDifficulty = originalDifficulty;
                AdjustedDifficulty = adjustedDifficulty;
            }
        }

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
