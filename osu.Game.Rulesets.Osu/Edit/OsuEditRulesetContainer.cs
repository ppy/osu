// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Cursor;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.UI;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuEditRulesetContainer : OsuRulesetContainer
    {
        public OsuEditRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override Vector2 PlayfieldArea => Vector2.One;

        protected override CursorContainer CreateCursor() => null;
    }
}
