// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Screens.Ranking.Statistics.User
{
    public partial class MaximumComboChangeRow : RankingChangeRow<int>
    {
        public MaximumComboChangeRow()
            : base(stats => stats.MaxCombo)
        {
        }

        protected override LocalisableString Label => UsersStrings.ShowStatsMaximumCombo;

        protected override LocalisableString FormatCurrentValue(int current) => LocalisableString.Interpolate($@"{current:N0}x");

        protected override int CalculateDifference(int previous, int current, out LocalisableString formattedDifference)
        {
            int difference = current - previous;

            if (difference < 0)
                formattedDifference = LocalisableString.Interpolate($@"{difference:N0}x");
            else if (difference > 0)
                formattedDifference = LocalisableString.Interpolate($@"+{difference:N0}x");
            else
                formattedDifference = string.Empty;

            return current.CompareTo(previous);
        }
    }
}
