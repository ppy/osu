// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System.Linq;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModMirror : Mod, IApplicableToRulesetContainer<ManiaHitObject>
    {
        public override string Name => "Mirror";
        public override string ShortenedName => "MR";
        public override ModType Type => ModType.Special;
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;

        public void ApplyToRulesetContainer(RulesetContainer<ManiaHitObject> rulesetContainer)
        {
            var availableColumns = ((ManiaRulesetContainer)rulesetContainer).Beatmap.TotalColumns;

            rulesetContainer.Objects.OfType<ManiaHitObject>().ForEach(h => h.Column = availableColumns - 1 - h.Column);
        }
    }
}
