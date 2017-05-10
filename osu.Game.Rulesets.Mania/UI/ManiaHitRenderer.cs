// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Timing;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaHitRenderer : HitRenderer<ManiaHitObject, ManiaJudgement>
    {
        private readonly int columns;

        public ManiaHitRenderer(WorkingBeatmap beatmap, int columns = 5)
            : base(beatmap)
        {
            this.columns = columns;
            // Has to be done before drawable hit objects are generated in load()
            loadTimingSections();
        }

        private void loadTimingSections()
        {
            var maniaPlayfield = Playfield as ManiaPlayfield;
            if (maniaPlayfield == null)
                return;

            var sections = new List<TimingSection>();

            // Construct all the relevant timing sections
            ControlPoint lastTimingChange = null;
            foreach (ControlPoint point in Beatmap.TimingInfo.ControlPoints)
            {
                if (point.TimingChange)
                    lastTimingChange = point;

                sections.Add(new TimingSection
                {
                    StartTime = point.Time,
                    // Todo: Should this be dividing by beatlength?
                    BeatLength = point.SpeedMultiplier * lastTimingChange.BeatLength,
                    TimeSignature = point.TimeSignature
                });
            }

            double lastObjectTime = (Objects.Last() as IHasEndTime)?.EndTime ?? Objects.Last().StartTime;

            // Perform some post processing of the timing sections
            sections = sections
                // Collapse sections after the last hit object
                .Where(s => s.StartTime <= lastObjectTime)
                // Collapse sections with the same start time
                .GroupBy(s => s.StartTime).Select(g => g.Last()).OrderBy(s => s.StartTime)
                // Collapse sections with the same beat length
                .GroupBy(s => s.BeatLength).Select(g => g.First())
                .ToList();

            // Determine duration of timing sections
            for (int i = 0; i < sections.Count; i++)
            {
                if (i < sections.Count - 1)
                    sections[i].Duration = sections[i + 1].StartTime - sections[i].StartTime;
                else
                {
                    // Extra length added for the last timing section to extend past the last hitobject
                    double extraLength = sections[i].BeatLength * (int)sections[i].TimeSignature;
                    sections[i].Duration = lastObjectTime + extraLength - sections[i].StartTime;
                }
            }

            sections.ForEach(s => maniaPlayfield.Columns.Children.ForEach(c => c.AddTimingSection(s)));
        }

        public override ScoreProcessor CreateScoreProcessor() => new ManiaScoreProcessor(this);

        protected override BeatmapConverter<ManiaHitObject> CreateBeatmapConverter() => new ManiaBeatmapConverter();

        protected override Playfield<ManiaHitObject, ManiaJudgement> CreatePlayfield() => new ManiaPlayfield(columns);

        protected override DrawableHitObject<ManiaHitObject, ManiaJudgement> GetVisualRepresentation(ManiaHitObject h) => null;
    }
}
