// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class DrawableOsuEditRuleset : DrawableOsuRuleset
    {
        public DrawableOsuEditRuleset(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override Playfield CreatePlayfield() => new OsuPlayfieldNoCursor();

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new OsuPlayfieldAdjustmentContainer { Size = Vector2.One };

        private class OsuPlayfieldNoCursor : OsuPlayfield
        {
            protected override GameplayCursorContainer CreateCursor() => null;
        }
    }
}
