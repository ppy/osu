// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Screens.Ranking.Statistics.User
{
    public partial class PerformancePointsChangeRow : RankingChangeRow<decimal?>
    {
        public PerformancePointsChangeRow()
            : base(stats => stats.PP)
        {
        }

        protected override LocalisableString Label => RankingsStrings.StatPerformance;

        protected override LocalisableString FormatCurrentValue(decimal? current)
            => current == null ? string.Empty : LocalisableString.Interpolate($@"{current:N0}pp");

        protected override int CalculateDifference(decimal? previous, decimal? current, out LocalisableString formattedDifference)
        {
            if (previous == null && current == null)
            {
                formattedDifference = string.Empty;
                return 0;
            }

            if (previous == null && current != null)
            {
                formattedDifference = LocalisableString.Interpolate($"+{current.Value:N0}pp");
                return 1;
            }

            if (previous != null && current == null)
            {
                formattedDifference = LocalisableString.Interpolate($"-{previous.Value:N0}pp");
                return -1;
            }

            Debug.Assert(previous != null && current != null);

            decimal difference = current.Value - previous.Value;

            if (difference < 0)
                formattedDifference = LocalisableString.Interpolate($@"{difference:N0}pp");
            else if (difference > 0)
                formattedDifference = LocalisableString.Interpolate($@"+{difference:N0}pp");
            else
                formattedDifference = string.Empty;

            return current.Value.CompareTo(previous.Value);
        }
    }
}
