// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModConstantSpeed : Mod, IApplicableToDrawableRuleset<ManiaHitObject>
    {
        public override string Name => "Constant Speed";

        public override string Acronym => "CS";

        public override double ScoreMultiplier => 0.9;

        public override LocalisableString Description => "No more tricky speed changes!";

        public override IconUsage? Icon => FontAwesome.Solid.Equals;

        public override ModType Type => ModType.Conversion;

        public void ApplyToDrawableRuleset(DrawableRuleset<ManiaHitObject> drawableRuleset)
        {
            var maniaRuleset = (DrawableManiaRuleset)drawableRuleset;
            maniaRuleset.VisualisationMethod = ScrollVisualisationMethod.Constant;
        }
    }
}
