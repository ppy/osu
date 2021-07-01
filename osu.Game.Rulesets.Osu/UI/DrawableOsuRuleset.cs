// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public class DrawableOsuRuleset : DrawableRuleset<OsuHitObject>
    {
        protected new OsuRulesetConfigManager Config => (OsuRulesetConfigManager)base.Config;

        public new OsuPlayfield Playfield => (OsuPlayfield)base.Playfield;

        public DrawableOsuRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            : base(ruleset, beatmap, mods)
        {
        }

        public override DrawableHitObject<OsuHitObject> CreateDrawableRepresentation(OsuHitObject h) => null;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true; // always show the gameplay cursor

        protected override Playfield CreatePlayfield() => new OsuPlayfield();

        protected override PassThroughInputManager CreateInputManager() => new OsuInputManager(Ruleset.RulesetInfo);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new OsuPlayfieldAdjustmentContainer { AlignWithStoryboard = true };

        protected override ResumeOverlay CreateResumeOverlay() => new OsuResumeOverlay();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new OsuFramedReplayInputHandler(replay);

        protected override ReplayRecorder CreateReplayRecorder(Score score) => new OsuReplayRecorder(score);

        public override double GameplayStartTime
        {
            get
            {
                if (Objects.FirstOrDefault() is OsuHitObject first)
                    return first.StartTime - Math.Max(2000, first.TimePreempt);

                return 0;
            }
        }
    }
}
