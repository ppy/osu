using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets;

namespace osu.Game.Rulesets.Tkacz
{
    public class TkaczRuleset : Ruleset
    {
        public override string Description => "o!tkacz";

        public override string ShortName => "tkacz";

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap)
        {
            throw new NotImplementedException();
        }

        public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap)
        {
            throw new NotImplementedException();
        }

        public override RulesetContainer CreateRulesetContainerWith(WorkingBeatmap beatmap)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            throw new NotImplementedException();
        }
    }
}
