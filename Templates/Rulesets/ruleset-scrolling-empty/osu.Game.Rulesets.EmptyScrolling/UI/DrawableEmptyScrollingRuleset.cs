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
using osu.Game.Rulesets.EmptyScrolling.Objects;
using osu.Game.Rulesets.EmptyScrolling.Objects.Drawables;
using osu.Game.Rulesets.EmptyScrolling.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.EmptyScrolling.UI
{
    [Cached]
    public partial class DrawableEmptyScrollingRuleset : DrawableScrollingRuleset<EmptyScrollingHitObject>
    {
        public DrawableEmptyScrollingRuleset(EmptyScrollingRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            : base(ruleset, beatmap, mods)
        {
            Direction.Value = ScrollingDirection.Left;
            TimeRange.Value = 6000;
        }

        protected override Playfield CreatePlayfield() => new EmptyScrollingPlayfield();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new EmptyScrollingFramedReplayInputHandler(replay);

        public override DrawableHitObject<EmptyScrollingHitObject> CreateDrawableRepresentation(EmptyScrollingHitObject h) => new DrawableEmptyScrollingHitObject(h);

        protected override PassThroughInputManager CreateInputManager() => new EmptyScrollingInputManager(Ruleset?.RulesetInfo);
    }
}
