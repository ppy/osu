// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
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

        public ManiaHitRenderer(WorkingBeatmap beatmap)
            : base(beatmap)
        {
        }

        protected override Playfield<ManiaHitObject, ManiaJudgement> CreatePlayfield()
        {
            List<TimingSection> timingSections = new List<TimingSection>();

            // Construct all the relevant timing sections
            ControlPoint lastTimingChange = null;
            foreach (ControlPoint point in Beatmap.TimingInfo.ControlPoints)
            {
                if (point.TimingChange)
                    lastTimingChange = point;

                timingSections.Add(new TimingSection
                {
                    StartTime = point.Time,
                    // Todo: Should this be dividing by beatlength?
                    BeatLength = point.SpeedMultiplier * lastTimingChange.BeatLength,
                    TimeSignature = point.TimeSignature
                });
            }

            double lastObjectTime = (Objects.Last() as IHasEndTime)?.EndTime ?? Objects.Last().StartTime;

            // Perform some post processing of the timing sections
            timingSections = timingSections
                // Collapse sections after the last hit object
                .Where(s => s.StartTime <= lastObjectTime)
                // Collapse sections with the same start time
                .GroupBy(s => s.StartTime).Select(g => g.Last()).OrderBy(s => s.StartTime)
                // Collapse sections with the same beat length
                .GroupBy(s => s.BeatLength).Select(g => g.First())
                .ToList();

            // Determine duration of timing sections
            for (int i = 0; i < timingSections.Count; i++)
            {
                if (i < timingSections.Count - 1)
                    timingSections[i].Duration = timingSections[i + 1].StartTime - timingSections[i].StartTime;
                else
                {
                    // Extra length added for the last timing section to extend past the last hitobject
                    double extraLength = timingSections[i].BeatLength * (int)timingSections[i].TimeSignature;
                    timingSections[i].Duration = lastObjectTime + extraLength - timingSections[i].StartTime;
                }
            }

            return new ManiaPlayfield(Columns ?? (int)Math.Round(Beatmap.BeatmapInfo.Difficulty.CircleSize), timingSections);
        }

        public override ScoreProcessor CreateScoreProcessor() => new ManiaScoreProcessor(this);

        protected override BeatmapConverter<ManiaHitObject> CreateBeatmapConverter() => new ManiaBeatmapConverter();

        protected override DrawableHitObject<ManiaHitObject, ManiaJudgement> GetVisualRepresentation(ManiaHitObject h)
        {
            var note = h as Note;
            if (note != null)
                return new DrawableNote(note);

            var holdNote = h as HoldNote;
            if (holdNote != null)
                return new DrawableHoldNote(holdNote);

            return null;
        }
    }
}
