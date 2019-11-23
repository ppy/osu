// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class DrawableTaikoRuleset : DrawableScrollingRuleset<TaikoHitObject>
    {
        protected override ScrollVisualisationMethod VisualisationMethod => ScrollVisualisationMethod.Overlapping;

        protected override bool UserScrollSpeedAdjustment => false;

        public DrawableTaikoRuleset(Ruleset ruleset, IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
            Direction.Value = ScrollingDirection.Left;
            TimeRange.Value = 7000;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            new BarLineGenerator<BarLine>(Beatmap).BarLines.ForEach(bar => Playfield.Add(bar.Major ? new DrawableBarLineMajor(bar) : new DrawableBarLine(bar)));
        }

        public override ScoreProcessor CreateScoreProcessor() => new TaikoScoreProcessor(this);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new TaikoPlayfieldAdjustmentContainer();

        protected override PassThroughInputManager CreateInputManager() => new TaikoInputManager(Ruleset.RulesetInfo);

        protected override Playfield CreatePlayfield() => new TaikoPlayfield(Beatmap.ControlPointInfo);

        public override DrawableHitObject<TaikoHitObject> CreateDrawableRepresentation(TaikoHitObject h)
        {
            switch (h)
            {
                case CentreHit centreHit:
                    return new DrawableCentreHit(centreHit);

                case RimHit rimHit:
                    return new DrawableRimHit(rimHit);

                case DrumRoll drumRoll:
                    return new DrawableDrumRoll(drumRoll);

                case Swell swell:
                    return new DrawableSwell(swell);
            }

            return null;
        }

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new TaikoFramedReplayInputHandler(replay);
    }
}
