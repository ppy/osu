// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Layout;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Screens.Ranking.Statistics
{
    /// <summary>
    /// A graph which displays the distribution of hit timing in a series of <see cref="HitEvent"/>s.
    /// </summary>
    public partial class HitEventTimingDistributionGraph : CompositeDrawable
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

        private readonly IDictionary<HitResult, int>[] bins;
        private double binSize;
        private double hitOffset;

        private Bar[]? barDrawables;

        /// <summary>
        /// Creates a new <see cref="HitEventTimingDistributionGraph"/>.
        /// </summary>
        /// <param name="hitEvents">The <see cref="HitEvent"/>s to display the timing distribution of.</param>
        public HitEventTimingDistributionGraph(IReadOnlyList<HitEvent> hitEvents)
        {
            this.hitEvents = hitEvents.Where(e => !(e.HitObject.HitWindows is HitWindows.EmptyHitWindows) && e.Result.IsBasic() && e.Result.IsHit()).ToList();
            bins = Enumerable.Range(0, total_timing_distribution_bins).Select(_ => new Dictionary<HitResult, int>()).ToArray<IDictionary<HitResult, int>>();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (hitEvents.Count == 0)
                return;

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

            foreach (var bin in bins)
                bin.Clear();

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
                {
                    bins[index].TryGetValue(e.Result, out int value);
                    bins[index][e.Result] = ++value;
                }
            }

            if (barDrawables == null)
                createBarDrawables();
            else
            {
                for (int i = 0; i < barDrawables.Length; i++)
                    barDrawables[i].UpdateOffset(bins[i].Sum(b => b.Value));
            }
        }

        private void createBarDrawables()
        {
            int maxCount = bins.Max(b => b.Values.Sum());
            barDrawables = bins.Select((_, i) => new Bar(bins[i], maxCount, i == timing_distribution_centre_bin_index)).ToArray();

            Container axisFlow;

            Padding = new MarginPadding { Horizontal = 5 };

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
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
                            Height = StatisticItem.FONT_SIZE,
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
                Font = OsuFont.GetFont(size: StatisticItem.FONT_SIZE, weight: FontWeight.SemiBold)
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
                    Font = OsuFont.GetFont(size: StatisticItem.FONT_SIZE, weight: FontWeight.SemiBold)
                });

                axisFlow.Add(new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.X,
                    X = position / 2,
                    Alpha = alpha,
                    Text = axisValue.ToString("+0"),
                    Font = OsuFont.GetFont(size: StatisticItem.FONT_SIZE, weight: FontWeight.SemiBold)
                });
            }
        }

        private partial class Bar : CompositeDrawable
        {
            private readonly IReadOnlyList<KeyValuePair<HitResult, int>> values;
            private readonly float maxValue;
            private readonly bool isCentre;
            private readonly float totalValue;

            private const float minimum_height = 0.02f;

            private float offsetAdjustment;

            private Circle[] boxOriginals = null!;

            private Circle? boxAdjustment;

            private float? lastDrawHeight;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            private const double duration = 300;

            public Bar(IDictionary<HitResult, int> values, float maxValue, bool isCentre)
            {
                this.values = values.OrderBy(v => v.Key.GetIndexForOrderedDisplay()).ToList();
                this.maxValue = maxValue;
                this.isCentre = isCentre;
                totalValue = values.Sum(v => v.Value);
                offsetAdjustment = totalValue;

                RelativeSizeAxes = Axes.Both;
                Masking = true;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                if (values.Any())
                {
                    boxOriginals = values.Select((v, i) => new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Colour = isCentre && i == 0 ? Color4.White : colours.ForHitResult(v.Key),
                        Height = 0,
                    }).ToArray();
                    // The bars of the stacked bar graph will be processed (stacked) from the bottom, which is the base position,
                    // to the top, and the bottom bar should be drawn more toward the front by design,
                    // while the drawing order is from the back to the front, so the order passed to `InternalChildren` is the opposite.
                    InternalChildren = boxOriginals.Reverse().ToArray();
                }
                else
                {
                    // A bin with no value draws a grey dot instead.
                    InternalChildren = boxOriginals = new[]
                    {
                        new Circle
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Colour = isCentre ? Color4.White : Color4.Gray,
                            Height = 0,
                        }
                    };
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Scheduler.AddOnce(updateMetrics, true);
            }

            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                if (invalidation.HasFlagFast(Invalidation.DrawSize))
                {
                    if (lastDrawHeight != null && lastDrawHeight != DrawHeight)
                        Scheduler.AddOnce(updateMetrics, false);
                }

                return base.OnInvalidate(invalidation, source);
            }

            public void UpdateOffset(float adjustment)
            {
                bool hasAdjustment = adjustment != totalValue;

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

                offsetAdjustment = adjustment;

                Scheduler.AddOnce(updateMetrics, true);
            }

            private void updateMetrics(bool animate = true)
            {
                float offsetValue = 0;

                for (int i = 0; i < boxOriginals.Length; i++)
                {
                    int value = i < values.Count ? values[i].Value : 0;

                    var box = boxOriginals[i];

                    box.MoveToY(offsetForValue(offsetValue) * BoundingBox.Height, duration, Easing.OutQuint);
                    box.ResizeHeightTo(heightForValue(value), duration, Easing.OutQuint);
                    offsetValue -= value;
                }

                if (boxAdjustment != null)
                    drawAdjustmentBar();

                if (!animate)
                    FinishTransforms(true);

                lastDrawHeight = DrawHeight;
            }

            private void drawAdjustmentBar()
            {
                bool hasAdjustment = offsetAdjustment != totalValue;

                boxAdjustment.ResizeHeightTo(heightForValue(offsetAdjustment), duration, Easing.OutQuint);
                boxAdjustment.FadeTo(!hasAdjustment ? 0 : 1, duration, Easing.OutQuint);
            }

            private float offsetForValue(float value) => (1 - minimum_height) * value / maxValue;

            private float heightForValue(float value) => minimum_height + offsetForValue(value);
        }
    }
}
