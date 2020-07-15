// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModFadeIn : ManiaModHidden
    {
        public override string Name => "Fade In";
        public override string Acronym => "FI";
        public override IconUsage? Icon => OsuIcon.ModHidden;
        public override string Description => @"Keys appear out of nowhere!";

        public override void ApplyToDrawableRuleset(DrawableRuleset<ManiaHitObject> drawableRuleset)
        {
            base.ApplyToDrawableRuleset(drawableRuleset);
            laneCovers.ForEach(laneCover => laneCover.Scale = new Vector2(1f, -1f));
        }
    }
}
