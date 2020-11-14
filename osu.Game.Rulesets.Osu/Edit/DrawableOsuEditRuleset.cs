// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics.Pooling;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class DrawableOsuEditRuleset : DrawableOsuRuleset
    {
        public DrawableOsuEditRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new OsuEditPlayfield();

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new OsuPlayfieldAdjustmentContainer { Size = Vector2.One };

        private class OsuEditPlayfield : OsuPlayfield
        {
            protected override GameplayCursorContainer CreateCursor() => null;

            protected override DrawablePool<TDrawable> CreatePool<TDrawable>(int initialSize, int? maximumSize = null)
                => new DrawableOsuEditPool<TDrawable>(CheckHittable, OnHitObjectLoaded, initialSize, maximumSize);
        }
    }
}
