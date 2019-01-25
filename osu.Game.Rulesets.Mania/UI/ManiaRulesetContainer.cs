﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaRulesetContainer : ScrollingRulesetContainer<ManiaPlayfield, ManiaHitObject>
    {
        public new ManiaBeatmap Beatmap => (ManiaBeatmap)base.Beatmap;

        public IEnumerable<BarLine> BarLines;

        protected new ManiaConfigManager Config => (ManiaConfigManager)base.Config;

        private readonly Bindable<ManiaScrollingDirection> configDirection = new Bindable<ManiaScrollingDirection>();

        public ManiaRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
            // Generate the bar lines
            double lastObjectTime = (Objects.LastOrDefault() as IHasEndTime)?.EndTime ?? Objects.LastOrDefault()?.StartTime ?? double.MaxValue;

            var timingPoints = Beatmap.ControlPointInfo.TimingPoints;
            var barLines = new List<BarLine>();

            for (int i = 0; i < timingPoints.Count; i++)
            {
                TimingControlPoint point = timingPoints[i];

                // Stop on the beat before the next timing point, or if there is no next timing point stop slightly past the last object
                double endTime = i < timingPoints.Count - 1 ? timingPoints[i + 1].Time - point.BeatLength : lastObjectTime + point.BeatLength * (int)point.TimeSignature;

                int index = 0;
                for (double t = timingPoints[i].Time; Precision.DefinitelyBigger(endTime, t); t += point.BeatLength, index++)
                {
                    barLines.Add(new BarLine
                    {
                        StartTime = t,
                        ControlPoint = point,
                        BeatIndex = index
                    });
                }
            }

            BarLines = barLines;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            BarLines.ForEach(Playfield.Add);

            Config.BindWith(ManiaSetting.ScrollDirection, configDirection);
            configDirection.BindValueChanged(v => Direction.Value = (ScrollingDirection)v, true);

            Config.BindWith(ManiaSetting.ScrollTime, TimeRange);
        }

        /// <summary>
        /// Retrieves the column that intersects a screen-space position.
        /// </summary>
        /// <param name="screenSpacePosition">The screen-space position.</param>
        /// <returns>The column which intersects with <paramref name="screenSpacePosition"/>.</returns>
        public Column GetColumnByPosition(Vector2 screenSpacePosition) => Playfield.GetColumnByPosition(screenSpacePosition);

        protected override Playfield CreatePlayfield() => new ManiaPlayfield(Beatmap.Stages)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        };

        public override ScoreProcessor CreateScoreProcessor() => new ManiaScoreProcessor(this);

        public override int Variant => (int)(Mods.OfType<IPlayfieldTypeMod>().FirstOrDefault()?.PlayfieldType ?? PlayfieldType.Single) + Beatmap.TotalColumns;

        public override PassThroughInputManager CreateInputManager() => new ManiaInputManager(Ruleset.RulesetInfo, Variant);

        public override DrawableHitObject<ManiaHitObject> GetVisualRepresentation(ManiaHitObject h)
        {
            switch (h)
            {
                case HoldNote holdNote:
                    return new DrawableHoldNote(holdNote);
                case Note note:
                    return new DrawableNote(note);
                default:
                    return null;
            }
        }

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new ManiaFramedReplayInputHandler(replay);
    }
}
