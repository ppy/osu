// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Users;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
    public class PaginatedRecentActivityContainer : PaginatedContainer
    {
        private GetUserRecentActivitiesRequest request;

        public PaginatedRecentActivityContainer(Bindable<User> user, string header, string missing)
            : base(user, header, missing)
        {
        }

        protected override void ShowMore()
        {
            request = new GetUserRecentActivitiesRequest(User.Value.Id, VisiblePages++ * ITEMS_PER_PAGE);
            request.Success += activities => Schedule(() =>
            {
                MoreButton.FadeTo(activities.Count == ITEMS_PER_PAGE ? 1 : 0);
                MoreButton.IsLoading = false;

                if (!activities.Any() && VisiblePages == 1)
                {
                    MissingText.Show();
                    return;
                }

                MissingText.Hide();

                foreach (APIRecentActivity activity in activities)
                {
                    ItemsContainer.Add(new DrawableRecentActivity(activity));
                }
            });

            Api.Queue(request);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            request?.Cancel();
        }
    }
}
