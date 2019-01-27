// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Cursor;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuEditRulesetContainer : OsuRulesetContainer
    {
        public OsuEditRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override CursorContainer CreateCursor() => null;

        protected override Playfield CreatePlayfield() => new OsuPlayfield { Size = Vector2.One　};
    }
}
