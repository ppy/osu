// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class PaginatedMostPlayedBeatmapContainer : PaginatedContainer<APIUserMostPlayedBeatmap>
    {
        public PaginatedMostPlayedBeatmapContainer(Bindable<User> user)
            : base(user, "Most Played Beatmaps", "No records. :(")
        {
            ItemsPerPage = 5;

            ItemsContainer.Direction = FillDirection.Vertical;
        }

        protected override APIRequest<List<APIUserMostPlayedBeatmap>> CreateRequest() =>
            new GetUserMostPlayedBeatmapsRequest(User.Value.Id, VisiblePages++, ItemsPerPage);

        protected override Drawable CreateDrawableItem(APIUserMostPlayedBeatmap model) =>
            new DrawableMostPlayedBeatmap(model.GetBeatmapInfo(Rulesets), model.PlayCount);
    }
}
