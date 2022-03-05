// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Screens.Ranking.Statistics
{
    /// <summary>
    /// A graph which displays the distribution of hit timing in a series of <see cref="HitEvent"/>s.
    /// </summary>
    public class HitEventTimingDistributionGraph : CompositeDrawable
    {
        /// <summary>
        /// The number of bins on each side of the timing distribution.
        /// </summary>
        private const int timing_distribution_bins = 50;

        /// <summary>
        /// The total number of bins in the timing distribution, including bins on both sides and the centre bin at 0.
        /// </summary>
        private const int total_timing_distribution_bins = timing_distribution_bins * 2 + 1;

        /// <summary>
        /// The centre bin, with a timing distribution very close to/at 0.
        /// </summary>
        private const int timing_distribution_centre_bin_index = timing_distribution_bins;

        /// <summary>
        /// The number of data points shown on each side of the axis below the graph.
        /// </summary>
        private const float axis_points = 5;

        /// <summary>
        /// The currently displayed hit events.
        /// </summary>
        private readonly IReadOnlyList<HitEvent> hitEvents;

        /// <summary>
        /// Creates a new <see cref="HitEventTimingDistributionGraph"/>.
        /// </summary>
        /// <param name="hitEvents">The <see cref="HitEvent"/>s to display the timing distribution of.</param>
        public HitEventTimingDistributionGraph(IReadOnlyList<HitEvent> hitEvents)
        {
            this.hitEvents = hitEvents.Where(e => !(e.HitObject.HitWindows is HitWindows.EmptyHitWindows) && e.Result.IsHit()).ToList();
        }

        private int[] bins;
        private double binSize;
        private double hitOffset;

        private Bar[] barDrawables;

        [BackgroundDependencyLoader]
        private void load()
        {
            if (hitEvents == null || hitEvents.Count == 0)
                return;

            bins = new int[total_timing_distribution_bins];

            binSize = Math.Ceiling(hitEvents.Max(e => Math.Abs(e.TimeOffset)) / timing_distribution_bins);

            // Prevent div-by-0 by enforcing a minimum bin size
            binSize = Math.Max(1, binSize);

            Scheduler.AddOnce(updateDisplay);
        }

        public void UpdateOffset(double hitOffset)
        {
            this.hitOffset = hitOffset;
            Scheduler.AddOnce(updateDisplay);
        }

        private void updateDisplay()
        {
            bool roundUp = true;

            Array.Clear(bins, 0, bins.Length);

            foreach (var e in hitEvents)
            {
                double time = e.TimeOffset + hitOffset;

                double binOffset = time / binSize;

                // .NET's round midpoint handling doesn't provide a behaviour that works amazingly for display
                // purposes here. We want midpoint rounding to roughly distribute evenly to each adjacent bucket
                // so the easiest way is to cycle between downwards and upwards rounding as we process events.
                if (Math.Abs(binOffset - (int)binOffset) == 0.5)
                {
                    binOffset = (int)binOffset + Math.Sign(binOffset) * (roundUp ? 1 : 0);
                    roundUp = !roundUp;
                }

                int index = timing_distribution_centre_bin_index + (int)Math.Round(binOffset, MidpointRounding.AwayFromZero);

                // may be out of range when applying an offset. for such cases we can just drop the results.
                if (index >= 0 && index < bins.Length)
                    bins[index]++;
            }

            if (barDrawables != null)
            {
                for (int i = 0; i < barDrawables.Length; i++)
                {
                    barDrawables[i].UpdateOffset(bins[i]);
                }
            }
            else
            {
                int maxCount = bins.Max();
                barDrawables = new Bar[total_timing_distribution_bins];

                for (int i = 0; i < barDrawables.Length; i++)
                    barDrawables[i] = new Bar(bins[i], maxCount, i == timing_distribution_centre_bin_index);

                Container axisFlow;

                const float axis_font_size = 12;

                InternalChild = new GridContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.8f,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Content = new[] { barDrawables }
                            }
                        },
                        new Drawable[]
                        {
                            axisFlow = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = axis_font_size,
                            }
                        },
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    }
                };

                // Our axis will contain one centre element + 5 points on each side, each with a value depending on the number of bins * bin size.
                double maxValue = timing_distribution_bins * binSize;
                double axisValueStep = maxValue / axis_points;

                axisFlow.Add(new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "0",
                    Font = OsuFont.GetFont(size: axis_font_size, weight: FontWeight.SemiBold)
                });

                for (int i = 1; i <= axis_points; i++)
                {
                    double axisValue = i * axisValueStep;
                    float position = (float)(axisValue / maxValue);
                    float alpha = 1f - position * 0.8f;

                    axisFlow.Add(new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativePositionAxes = Axes.X,
                        X = -position / 2,
                        Alpha = alpha,
                        Text = axisValue.ToString("-0"),
                        Font = OsuFont.GetFont(size: axis_font_size, weight: FontWeight.SemiBold)
                    });

                    axisFlow.Add(new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativePositionAxes = Axes.X,
                        X = position / 2,
                        Alpha = alpha,
                        Text = axisValue.ToString("+0"),
                        Font = OsuFont.GetFont(size: axis_font_size, weight: FontWeight.SemiBold)
                    });
                }
            }
        }

        private class Bar : CompositeDrawable
        {
            private readonly float value;
            private readonly float maxValue;

            private readonly Circle boxOriginal;
            private Circle boxAdjustment;

            private const float minimum_height = 0.05f;

            public Bar(float value, float maxValue, bool isCentre)
            {
                this.value = value;
                this.maxValue = maxValue;

                RelativeSizeAxes = Axes.Both;
                Masking = true;

                InternalChildren = new Drawable[]
                {
                    boxOriginal = new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Colour = isCentre ? Color4.White : Color4Extensions.FromHex("#66FFCC"),
                        Height = minimum_height,
                    },
                };
            }

            private const double duration = 300;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                float height = Math.Clamp(value / maxValue, minimum_height, 1);

                if (height > minimum_height)
                    boxOriginal.ResizeHeightTo(height, duration, Easing.OutQuint);
            }

            public void UpdateOffset(float adjustment)
            {
                bool hasAdjustment = adjustment != value && adjustment / maxValue >= minimum_height;

                if (boxAdjustment == null)
                {
                    if (!hasAdjustment)
                        return;

                    AddInternal(boxAdjustment = new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Colour = Color4.Yellow,
                        Blending = BlendingParameters.Additive,
                        Alpha = 0.6f,
                        Height = 0,
                    });
                }

                boxAdjustment.ResizeHeightTo(Math.Clamp(adjustment / maxValue, minimum_height, 1), duration, Easing.OutQuint);
                boxAdjustment.FadeTo(!hasAdjustment ? 0 : 1, duration, Easing.OutQuint);
            }
        }
    }
}
