// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class DrawableOsuEditorRuleset : DrawableOsuRuleset
    {
        public DrawableOsuEditorRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new OsuEditorPlayfield();

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new OsuPlayfieldAdjustmentContainer { Size = Vector2.One };

        private partial class OsuEditorPlayfield : OsuPlayfield
        {
            protected override GameplayCursorContainer? CreateCursor() => null;

            public OsuEditorPlayfield()
            {
                HitPolicy = new AnyOrderHitPolicy();
            }
        }
    }
}
