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
        private readonly HashSet<int> includedKeyCounts = Enumerable.Range(1, 20).ToHashSet();

        public bool Matches(BeatmapInfo beatmapInfo, FilterCriteria criteria)
        {
            int keyCount = ManiaBeatmapConverter.GetColumnCount(LegacyBeatmapConversionDifficultyInfo.FromBeatmapInfo(beatmapInfo), criteria.Mods);

            return includedKeyCounts.Contains(keyCount);
        }

        public bool TryParseCustomKeywordCriteria(string key, Operator op, string strValues)
        {
            switch (key)
            {
                case "key":
                case "keys":
                    if (op == Operator.Equal)
                    {
                        includedKeyCounts.Clear();

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
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (!int.TryParse(strValues, out int value))
                        {
                            return false;
                        }

                        if (value <= 0)
                        {
                            return false;
                        }

                        switch (op)
                        {
                            case Operator.Less:
                                includedKeyCounts.RemoveWhere(k => k >= value);
                                break;
                            case Operator.LessOrEqual:
                                includedKeyCounts.RemoveWhere(k => k > value);
                                break;
                            case Operator.Greater:
                                includedKeyCounts.RemoveWhere(k => k <= value);
                                break;
                            case Operator.GreaterOrEqual:
                                includedKeyCounts.RemoveWhere(k => k < value);
                                break;
                        }
                    }

                    break;
            }

            return true;
        }

        public bool FilterMayChangeFromMods(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            if (includedKeyCounts.Count > 0)
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
