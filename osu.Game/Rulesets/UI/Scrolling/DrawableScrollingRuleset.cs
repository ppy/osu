// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Lists;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;

namespace osu.Game.Rulesets.UI.Scrolling
{
    /// <summary>
    /// A type of <see cref="DrawableRuleset{TObject}"/> that supports a <see cref="ScrollingPlayfield"/>.
    /// <see cref="HitObject"/>s inside this <see cref="DrawableRuleset{TObject}"/> will scroll within the playfield.
    /// </summary>
    public abstract class DrawableScrollingRuleset<TObject> : DrawableRuleset<TObject>, IKeyBindingHandler<GlobalAction>
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
        private const double time_span_max = 10000;

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
            Default = time_span_default,
            MinValue = time_span_min,
            MaxValue = time_span_max
        };

        protected virtual ScrollVisualisationMethod VisualisationMethod => ScrollVisualisationMethod.Sequential;

        /// <summary>
        /// Whether the player can change <see cref="VisibleTimeRange"/>.
        /// </summary>
        protected virtual bool UserScrollSpeedAdjustment => true;

        /// <summary>
        /// Provides the default <see cref="MultiplierControlPoint"/>s that adjust the scrolling rate of <see cref="HitObject"/>s
        /// inside this <see cref="DrawableRuleset{TObject}"/>.
        /// </summary>
        /// <returns></returns>
        private readonly SortedList<MultiplierControlPoint> controlPoints = new SortedList<MultiplierControlPoint>(Comparer<MultiplierControlPoint>.Default);

        protected IScrollingInfo ScrollingInfo => scrollingInfo;

        [Cached(Type = typeof(IScrollingInfo))]
        private readonly LocalScrollingInfo scrollingInfo;

        protected DrawableScrollingRuleset(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
            scrollingInfo = new LocalScrollingInfo();
            scrollingInfo.Direction.BindTo(Direction);
            scrollingInfo.TimeRange.BindTo(TimeRange);

            switch (VisualisationMethod)
            {
                case ScrollVisualisationMethod.Sequential:
                    scrollingInfo.Algorithm = new SequentialScrollAlgorithm(controlPoints);
                    break;
                case ScrollVisualisationMethod.Overlapping:
                    scrollingInfo.Algorithm = new OverlappingScrollAlgorithm(controlPoints);
                    break;
                case ScrollVisualisationMethod.Constant:
                    scrollingInfo.Algorithm = new ConstantScrollAlgorithm();
                    break;
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // Calculate default multiplier control points
            var lastTimingPoint = new TimingControlPoint();
            var lastDifficultyPoint = new DifficultyControlPoint();

            // Merge timing + difficulty points
            var allPoints = new SortedList<ControlPoint>(Comparer<ControlPoint>.Default);
            allPoints.AddRange(Beatmap.ControlPointInfo.TimingPoints);
            allPoints.AddRange(Beatmap.ControlPointInfo.DifficultyPoints);

            // Generate the timing points, making non-timing changes use the previous timing change
            var timingChanges = allPoints.Select(c =>
            {
                var timingPoint = c as TimingControlPoint;
                var difficultyPoint = c as DifficultyControlPoint;

                if (timingPoint != null)
                    lastTimingPoint = timingPoint;

                if (difficultyPoint != null)
                    lastDifficultyPoint = difficultyPoint;

                return new MultiplierControlPoint(c.Time)
                {
                    Velocity = Beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier,
                    TimingPoint = lastTimingPoint,
                    DifficultyPoint = lastDifficultyPoint
                };
            });

            double lastObjectTime = (Objects.LastOrDefault() as IHasEndTime)?.EndTime ?? Objects.LastOrDefault()?.StartTime ?? double.MaxValue;

            // Perform some post processing of the timing changes
            timingChanges = timingChanges
                            // Collapse sections after the last hit object
                            .Where(s => s.StartTime <= lastObjectTime)
                            // Collapse sections with the same start time
                            .GroupBy(s => s.StartTime).Select(g => g.Last()).OrderBy(s => s.StartTime);

            controlPoints.AddRange(timingChanges);

            // If we have no control points, add a default one
            if (controlPoints.Count == 0)
                controlPoints.Add(new MultiplierControlPoint { Velocity = Beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier });
        }

        public bool OnPressed(GlobalAction action)
        {
            if (!UserScrollSpeedAdjustment)
                return false;

            switch (action)
            {
                case GlobalAction.IncreaseScrollSpeed:
                    this.TransformBindableTo(TimeRange, TimeRange.Value - time_span_step, 200, Easing.OutQuint);
                    return true;
                case GlobalAction.DecreaseScrollSpeed:
                    this.TransformBindableTo(TimeRange, TimeRange.Value + time_span_step, 200, Easing.OutQuint);
                    return true;
            }

            return false;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (!(Playfield is ScrollingPlayfield))
                throw new ArgumentException($"{nameof(Playfield)} must be a {nameof(ScrollingPlayfield)} when using {nameof(DrawableScrollingRuleset<TObject>)}.");
        }

        public bool OnReleased(GlobalAction action) => false;

        private class LocalScrollingInfo : IScrollingInfo
        {
            public IBindable<ScrollingDirection> Direction { get; } = new Bindable<ScrollingDirection>();

            public IBindable<double> TimeRange { get; } = new BindableDouble();

            public IScrollAlgorithm Algorithm { get; set; }
        }
    }
}
