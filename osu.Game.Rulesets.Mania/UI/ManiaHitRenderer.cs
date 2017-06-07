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
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mania.Timing.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.Timing.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaHitRenderer : HitRenderer<ManiaHitObject, ManiaJudgement>
    {
        /// <summary>
        /// Preferred column count. This will only have an effect during the initialization of the play field.
        /// </summary>
        public int PreferredColumns;

        /// <summary>
        /// Per-column timing changes.
        /// </summary>
        public List<DrawableTimingChange>[] HitObjectTimingChanges;

        /// <summary>
        /// Bar line timing changes.
        /// </summary>
        public List<DrawableTimingChange> BarlineTimingChanges;

        /// <summary>
        /// Number of columns in the playfield of this hit renderer. Null if the play field hasn't been generated yet.
        /// </summary>
        public int? Columns { get; private set; }

        public ManiaHitRenderer(WorkingBeatmap beatmap, bool isForCurrentRuleset)
            : base(beatmap, isForCurrentRuleset)
        {
            Columns = PreferredColumns;

            generateDefaultTimingChanges();
        }

        private void generateDefaultTimingChanges()
        {
            if (HitObjectTimingChanges != null || BarlineTimingChanges != null)
                return;

            HitObjectTimingChanges = new List<DrawableTimingChange>[PreferredColumns];
            BarlineTimingChanges = new List<DrawableTimingChange>();

            for (int i = 0; i < PreferredColumns; i++)
                HitObjectTimingChanges[i] = new List<DrawableTimingChange>();

            double lastSpeedMultiplier = 1;
            double lastBeatLength = 500;

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
                    lastBeatLength = timingPoint.BeatLength;

                if (difficultyPoint != null)
                    lastSpeedMultiplier = difficultyPoint.SpeedMultiplier;

                return new TimingChange
                {
                    Time = c.Time,
                    BeatLength = lastBeatLength,
                    SpeedMultiplier = lastSpeedMultiplier
                };
            });

            double lastObjectTime = (Objects.LastOrDefault() as IHasEndTime)?.EndTime ?? Objects.LastOrDefault()?.StartTime ?? double.MaxValue;

            // Perform some post processing of the timing changes
            timingChanges = timingChanges
                // Collapse sections after the last hit object
                .Where(s => s.Time <= lastObjectTime)
                // Collapse sections with the same start time
                .GroupBy(s => s.Time).Select(g => g.Last()).OrderBy(s => s.Time)
                // Collapse sections with the same beat length
                .GroupBy(s => s.BeatLength * s.SpeedMultiplier).Select(g => g.First())
                .ToList();

            timingChanges.ForEach(t =>
            {
                for (int i = 0; i < PreferredColumns; i++)
                    HitObjectTimingChanges[i].Add(new DrawableManiaScrollingTimingChange(t));

                BarlineTimingChanges.Add(new DrawableManiaScrollingTimingChange(t));
            });
        }

        protected override void ApplyBeatmap()
        {
            base.ApplyBeatmap();
            PreferredColumns = (int)Math.Round(Beatmap.BeatmapInfo.Difficulty.CircleSize);
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
                foreach (var change in HitObjectTimingChanges[i])
                    playfield.Columns.ElementAt(i).Add(change);
            }

            foreach (var change in BarlineTimingChanges)
                playfield.Add(change);

            return playfield;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var maniaPlayfield = (ManiaPlayfield)Playfield;

            double lastObjectTime = (Objects.LastOrDefault() as IHasEndTime)?.EndTime ?? Objects.LastOrDefault()?.StartTime ?? double.MaxValue;

            SortedList<TimingControlPoint> timingPoints = Beatmap.ControlPointInfo.TimingPoints;
            for (int i = 0; i < timingPoints.Count; i++)
            {
                TimingControlPoint point = timingPoints[i];

                // Stop on the beat before the next timing point, or if there is no next timing point stop slightly past the last object
                double endTime = i < timingPoints.Count - 1 ? timingPoints[i + 1].Time - point.BeatLength : lastObjectTime + point.BeatLength * (int)point.TimeSignature;

                int index = 0;
                for (double t = timingPoints[i].Time; Precision.DefinitelyBigger(endTime, t); t += point.BeatLength, index++)
                {
                    maniaPlayfield.Add(new DrawableBarLine(new BarLine
                    {
                        StartTime = t,
                        ControlPoint = point,
                        BeatIndex = index
                    }));
                }
            }
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
