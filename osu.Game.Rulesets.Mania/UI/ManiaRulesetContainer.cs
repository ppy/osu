﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mania.Timing;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaRulesetContainer : ScrollingRulesetContainer<ManiaPlayfield, ManiaHitObject>
    {
        public new ManiaBeatmap Beatmap => (ManiaBeatmap)base.Beatmap;

        public IEnumerable<DrawableBarLine> BarLines;

        public ManiaRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap, bool isForCurrentRuleset)
            : base(ruleset, beatmap, isForCurrentRuleset)
        {
            // Generate the bar lines
            double lastObjectTime = (Objects.LastOrDefault() as IHasEndTime)?.EndTime ?? Objects.LastOrDefault()?.StartTime ?? double.MaxValue;

            var timingPoints = Beatmap.ControlPointInfo.TimingPoints;
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
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            BarLines.ForEach(Playfield.Add);
        }

        protected sealed override Playfield CreatePlayfield() => new ManiaPlayfield(Beatmap.TotalColumns)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        };

        public override ScoreProcessor CreateScoreProcessor() => new ManiaScoreProcessor(this);

        public override PassThroughInputManager CreateInputManager() => new ManiaInputManager(Ruleset.RulesetInfo, Beatmap.TotalColumns);

        protected override BeatmapConverter<ManiaHitObject> CreateBeatmapConverter() => new ManiaBeatmapConverter(IsForCurrentRuleset, WorkingBeatmap.Beatmap);

        protected override DrawableHitObject<ManiaHitObject> GetVisualRepresentation(ManiaHitObject h)
        {
            ManiaAction action = Playfield.Columns.ElementAt(h.Column).Action;

            var holdNote = h as HoldNote;
            if (holdNote != null)
                return new DrawableHoldNote(holdNote, action);

            var note = h as Note;
            if (note != null)
                return new DrawableNote(note, action);

            return null;
        }

        protected override Vector2 GetPlayfieldAspectAdjust() => new Vector2(1, 0.8f);

        protected override SpeedAdjustmentContainer CreateSpeedAdjustmentContainer(MultiplierControlPoint controlPoint) => new ManiaSpeedAdjustmentContainer(controlPoint, ScrollingAlgorithm.Basic);

        protected override FramedReplayInputHandler CreateReplayInputHandler(Replay replay) => new ManiaFramedReplayInputHandler(replay, this);
    }
}
