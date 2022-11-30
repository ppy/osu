// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.EmptyFreeform.Objects;
using osu.Game.Rulesets.EmptyFreeform.Objects.Drawables;
using osu.Game.Rulesets.EmptyFreeform.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.EmptyFreeform.UI
{
    [Cached]
    public partial class DrawableEmptyFreeformRuleset : DrawableRuleset<EmptyFreeformHitObject>
    {
        public DrawableEmptyFreeformRuleset(EmptyFreeformRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new EmptyFreeformPlayfield();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new EmptyFreeformFramedReplayInputHandler(replay);

        public override DrawableHitObject<EmptyFreeformHitObject> CreateDrawableRepresentation(EmptyFreeformHitObject h) => new DrawableEmptyFreeformHitObject(h);

        protected override PassThroughInputManager CreateInputManager() => new EmptyFreeformInputManager(Ruleset?.RulesetInfo);
    }
}
