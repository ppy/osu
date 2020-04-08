// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Tau.Objects;
using osu.Game.Rulesets.Tau.Objects.Drawables;
using osu.Game.Rulesets.Tau.Replays;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Tau.UI
{
    [Cached]
    public class DrawabletauRuleset : DrawableRuleset<TauHitObject>
    {
        public DrawabletauRuleset(TauRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new TauPlayfield(Beatmap.BeatmapInfo.BaseDifficulty);

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new TauFramedReplayInputHandler(replay);

        public override DrawableHitObject<TauHitObject> CreateDrawableRepresentation(TauHitObject h) => new DrawabletauHitObject(h);

        protected override PassThroughInputManager CreateInputManager() => new TauInputManager(Ruleset?.RulesetInfo);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new TauPlayfieldAdjustmentContainer();

        protected override ReplayRecorder CreateReplayRecorder(Replay replay) => new TauReplayRecorder(replay);
    }
}
