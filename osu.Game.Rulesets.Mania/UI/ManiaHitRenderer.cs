// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using OpenTK;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Scoring;
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
            ControlPoint firstTimingChange = Beatmap.TimingInfo.ControlPoints.FirstOrDefault(t => t.TimingChange);

            if (firstTimingChange == null)
                throw new Exception("The Beatmap contains no timing points!");

            // Generate the timing points, making non-timing changes use the previous timing change
            var timingChanges = Beatmap.TimingInfo.ControlPoints.Select(c =>
            {
                ControlPoint t = c.Clone();

                if (c.TimingChange)
                    firstTimingChange = c;
                else
                    t.BeatLength = firstTimingChange.BeatLength;

                return t;
            });

            double lastObjectTime = (Objects.Last() as IHasEndTime)?.EndTime ?? Objects.Last().StartTime;

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
            var note = h as Note;
            if (note != null)
                return new DrawableNote(note);

            return null;
        }

        protected override Vector2 GetPlayfieldAspectAdjust() => new Vector2(1, 0.8f);
    }
}
