// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Utils;

namespace osu.Game.Screens.Ranking.Statistics.User
{
    public partial class AccuracyChangeRow : RankingChangeRow<double>
    {
        public AccuracyChangeRow()
            : base(stats => stats.Accuracy)
        {
        }

        protected override LocalisableString Label => UsersStrings.ShowStatsHitAccuracy;

        protected override LocalisableString FormatCurrentValue(double current) => (current / 100).FormatAccuracy();

        protected override int CalculateDifference(double previous, double current, out LocalisableString formattedDifference)
        {
            double difference = (current - previous) / 100;

            if (difference < 0)
                formattedDifference = difference.FormatAccuracy();
            else if (difference > 0)
                formattedDifference = LocalisableString.Interpolate($@"+{difference.FormatAccuracy()}");
            else
                formattedDifference = string.Empty;

            return current.CompareTo(previous);
        }
    }
}
