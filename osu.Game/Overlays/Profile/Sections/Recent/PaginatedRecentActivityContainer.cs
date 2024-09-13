// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
    public partial class PaginatedRecentActivityContainer : PaginatedProfileSubsection<APIRecentActivity>
    {
        public PaginatedRecentActivityContainer(Bindable<UserProfileData?> user)
            : base(user, missingText: EventsStrings.Empty)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ItemsContainer.Spacing = new Vector2(0, 8);
        }

        protected override APIRequest<List<APIRecentActivity>> CreateRequest(UserProfileData user, PaginationParameters pagination) =>
            new GetUserRecentActivitiesRequest(user.User.Id, pagination);

        protected override Drawable CreateDrawableItem(APIRecentActivity model) => new DrawableRecentActivity(model);
    }
}
