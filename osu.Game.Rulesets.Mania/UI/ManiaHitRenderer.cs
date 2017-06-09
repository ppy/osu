// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Lists;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mania.Timing;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaHitRenderer : HitRenderer<ManiaHitObject, ManiaJudgement>
    {
        /// <summary>
        /// Preferred column count. This will only have an effect during the initialization of the play field.
        /// </summary>
        public int PreferredColumns;

        public IEnumerable<DrawableBarLine> BarLines;

        /// <summary>
        /// Per-column timing changes.
        /// </summary>
        private readonly List<SpeedAdjustmentContainer>[] hitObjectTimingChanges;

        /// <summary>
        /// Bar line timing changes.
        /// </summary>
        private readonly List<SpeedAdjustmentContainer> barlineTimingChanges = new List<SpeedAdjustmentContainer>();

        private readonly SortedList<MultiplierControlPoint> defaultControlPoints = new SortedList<MultiplierControlPoint>(Comparer<MultiplierControlPoint>.Default);

        public ManiaHitRenderer(WorkingBeatmap beatmap, bool isForCurrentRuleset)
            : base(beatmap, isForCurrentRuleset)
        {
            // Generate the speed adjustment container lists
            hitObjectTimingChanges = new List<SpeedAdjustmentContainer>[PreferredColumns];
            for (int i = 0; i < PreferredColumns; i++)
                hitObjectTimingChanges[i] = new List<SpeedAdjustmentContainer>();

            // Generate the bar lines
            double lastObjectTime = (Objects.LastOrDefault() as IHasEndTime)?.EndTime ?? Objects.LastOrDefault()?.StartTime ?? double.MaxValue;

            SortedList<TimingControlPoint> timingPoints = Beatmap.ControlPointInfo.TimingPoints;
            var barLines = new List<DrawableBarLine>();

            for (int i = 0; i < timingPoints.Count; i++)
            {
                TimingControlPoint point = timingPoints[i];

                // Stop on the beat before the next timing point, or if there is no next timing point stop slightly past the last object
                double endTime = i < timingPoints.Count - 1 ? timingPoints[i + 1].Time - point.BeatLength : lastObjectTime + point.BeatLength * (int)point.TimeSignature;

                int index = 0;
                for (double t = timingPoints[i].Time; Precision.DefinitelyBigger(endTime, t); t += point.BeatLength, index++)
                {
                    barLines.Add(new DrawableBarLine(new BarLine
                    {
                        StartTime = t,
                        ControlPoint = point,
                        BeatIndex = index
                    }));
                }
            }

            BarLines = barLines;

            // Generate speed adjustments from mods first
            bool useDefaultSpeedAdjustments = true;

            if (Mods != null)
            {
                foreach (var speedAdjustmentMod in Mods.OfType<IGenerateSpeedAdjustments>())
                {
                    useDefaultSpeedAdjustments = false;
                    speedAdjustmentMod.ApplyToHitRenderer(this, ref hitObjectTimingChanges, ref barlineTimingChanges);
                }
            }

            // Generate the default speed adjustments
            if (useDefaultSpeedAdjustments)
                generateDefaultSpeedAdjustments();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var maniaPlayfield = Playfield as ManiaPlayfield;
            if (maniaPlayfield == null)
                return;

            BarLines.ForEach(maniaPlayfield.Add);
        }

        private void generateDefaultSpeedAdjustments()
        {
            defaultControlPoints.ForEach(c =>
            {
                foreach (List<SpeedAdjustmentContainer> t in hitObjectTimingChanges)
                    t.Add(new ManiaSpeedAdjustmentContainer(c, ScrollingAlgorithm.Basic));
                barlineTimingChanges.Add(new ManiaSpeedAdjustmentContainer(c, ScrollingAlgorithm.Basic));
            });
        }

        /// <summary>
        /// Generates a control point at a point in time with the relevant timing change/difficulty change from the beatmap.
        /// </summary>
        /// <param name="time">The time to create the control point at.</param>
        /// <returns>The <see cref="MultiplierControlPoint"/> at <paramref name="time"/>.</returns>
        public MultiplierControlPoint CreateControlPointAt(double time)
        {
            if (defaultControlPoints.Count == 0)
                return new MultiplierControlPoint(time);

            int index = defaultControlPoints.BinarySearch(new MultiplierControlPoint(time));
            if (index < 0)
                return new MultiplierControlPoint(time);

            return new MultiplierControlPoint(time, defaultControlPoints[index].DeepClone());
        }

        protected override void ApplyBeatmap()
        {
            base.ApplyBeatmap();

            PreferredColumns = (int)Math.Round(Beatmap.BeatmapInfo.Difficulty.CircleSize);

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
                .GroupBy(s => s.TimingPoint.BeatLength * s.DifficultyPoint.SpeedMultiplier).Select(g => g.First())
                .ToList();

            defaultControlPoints.AddRange(timingChanges);
        }

        protected override Playfield<ManiaHitObject, ManiaJudgement> CreatePlayfield()
        {
            var playfield = new ManiaPlayfield(PreferredColumns)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                // Invert by default for now (should be moved to config/skin later)
                Scale = new Vector2(1, -1)
            };

            for (int i = 0; i < PreferredColumns; i++)
            {
                foreach (var change in hitObjectTimingChanges[i])
                    playfield.Columns.ElementAt(i).Add(change);
            }

            foreach (var change in barlineTimingChanges)
                playfield.Add(change);

            return playfield;
        }

        public override ScoreProcessor CreateScoreProcessor() => new ManiaScoreProcessor(this);

        protected override BeatmapConverter<ManiaHitObject> CreateBeatmapConverter() => new ManiaBeatmapConverter();

        protected override DrawableHitObject<ManiaHitObject, ManiaJudgement> GetVisualRepresentation(ManiaHitObject h)
        {
            var maniaPlayfield = Playfield as ManiaPlayfield;
            if (maniaPlayfield == null)
                return null;

            Bindable<Key> key = maniaPlayfield.Columns.ElementAt(h.Column).Key;

            var holdNote = h as HoldNote;
            if (holdNote != null)
                return new DrawableHoldNote(holdNote, key);

            var note = h as Note;
            if (note != null)
                return new DrawableNote(note, key);

            return null;
        }

        protected override Vector2 GetPlayfieldAspectAdjust() => new Vector2(1, 0.8f);
    }
}
