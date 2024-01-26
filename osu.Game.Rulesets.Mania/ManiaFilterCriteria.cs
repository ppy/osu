// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Filter;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaFilterCriteria : IRulesetFilterCriteria
    {
        private FilterCriteria.OptionalRange<float> keys;

        public bool Matches(BeatmapInfo beatmapInfo)
        {
            return !keys.HasFilter || keys.IsInRange(ManiaBeatmapConverter.GetColumnCount(LegacyBeatmapConversionDifficultyInfo.FromBeatmapInfo(beatmapInfo)));
        }

        public bool TryParseCustomKeywordCriteria(string key, Operator op, string value)
        {
            switch (key)
            {
                case "key":
                case "keys":
                    return FilterQueryParser.TryUpdateCriteriaRange(ref keys, op, value);
            }

            return false;
        }
    }
}
