﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Game.Online.API.Requests;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.API;
using System.Collections.Generic;
using osuTK;
using osu.Framework.Allocation;
using osu.Game.Resources.Localisation.Web;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
    public partial class PaginatedRecentActivityContainer : PaginatedProfileSubsection<APIRecentActivity>
    {
        public PaginatedRecentActivityContainer(Bindable<APIUser> user)
            : base(user, missingText: EventsStrings.Empty)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ItemsContainer.Spacing = new Vector2(0, 8);
        }

        protected override APIRequest<List<APIRecentActivity>> CreateRequest(PaginationParameters pagination) =>
            new GetUserRecentActivitiesRequest(User.Value.Id, pagination);

        protected override Drawable CreateDrawableItem(APIRecentActivity model) => new DrawableRecentActivity(model);
    }
}
