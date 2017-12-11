// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Rulesets;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select
{
    public class FilterCriteria
    {
        public GroupMode Group;
        public SortMode Sort;
        public string SearchText;
        public RulesetInfo Ruleset;
        public bool AllowConvertedBeatmaps;

        private bool canConvert(BeatmapInfo beatmapInfo) => beatmapInfo.RulesetID == Ruleset.ID || beatmapInfo.RulesetID == 0 && Ruleset.ID > 0 && AllowConvertedBeatmaps;

        public void Filter(List<BeatmapGroup> groups)
        {
            foreach (var g in groups)
            {
                var set = g.BeatmapSet;

                // we only support converts from osu! mode to other modes for now.
                // in the future this will have to change, at which point this condition will become a touch more complicated.
                bool hasCurrentMode = set.Beatmaps.Any(canConvert);

                bool match = hasCurrentMode;

                if (!string.IsNullOrEmpty(SearchText))
                    match &= set.Metadata.SearchableTerms.Any(term => term.IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) >= 0);

                foreach (var panel in g.BeatmapPanels)
                    panel.Filtered.Value = !canConvert(panel.Beatmap);

                switch (g.State)
                {
                    case BeatmapGroupState.Hidden:
                        if (match) g.State = BeatmapGroupState.Collapsed;
                        break;
                    default:
                        if (!match) g.State = BeatmapGroupState.Hidden;
                        break;
                }
            }

            switch (Sort)
            {
                default:
                case SortMode.Artist:
                    groups.Sort((x, y) => string.Compare(x.BeatmapSet.Metadata.Artist, y.BeatmapSet.Metadata.Artist, StringComparison.InvariantCultureIgnoreCase));
                    break;
                case SortMode.Title:
                    groups.Sort((x, y) => string.Compare(x.BeatmapSet.Metadata.Title, y.BeatmapSet.Metadata.Title, StringComparison.InvariantCultureIgnoreCase));
                    break;
                case SortMode.Author:
                    groups.Sort((x, y) => string.Compare(x.BeatmapSet.Metadata.Author.Username, y.BeatmapSet.Metadata.Author.Username, StringComparison.InvariantCultureIgnoreCase));
                    break;
                case SortMode.Difficulty:
                    groups.Sort((x, y) => x.BeatmapSet.MaxStarDifficulty.CompareTo(y.BeatmapSet.MaxStarDifficulty));
                    break;
            }
        }
    }
}
