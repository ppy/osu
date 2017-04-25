// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select
{
    public class FilterCriteria
    {
        public GroupMode Group;
        public SortMode Sort;
        public string SearchText;
        public RulesetInfo Ruleset;

        public void Filter(List<BeatmapGroup> groups)
        {
            foreach (var g in groups)
            {
                var set = g.BeatmapSet;

                bool hasCurrentMode = set.Beatmaps.Any(bm => bm.RulesetID == (Ruleset?.ID ?? 0));

                bool match = hasCurrentMode;

                match &= string.IsNullOrEmpty(SearchText)
                    || (set.Metadata.Artist ?? string.Empty).IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) != -1
                    || (set.Metadata.ArtistUnicode ?? string.Empty).IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) != -1
                    || (set.Metadata.Title ?? string.Empty).IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) != -1
                    || (set.Metadata.TitleUnicode ?? string.Empty).IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) != -1
                    || (set.Metadata.Tags ?? string.Empty).IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) != -1
                    || (set.Metadata.Source ?? string.Empty).IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) != -1;

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
                    groups.Sort((x, y) => string.Compare(x.BeatmapSet.Metadata.Author, y.BeatmapSet.Metadata.Author, StringComparison.InvariantCultureIgnoreCase));
                    break;
                case SortMode.Difficulty:
                    groups.Sort((x, y) => x.BeatmapSet.MaxStarDifficulty.CompareTo(y.BeatmapSet.MaxStarDifficulty));
                    break;
            }
        }
    }
}