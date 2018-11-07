// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Lists;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;

namespace osu.Game.Rulesets.UI.Scrolling
{
    /// <summary>
    /// A type of <see cref="RulesetContainer{TPlayfield,TObject}"/> that supports a <see cref="ScrollingPlayfield"/>.
    /// <see cref="HitObject"/>s inside this <see cref="RulesetContainer{TPlayfield,TObject}"/> will scroll within the playfield.
    /// </summary>
    public abstract class ScrollingRulesetContainer<TPlayfield, TObject> : RulesetContainer<TPlayfield, TObject>
        where TObject : HitObject
        where TPlayfield : ScrollingPlayfield
    {
        protected readonly Bindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();

        protected virtual ScrollAlgorithm ScrollAlgorithm => ScrollAlgorithm.Sequential;

        /// <summary>
        /// Provides the default <see cref="MultiplierControlPoint"/>s that adjust the scrolling rate of <see cref="HitObject"/>s
        /// inside this <see cref="RulesetContainer{TPlayfield,TObject}"/>.
        /// </summary>
        /// <returns></returns>
        private readonly SortedList<MultiplierControlPoint> controlPoints = new SortedList<MultiplierControlPoint>(Comparer<MultiplierControlPoint>.Default);

        [Cached(Type = typeof(IScrollingInfo))]
        protected readonly IScrollingInfo ScrollingInfo;

        [Cached(Type = typeof(IScrollAlgorithm))]
        private readonly IScrollAlgorithm algorithm;

        protected ScrollingRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
            switch (ScrollAlgorithm)
            {
                case ScrollAlgorithm.Sequential:
                    algorithm = new SequentialScrollAlgorithm(controlPoints);
                    break;
                case ScrollAlgorithm.Overlapping:
                    algorithm = new OverlappingScrollAlgorithm(controlPoints);
                    break;
                case ScrollAlgorithm.Constant:
                    algorithm = new ConstantScrollAlgorithm();
                    break;
            }

            ScrollingInfo = CreateScrollingInfo();
            ScrollingInfo.Direction.BindTo(Direction);
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

        protected virtual IScrollingInfo CreateScrollingInfo() => new LocalScrollingInfo();

        private class LocalScrollingInfo : IScrollingInfo
        {
            public IBindable<ScrollingDirection> Direction { get; } = new Bindable<ScrollingDirection>();
        }
    }
}
