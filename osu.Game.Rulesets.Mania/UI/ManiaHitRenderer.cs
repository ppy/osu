// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Input;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Lists;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mania.Timing;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaHitRenderer : HitRenderer<ManiaHitObject, ManiaJudgement>
    {
        public int? Columns;

        public ManiaHitRenderer(WorkingBeatmap beatmap, bool isForCurrentRuleset)
            : base(beatmap, isForCurrentRuleset)
        {
        }

        protected override Playfield<ManiaHitObject, ManiaJudgement> CreatePlayfield()
        {
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

            return new ManiaPlayfield(Columns ?? (int)Math.Round(Beatmap.BeatmapInfo.Difficulty.CircleSize), timingChanges)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                // Invert by default for now (should be moved to config/skin later)
                Scale = new Vector2(1, -1)
            };
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
