// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Lists;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;

namespace osu.Game.Rulesets.UI.Scrolling
{
    /// <summary>
    /// A type of <see cref="DrawableRuleset{TObject}"/> that supports a <see cref="ScrollingPlayfield"/>.
    /// <see cref="HitObject"/>s inside this <see cref="DrawableRuleset{TObject}"/> will scroll within the playfield.
    /// </summary>
    public abstract partial class DrawableScrollingRuleset<TObject> : DrawableRuleset<TObject>, IDrawableScrollingRuleset, IKeyBindingHandler<GlobalAction>
        where TObject : HitObject
    {
        /// <summary>
        /// The default span of time visible by the length of the scrolling axes.
        /// This is clamped between <see cref="time_span_min"/> and <see cref="time_span_max"/>.
        /// </summary>
        private const double time_span_default = 1500;

        /// <summary>
        /// The minimum span of time that may be visible by the length of the scrolling axes.
        /// </summary>
        private const double time_span_min = 50;

        /// <summary>
        /// The maximum span of time that may be visible by the length of the scrolling axes.
        /// </summary>
        private const double time_span_max = 20000;

        /// <summary>
        /// The step increase/decrease of the span of time visible by the length of the scrolling axes.
        /// </summary>
        private const double time_span_step = 200;

        protected readonly Bindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();

        /// <summary>
        /// The span of time that is visible by the length of the scrolling axes.
        /// For example, only hit objects with start time less than or equal to 1000 will be visible with <see cref="TimeRange"/> = 1000.
        /// </summary>
        protected readonly BindableDouble TimeRange = new BindableDouble(time_span_default)
        {
            MinValue = time_span_min,
            MaxValue = time_span_max
        };

        ScrollVisualisationMethod IDrawableScrollingRuleset.VisualisationMethod => VisualisationMethod;

        /// <summary>
        /// Whether the player can change <see cref="TimeRange"/>.
        /// </summary>
        protected virtual bool UserScrollSpeedAdjustment => true;

        /// <summary>
        /// Whether <see cref="TimingControlPoint"/> beat lengths should scale relative to the most common beat length in the <see cref="Beatmap"/>.
        /// </summary>
        protected virtual bool RelativeScaleBeatLengths => false;

        /// <summary>
        /// The <see cref="MultiplierControlPoint"/>s that adjust the scrolling rate of <see cref="HitObject"/>s inside this <see cref="DrawableRuleset{TObject}"/>.
        /// </summary>
        protected readonly SortedList<MultiplierControlPoint> ControlPoints = new SortedList<MultiplierControlPoint>(Comparer<MultiplierControlPoint>.Default);

        public IScrollingInfo ScrollingInfo => scrollingInfo;

        [Cached(Type = typeof(IScrollingInfo))]
        private readonly LocalScrollingInfo scrollingInfo;

        protected DrawableScrollingRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            : base(ruleset, beatmap, mods)
        {
            scrollingInfo = new LocalScrollingInfo();
            scrollingInfo.Direction.BindTo(Direction);
            scrollingInfo.TimeRange.BindTo(TimeRange);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            updateScrollAlgorithm();

            double lastObjectTime = Beatmap.HitObjects.Any() ? Beatmap.GetLastObjectTime() : double.MaxValue;
            double baseBeatLength = TimingControlPoint.DEFAULT_BEAT_LENGTH;

            if (RelativeScaleBeatLengths)
            {
                baseBeatLength = Beatmap.GetMostCommonBeatLength();

                // The slider multiplier is post-multiplied to determine the final velocity, but for relative scale beat lengths
                // the multiplier should not affect the effective timing point (the longest in the beatmap), so it is factored out here
                baseBeatLength /= Beatmap.Difficulty.SliderMultiplier;
            }

            // Merge sequences of timing and difficulty control points to create the aggregate "multiplier" control point
            var lastTimingPoint = new TimingControlPoint();
            var lastEffectPoint = new EffectControlPoint();
            var allPoints = new SortedList<ControlPoint>(Comparer<ControlPoint>.Default);

            allPoints.AddRange(Beatmap.ControlPointInfo.TimingPoints);
            allPoints.AddRange(Beatmap.ControlPointInfo.EffectPoints);

            // Generate the timing points, making non-timing changes use the previous timing change and vice-versa
            var timingChanges = allPoints.Select(c =>
            {
                switch (c)
                {
                    case TimingControlPoint timingPoint:
                        lastTimingPoint = timingPoint;
                        break;

                    case EffectControlPoint difficultyPoint:
                        lastEffectPoint = difficultyPoint;
                        break;
                }

                return new MultiplierControlPoint(c.Time)
                {
                    Velocity = Beatmap.Difficulty.SliderMultiplier,
                    BaseBeatLength = baseBeatLength,
                    TimingPoint = lastTimingPoint,
                    EffectPoint = lastEffectPoint
                };
            });

            // Trim unwanted sequences of timing changes
            timingChanges = timingChanges
                            // Collapse sections after the last hit object
                            .Where(s => s.Time <= lastObjectTime)
                            // Collapse sections with the same start time
                            .GroupBy(s => s.Time).Select(g => g.Last()).OrderBy(s => s.Time);

            ControlPoints.AddRange(timingChanges);

            if (ControlPoints.Count == 0)
                ControlPoints.Add(new MultiplierControlPoint { Velocity = Beatmap.Difficulty.SliderMultiplier });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (!(Playfield is ScrollingPlayfield))
                throw new ArgumentException($"{nameof(Playfield)} must be a {nameof(ScrollingPlayfield)} when using {nameof(DrawableScrollingRuleset<TObject>)}.");
        }

        private ScrollVisualisationMethod visualisationMethod = ScrollVisualisationMethod.Sequential;

        public ScrollVisualisationMethod VisualisationMethod
        {
            get => visualisationMethod;
            set
            {
                visualisationMethod = value;
                updateScrollAlgorithm();
            }
        }

        private void updateScrollAlgorithm()
        {
            switch (VisualisationMethod)
            {
                case ScrollVisualisationMethod.Sequential:
                    scrollingInfo.Algorithm.Value = new SequentialScrollAlgorithm(ControlPoints);
                    break;

                case ScrollVisualisationMethod.Overlapping:
                    scrollingInfo.Algorithm.Value = new OverlappingScrollAlgorithm(ControlPoints);
                    break;

                case ScrollVisualisationMethod.Constant:
                    scrollingInfo.Algorithm.Value = new ConstantScrollAlgorithm();
                    break;
            }
        }

        /// <summary>
        /// Adjusts the scroll speed of <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="amount">The amount to adjust by. Greater than 0 if the scroll speed should be increased, less than 0 if it should be decreased.</param>
        protected virtual void AdjustScrollSpeed(int amount) => this.TransformBindableTo(TimeRange, TimeRange.Value - amount * time_span_step, 200, Easing.OutQuint);

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (!UserScrollSpeedAdjustment)
                return false;

            switch (e.Action)
            {
                case GlobalAction.IncreaseScrollSpeed:
                    AdjustScrollSpeed(1);
                    return true;

                case GlobalAction.DecreaseScrollSpeed:
                    AdjustScrollSpeed(-1);
                    return true;
            }

            return false;
        }

        private ScheduledDelegate scheduledScrollSpeedAdjustment;

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            scheduledScrollSpeedAdjustment?.Cancel();
            scheduledScrollSpeedAdjustment = null;
        }

        private class LocalScrollingInfo : IScrollingInfo
        {
            public IBindable<ScrollingDirection> Direction { get; } = new Bindable<ScrollingDirection>();

            public IBindable<double> TimeRange { get; } = new BindableDouble();

            public readonly Bindable<IScrollAlgorithm> Algorithm = new Bindable<IScrollAlgorithm>(new ConstantScrollAlgorithm());

            IBindable<IScrollAlgorithm> IScrollingInfo.Algorithm => Algorithm;
        }
    }
}
