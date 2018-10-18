// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModRandom : Mod, IApplicableToRulesetContainer<ManiaHitObject>
    {
        public override string Name => "Random";
        public override string ShortenedName => "RD";
        public override ModType Type => ModType.Conversion;
        public override FontAwesome Icon => FontAwesome.fa_osu_dice;
        public override string Description => @"Shuffle around the keys!";
        public override double ScoreMultiplier => 1;

        public void ApplyToRulesetContainer(RulesetContainer<ManiaHitObject> rulesetContainer)
        {
            var availableColumns = ((ManiaRulesetContainer)rulesetContainer).Beatmap.TotalColumns;
            var shuffledColumns = Enumerable.Range(0, availableColumns).OrderBy(item => RNG.Next()).ToList();

            rulesetContainer.Objects.OfType<ManiaHitObject>().ForEach(h => h.Column = shuffledColumns[h.Column]);
        }
    }
}
