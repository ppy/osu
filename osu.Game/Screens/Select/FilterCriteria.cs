// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select
{
    public class FilterCriteria
    {
        public GroupMode Group;
        public SortMode Sort;

        public string[] SearchTerms = Array.Empty<string>();

        public RulesetInfo Ruleset;
        public bool AllowConvertedBeatmaps;

        private string searchText;

        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;
                SearchTerms = searchText.Split(',', ' ', '!').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            }
        }
    }
}
