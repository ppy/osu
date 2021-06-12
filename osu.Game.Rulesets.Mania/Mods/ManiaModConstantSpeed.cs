// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModConstantSpeed : Mod, IApplicableToDrawableRuleset<ManiaHitObject>
    {
        public override string Name => "恒定";

        public override string Acronym => "CS";

        public override double ScoreMultiplier => 1;

        public override string Description => "对突如其来的变速说No!";

        public override IconUsage? Icon => FontAwesome.Solid.Equals;

        public override ModType Type => ModType.Conversion;

        public void ApplyToDrawableRuleset(DrawableRuleset<ManiaHitObject> drawableRuleset)
        {
            var maniaRuleset = (DrawableManiaRuleset)drawableRuleset;
            maniaRuleset.ScrollMethod = ScrollVisualisationMethod.Constant;
        }
    }
}
