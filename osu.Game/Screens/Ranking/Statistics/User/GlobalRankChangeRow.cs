// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Utils;

namespace osu.Game.Screens.Ranking.Statistics.User
{
    public partial class GlobalRankChangeRow : RankingChangeRow<int?>
    {
        public GlobalRankChangeRow()
            : base(stats => stats.GlobalRank)
        {
        }

        protected override LocalisableString Label => UsersStrings.ShowRankGlobalSimple;

        protected override LocalisableString FormatCurrentValue(int? current)
            => current == null ? string.Empty : current.Value.FormatRank();

        protected override int CalculateDifference(int? previous, int? current, out LocalisableString formattedDifference)
        {
            if (previous == null && current == null)
            {
                formattedDifference = string.Empty;
                return 0;
            }

            if (previous == null && current != null)
            {
                formattedDifference = LocalisableString.Interpolate($"+{current.Value.FormatRank()}");
                return 1;
            }

            if (previous != null && current == null)
            {
                formattedDifference = LocalisableString.Interpolate($"-{previous.Value.FormatRank()}");
                return -1;
            }

            Debug.Assert(previous != null && current != null);

            // note that ranks work backwards, i.e. lower rank is _better_.
            int difference = previous.Value - current.Value;

            if (difference < 0)
                formattedDifference = difference.FormatRank();
            else if (difference > 0)
                formattedDifference = LocalisableString.Interpolate($"+{difference.FormatRank()}");
            else
                formattedDifference = string.Empty;

            return difference;
        }
    }
}
