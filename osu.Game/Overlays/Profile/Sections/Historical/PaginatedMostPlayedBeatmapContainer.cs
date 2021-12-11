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
    public class PaginatedMostPlayedBeatmapContainer : PaginatedProfileSubsection<APIUserMostPlayedBeatmap>
    {
        public PaginatedMostPlayedBeatmapContainer(Bindable<APIUser> user)
            : base(user, UsersStrings.ShowExtraHistoricalMostPlayedTitle)
        {
            ItemsPerPage = 5;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ItemsContainer.Direction = FillDirection.Vertical;
        }

        protected override int GetCount(APIUser user) => user.BeatmapPlaycountsCount;

        protected override APIRequest<List<APIUserMostPlayedBeatmap>> CreateRequest() =>
            new GetUserMostPlayedBeatmapsRequest(User.Value.Id, VisiblePages++, ItemsPerPage);

        protected override Drawable CreateDrawableItem(APIUserMostPlayedBeatmap mostPlayed) =>
            new DrawableMostPlayedBeatmap(mostPlayed);
    }
}
