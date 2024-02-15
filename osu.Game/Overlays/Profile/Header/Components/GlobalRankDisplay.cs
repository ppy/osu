// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class GlobalRankDisplay : ProfileValueDisplay
    {
        public readonly Bindable<UserStatistics?> UserStatistics = new Bindable<UserStatistics?>();
        public readonly Bindable<APIUser?> User = new Bindable<APIUser?>();

        public GlobalRankDisplay()
            : base(true)
        {
            Title = UsersStrings.ShowRankGlobalSimple;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            UserStatistics.BindValueChanged(s =>
            {
                Content = s.NewValue?.GlobalRank?.ToLocalisableString("\\##,##0") ?? (LocalisableString)"-";
            }, true);

            // needed as `UserStatistics` doesn't populate `User`
            User.BindValueChanged(u =>
            {
                var rankHighest = u.NewValue?.RankHighest;

                ContentTooltipText = rankHighest != null
                    ? UsersStrings.ShowRankHighest(rankHighest.Rank.ToLocalisableString("\\##,##0"), rankHighest.UpdatedAt.ToLocalisableString(@"d MMM yyyy"))
                    : string.Empty;
            }, true);
        }
    }
}
