// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Filter;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using Remotion.Linq.Utilities;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaFilterCriteria : IRulesetFilterCriteria
    {
        private Queue<FilterCriteria.OptionalRange<float>> keys = new Queue<FilterCriteria.OptionalRange<float>>();
        public bool Matches(BeatmapInfo beatmapInfo, FilterCriteria criteria)
        {
            // Performs as key.HasFilter
            bool result = (keys.Count == 0);

            foreach (FilterCriteria.OptionalRange<float> key in keys)
            {
                result |= key.IsInRange(ManiaBeatmapConverter.GetColumnCount(LegacyBeatmapConversionDifficultyInfo.FromBeatmapInfo(beatmapInfo), criteria.Mods));
            }

            return result;
        }

        public bool TryParseCustomKeywordCriteria(string key, Operator op, string values)
        {
            switch (key)
            {
                case "key":
                case "keys":

                    bool result = true;

                    foreach (string value in values.Split(','))
                    {
                        FilterCriteria.OptionalRange<float> tmp = new FilterCriteria.OptionalRange<float>();
                        result &= FilterQueryParser.TryUpdateCriteriaRange(ref tmp, op, value);
                        keys.Enqueue(tmp);
                    }

                    return result;

            }

            return false;
        }

        public bool FilterMayChangeFromMods(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            bool result = false;

            foreach (FilterCriteria.OptionalRange<float> key in keys)
            {
                if (key.HasFilter)
                {
                    // Interpreting as the Mod type is required for equality comparison.
                    HashSet<Mod> oldSet = mods.OldValue.OfType<ManiaKeyMod>().AsEnumerable<Mod>().ToHashSet();
                    HashSet<Mod> newSet = mods.NewValue.OfType<ManiaKeyMod>().AsEnumerable<Mod>().ToHashSet();

                    if (!oldSet.SetEquals(newSet))
                        result |= true;
                }
            }
            return result;
        }
    }
}
