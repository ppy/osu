// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Screens.Ranking.Statistics.User
{
    public partial class TotalScoreChangeRow : RankingChangeRow<long>
    {
        public TotalScoreChangeRow()
            : base(stats => stats.TotalScore)
        {
        }

        protected override LocalisableString Label => UsersStrings.ShowStatsTotalScore;

        protected override LocalisableString FormatCurrentValue(long current) => current.ToLocalisableString(@"N0");

        protected override int CalculateDifference(long previous, long current, out LocalisableString formattedDifference)
        {
            long difference = current - previous;

            if (difference < 0)
                formattedDifference = difference.ToLocalisableString(@"N0");
            else if (difference > 0)
                formattedDifference = LocalisableString.Interpolate($@"+{difference:N0}");
            else
                formattedDifference = string.Empty;

            return current.CompareTo(previous);
        }
    }
}
