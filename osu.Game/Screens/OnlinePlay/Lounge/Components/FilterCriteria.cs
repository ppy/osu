// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public class FilterCriteria
    {
        public string SearchString;
        public RoomStatusFilter Status;
        public string Category;
        public RulesetInfo Ruleset;
    }
}
