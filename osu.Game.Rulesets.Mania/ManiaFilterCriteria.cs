// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
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
        private readonly HashSet<int> includedKeyCounts = Enumerable.Range(1, LegacyBeatmapDecoder.MAX_MANIA_KEY_COUNT).ToHashSet();
        private FilterCriteria.OptionalRange<float> longNoteRatio;

        public bool Matches(BeatmapInfo beatmapInfo, FilterCriteria criteria)
        {
            int keyCount = ManiaBeatmapConverter.GetColumnCount(LegacyBeatmapConversionDifficultyInfo.FromBeatmapInfo(beatmapInfo), criteria.Mods);

            bool keyCountMatch = includedKeyCounts.Contains(keyCount);
            bool longNoteRatioMatch = !longNoteRatio.HasFilter || longNoteRatio.IsInRange(calculatelongNoteRatio(beatmapInfo));

            return keyCountMatch && longNoteRatioMatch;
        }

        public bool TryParseCustomKeywordCriteria(string key, Operator op, string strValues)
        {
            switch (key)
            {
                case "key":
                case "keys":
                {
                    var keyCounts = new HashSet<int>();

                    foreach (string strValue in strValues.Split(','))
                    {
                        if (!int.TryParse(strValue, out int keyCount))
                            return false;

                        keyCounts.Add(keyCount);
                    }

                    int? singleKeyCount = keyCounts.Count == 1 ? keyCounts.Single() : null;

                    switch (op)
                    {
                        case Operator.Equal:
                            includedKeyCounts.IntersectWith(keyCounts);
                            return true;

                        case Operator.NotEqual:
                            includedKeyCounts.ExceptWith(keyCounts);
                            return true;

                        case Operator.Less:
                            if (singleKeyCount == null) return false;

                            includedKeyCounts.RemoveWhere(k => k >= singleKeyCount.Value);
                            return true;

                        case Operator.LessOrEqual:
                            if (singleKeyCount == null) return false;

                            includedKeyCounts.RemoveWhere(k => k > singleKeyCount.Value);
                            return true;

                        case Operator.Greater:
                            if (singleKeyCount == null) return false;

                            includedKeyCounts.RemoveWhere(k => k <= singleKeyCount.Value);
                            return true;

                        case Operator.GreaterOrEqual:
                            if (singleKeyCount == null) return false;

                            includedKeyCounts.RemoveWhere(k => k < singleKeyCount.Value);
                            return true;

                        default:
                            return false;
                    }
                }

                case "ln":
                case "lns":
                    return FilterQueryParser.TryUpdateCriteriaRange(ref longNoteRatio, op, strValues);
            }

            return false;
        }

        private static float calculatelongNoteRatio(BeatmapInfo beatmapInfo)
        {
            int holdNotes = beatmapInfo.EndTimeObjectCount;
            int totalNotes = beatmapInfo.TotalObjectCount;
            int sum = Math.Max(1, totalNotes);

            return holdNotes / (float)sum * 100;
        }

        public bool FilterMayChangeFromMods(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            if (includedKeyCounts.Count != LegacyBeatmapDecoder.MAX_MANIA_KEY_COUNT)
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
