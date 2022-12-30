// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public partial class PaginatedMostPlayedBeatmapContainer : PaginatedProfileSubsection<APIUserMostPlayedBeatmap>
    {
        public PaginatedMostPlayedBeatmapContainer(Bindable<UserProfile?> userProfile)
            : base(userProfile, UsersStrings.ShowExtraHistoricalMostPlayedTitle)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ItemsContainer.Direction = FillDirection.Vertical;
        }

        protected override int GetCount(APIUser user) => user.BeatmapPlayCountsCount;

        protected override APIRequest<List<APIUserMostPlayedBeatmap>> CreateRequest(UserProfile userProfile, PaginationParameters pagination) =>
            new GetUserMostPlayedBeatmapsRequest(userProfile.User.Id, pagination);

        protected override Drawable CreateDrawableItem(APIUserMostPlayedBeatmap mostPlayed) =>
            new DrawableMostPlayedBeatmap(mostPlayed);
    }
}
