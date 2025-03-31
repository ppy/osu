// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Filter;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaFilterCriteria : IRulesetFilterCriteria
    {
        private FilterCriteria.OptionalRange<float> includedKeyCountRange;
        private readonly HashSet<int> includedKeyCounts = new HashSet<int>();
        private readonly HashSet<int> excludedKeyCounts = new HashSet<int>();

        public bool Matches(BeatmapInfo beatmapInfo, FilterCriteria criteria)
        {
            bool result = !includedKeyCountRange.HasFilter && includedKeyCounts.Count == 0;
            int keyCount = ManiaBeatmapConverter.GetColumnCount(LegacyBeatmapConversionDifficultyInfo.FromBeatmapInfo(beatmapInfo), criteria.Mods);

            result |= (includedKeyCountRange.HasFilter && includedKeyCountRange.IsInRange(keyCount)) || includedKeyCounts.Contains(keyCount);
            result &= !excludedKeyCounts.Contains(keyCount);

            return result;
        }

        public bool TryParseCustomKeywordCriteria(string key, Operator op, string strValues)
        {
            switch (key)
            {
                case "key":
                case "keys":
                    if (op == Operator.Equal)
                    {
                        foreach (string strValue in strValues.Split(','))
                        {
                            if (int.TryParse(strValue, out int value))
                            {
                                if (value > 0)
                                {
                                    includedKeyCounts.Add(value);
                                }
                                else
                                {
                                    excludedKeyCounts.Add(-value);
                                }
                            }
                        }

                        return true;
                    }
                    else
                    {
                        // In this case, the strValues is a string of a single value
                        return FilterQueryParser.TryUpdateCriteriaRange(ref includedKeyCountRange, op, strValues);
                    }
            }

            return false;
        }

        public bool FilterMayChangeFromMods(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            if (includedKeyCountRange.HasFilter || includedKeyCounts.Count != 0 || excludedKeyCounts.Count != 0)
            {
                // Interpreting as the Mod type is required for equality comparison.
                HashSet<Mod> oldSet = mods.OldValue.OfType<ManiaKeyMod>().AsEnumerable<Mod>().ToHashSet();
                HashSet<Mod> newSet = mods.NewValue.OfType<ManiaKeyMod>().AsEnumerable<Mod>().ToHashSet();

                if (!oldSet.SetEquals(newSet))
                    return true;
            }

            return false;
        }
    }
}
