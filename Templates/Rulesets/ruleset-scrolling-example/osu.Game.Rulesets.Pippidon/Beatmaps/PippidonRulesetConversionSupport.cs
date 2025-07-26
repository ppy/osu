// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Filter;

namespace osu.Game.Rulesets.Pippidon.Beatmaps
{
    public class PippidonRulesetConversionSupport : IRulesetConvertSupport
    {
        public bool CanBePlayed(RulesetInfo ruleset, bool conversionEnabled)
        {
            // Always show ctb maps even without converts, since extensive playtesting has shown that the maps match
            // pippidon very well and we also don't have many maps to start out with yet.
            // Show std only when converts are enabled
            return ruleset.ShortName == "pippidon" || ruleset.ShortName == "fruits" ||
                   (conversionEnabled && ruleset.ShortName == "osu");
        }
    }
}
