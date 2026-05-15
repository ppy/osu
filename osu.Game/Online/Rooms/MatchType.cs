// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Online.Rooms
{
    public enum MatchType
    {
        // used for osu-web deserialization so names shouldn't be changed.

        Playlists,

        [LocalisableDescription(typeof(MatchesStrings), nameof(MatchesStrings.MatchTeamTypesHeadToHead))]
        HeadToHead,

        [LocalisableDescription(typeof(MatchesStrings), nameof(MatchesStrings.MatchTeamTypesTeamVersus))]
        TeamVersus,

        /// <summary>
        /// Matchmaking: Quick play
        /// </summary>
        Matchmaking,

        /// <summary>
        /// Matchmaking: Ranked play
        /// </summary>
        RankedPlay
    }

    public static class MatchTypeExtensions
    {
        public static bool IsMatchmakingType(this MatchType type)
        {
            switch (type)
            {
                case MatchType.Matchmaking:
                case MatchType.RankedPlay:
                    return true;

                default:
                    return false;
            }
        }
    }
}
