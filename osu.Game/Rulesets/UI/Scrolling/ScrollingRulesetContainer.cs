// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Lists;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Timing;

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
        /// <summary>
        /// Provides the default <see cref="MultiplierControlPoint"/>s that adjust the scrolling rate of <see cref="HitObject"/>s
        /// inside this <see cref="RulesetContainer{TPlayfield,TObject}"/>.
        /// </summary>
        /// <returns></returns>
        protected readonly SortedList<MultiplierControlPoint> DefaultControlPoints = new SortedList<MultiplierControlPoint>(Comparer<MultiplierControlPoint>.Default);

        protected ScrollingRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
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

            DefaultControlPoints.AddRange(timingChanges);

            // If we have no control points, add a default one
            if (DefaultControlPoints.Count == 0)
                DefaultControlPoints.Add(new MultiplierControlPoint());

            DefaultControlPoints.ForEach(c => applySpeedAdjustment(c, Playfield));
        }

        private void applySpeedAdjustment(MultiplierControlPoint controlPoint, ScrollingPlayfield playfield)
        {
            playfield.HitObjects.AddControlPoint(controlPoint);
            playfield.NestedPlayfields?.OfType<ScrollingPlayfield>().ForEach(p => applySpeedAdjustment(controlPoint, p));
        }

        /// <summary>
        /// Generates a <see cref="MultiplierControlPoint"/> with the default timing change/difficulty change from the beatmap at a time.
        /// </summary>
        /// <param name="time">The time to create the control point at.</param>
        /// <returns>The default <see cref="MultiplierControlPoint"/> at <paramref name="time"/>.</returns>
        public MultiplierControlPoint CreateControlPointAt(double time)
        {
            if (DefaultControlPoints.Count == 0)
                return new MultiplierControlPoint(time);

            int index = DefaultControlPoints.BinarySearch(new MultiplierControlPoint(time));
            if (index < 0)
                return new MultiplierControlPoint(time);

            return new MultiplierControlPoint(time, DefaultControlPoints[index].DeepClone());
        }
    }
}
