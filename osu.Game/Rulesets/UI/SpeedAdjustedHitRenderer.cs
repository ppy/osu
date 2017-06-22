// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Lists;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// A type of <see cref="HitRenderer{TObject, TJudgement}"/> that supports speed adjustments in some capacity.
    /// </summary>
    public abstract class SpeedAdjustedHitRenderer<TObject, TJudgement> : HitRenderer<TObject, TJudgement>
        where TObject : HitObject
        where TJudgement : Judgement
    {
        protected readonly SortedList<MultiplierControlPoint> DefaultControlPoints = new SortedList<MultiplierControlPoint>(Comparer<MultiplierControlPoint>.Default);

        protected SpeedAdjustedHitRenderer(WorkingBeatmap beatmap, bool isForCurrentRuleset)
            : base(beatmap, isForCurrentRuleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ApplySpeedAdjustments();
        }

        protected override void ApplyBeatmap()
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
                .GroupBy(s => s.StartTime).Select(g => g.Last()).OrderBy(s => s.StartTime)
                // Collapse sections with the same beat length
                .GroupBy(s => s.TimingPoint.BeatLength * s.DifficultyPoint.SpeedMultiplier).Select(g => g.First());

            DefaultControlPoints.AddRange(timingChanges);
        }

        /// <summary>
        /// Generates a control point with the default timing change/difficulty change from the beatmap at a time.
        /// </summary>
        /// <param name="time">The time to create the control point at.</param>
        /// <returns>The <see cref="MultiplierControlPoint"/> at <paramref name="time"/>.</returns>
        public MultiplierControlPoint CreateControlPointAt(double time)
        {
            if (DefaultControlPoints.Count == 0)
                return new MultiplierControlPoint(time);

            int index = DefaultControlPoints.BinarySearch(new MultiplierControlPoint(time));
            if (index < 0)
                return new MultiplierControlPoint(time);

            return new MultiplierControlPoint(time, DefaultControlPoints[index].DeepClone());
        }

        /// <summary>
        /// Applies speed changes to the playfield.
        /// </summary>
        protected abstract void ApplySpeedAdjustments();
    }
}
