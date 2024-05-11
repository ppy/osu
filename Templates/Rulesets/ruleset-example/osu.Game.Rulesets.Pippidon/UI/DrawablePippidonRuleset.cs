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
using osu.Game.Rulesets.Pippidon.Objects;
using osu.Game.Rulesets.Pippidon.Objects.Drawables;
using osu.Game.Rulesets.Pippidon.Replays;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Pippidon.UI
{
    [Cached]
    public partial class DrawablePippidonRuleset : DrawableRuleset<PippidonHitObject>
    {
        public DrawablePippidonRuleset(PippidonRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            : base(ruleset, beatmap, mods)
        {
        }

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new PippidonPlayfieldAdjustmentContainer();

        protected override Playfield CreatePlayfield() => new PippidonPlayfield();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new PippidonFramedReplayInputHandler(replay);

        public override DrawableHitObject<PippidonHitObject> CreateDrawableRepresentation(PippidonHitObject h) => new DrawablePippidonHitObject(h);

        protected override PassThroughInputManager CreateInputManager() => new PippidonInputManager(Ruleset?.RulesetInfo);
    }
}
