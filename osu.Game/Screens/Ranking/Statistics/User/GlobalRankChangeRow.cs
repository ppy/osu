// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

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
            => current?.ToLocalisableString(@"N0") ?? string.Empty;

        protected override int CalculateDifference(int? previous, int? current, out LocalisableString formattedDifference)
        {
            if (previous == null && current == null)
            {
                formattedDifference = string.Empty;
                return 0;
            }

            if (previous == null && current != null)
            {
                formattedDifference = LocalisableString.Interpolate($"+{current.Value:N0}");
                return 1;
            }

            if (previous != null && current == null)
            {
                formattedDifference = LocalisableString.Interpolate($"-{previous.Value:N0}");
                return -1;
            }

            Debug.Assert(previous != null && current != null);

            // note that ranks work backwards, i.e. lower rank is _better_.
            int difference = previous.Value - current.Value;

            if (difference < 0)
                formattedDifference = difference.ToLocalisableString(@"N0");
            else if (difference > 0)
                formattedDifference = LocalisableString.Interpolate($"+{difference:N0}");
            else
                formattedDifference = string.Empty;

            return difference;
        }
    }
}
