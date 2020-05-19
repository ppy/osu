// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets;

namespace osu.Game.Screens.Multi.Lounge.Components
{
    public class FilterCriteria
    {
        public string SearchString;
        public PrimaryFilter PrimaryFilter;
        public SecondaryFilter SecondaryFilter;
        public RulesetInfo Ruleset;
    }
}
