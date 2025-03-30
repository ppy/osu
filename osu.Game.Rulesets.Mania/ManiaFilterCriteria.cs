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
        private FilterCriteria.OptionalRange<float> included_key_range;
        private HashSet<int> included_keys = new HashSet<int>();
        private HashSet<int> excluded_keys = new HashSet<int>();
        public bool Matches(BeatmapInfo beatmapInfo, FilterCriteria criteria)
        {
            bool result = (!included_key_range.HasFilter) && (included_keys.Count == 0);
            int key_index = ManiaBeatmapConverter.GetColumnCount(LegacyBeatmapConversionDifficultyInfo.FromBeatmapInfo(beatmapInfo), criteria.Mods);

            result |= (included_key_range.HasFilter && included_key_range.IsInRange(key_index)) || included_keys.Contains(key_index);
            result &= !excluded_keys.Contains(key_index);

            return result;
        }

        public bool TryParseCustomKeywordCriteria(string key, Operator op, string str_values)
        {
            switch (key)
            {
                case "key":
                case "keys":
                    if (op == Operator.Equal)
                    {
                        foreach (string str_value in str_values.Split(','))
                        {
                            if (int.TryParse(str_value, out int value))
                            {
                                if (value > 0)
                                {
                                    included_keys.Add(value);
                                }
                                else
                                {
                                    excluded_keys.Add(-value);
                                }
                            }
                        }

                        return true;
                    }
                    else
                    {
                        // In this case, the str_values is a string of a single value
                        return FilterQueryParser.TryUpdateCriteriaRange(ref included_key_range, op, str_values);
                    }
            }

            return false;
        }

        public bool FilterMayChangeFromMods(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            if (included_key_range.HasFilter || included_keys.Count != 0 || excluded_keys.Count != 0)
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
