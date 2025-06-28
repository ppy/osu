// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Online.API.Requests;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Online.API;

namespace osu.Game.Overlays.Profile.Sections
{
    public partial class RanksSection : ProfileSection
    {
        public override LocalisableString Title => UsersStrings.ShowExtraTopRanksTitle;

        public override string Identifier => @"top_ranks";

        private PaginatedScoreContainer pinnedScoreContainer;
        private PaginatedScoreContainer bestScoreContainer;
        private PaginatedScoreContainer firstsScoreContainer;

        private HashSet<long> pinnedScoreIds = new HashSet<long>();
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public RanksSection()
        {
            Children = new[]
            {
                pinnedScoreContainer = new PaginatedScoreContainer(ScoreType.Pinned, User, UsersStrings.ShowExtraTopRanksPinnedTitle, pinnedScoreIds),
                bestScoreContainer = new PaginatedScoreContainer(ScoreType.Best, User, UsersStrings.ShowExtraTopRanksBestTitle, pinnedScoreIds),
                firstsScoreContainer = new PaginatedScoreContainer(ScoreType.Firsts, User, UsersStrings.ShowExtraTopRanksFirstTitle, pinnedScoreIds)
            };

            pinnedScoreContainer.OnScorePinChanged = () => refreshAllSections();
            bestScoreContainer.OnScorePinChanged = () => refreshAllSections();
            firstsScoreContainer.OnScorePinChanged = () => refreshAllSections();
        }

        private void refreshAllSections()
        {
            pinnedScoreIds.Clear();

            Schedule(() =>
            {
                var currentUser = User.Value;
                if (currentUser?.User != null)
                {
                    var req = new GetUserRequest(currentUser.User.Id);
                    req.Success += user =>
                    {
                        Schedule(() =>
                        {
                            if (User.Value != null)
                            {
                                User.Value = new UserProfileData(user, User.Value.Ruleset);
                            }
                        });
                    };
                    api.Queue(req);
                }
            });
        }
    }
}
