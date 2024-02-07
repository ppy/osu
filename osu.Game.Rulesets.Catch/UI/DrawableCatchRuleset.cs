// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Catch.UI
{
    public partial class DrawableCatchRuleset : DrawableScrollingRuleset<CatchHitObject>
    {
        protected override bool UserScrollSpeedAdjustment => false;

        public DrawableCatchRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod>? mods = null)
            : base(ruleset, beatmap, mods)
        {
            Direction.Value = ScrollingDirection.Down;
            TimeRange.Value = GetTimeRange(beatmap.Difficulty.ApproachRate);
            VisualisationMethod = ScrollVisualisationMethod.Constant;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // With relax mod, input maps directly to x position and left/right buttons are not used.
            if (!Mods.Any(m => m is ModRelax))
                KeyBindingInputManager.Add(new CatchTouchInputMapper());
        }

        protected double GetTimeRange(float approachRate) => IBeatmapDifficultyInfo.DifficultyRange(approachRate, 1800, 1200, 450);

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new CatchFramedReplayInputHandler(replay);

        protected override ReplayRecorder CreateReplayRecorder(Score score) => new CatchReplayRecorder(score, (CatchPlayfield)Playfield);

        protected override Playfield CreatePlayfield() => new CatchPlayfield(Beatmap.Difficulty);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new CatchPlayfieldAdjustmentContainer();

        protected override PassThroughInputManager CreateInputManager() => new CatchInputManager(Ruleset.RulesetInfo);

        public override DrawableHitObject<CatchHitObject>? CreateDrawableRepresentation(CatchHitObject h) => null;

        protected override ResumeOverlay CreateResumeOverlay() => new DelayedResumeOverlay();
    }
}
