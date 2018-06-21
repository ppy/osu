// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Users;
using System.Linq;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
    public class PaginatedRecentActivityContainer : PaginatedContainer
    {
        public PaginatedRecentActivityContainer(Bindable<User> user, string header, string missing)
            : base(user, header, missing)
        {
            ItemsPerPage = 5;
        }

        protected override void ShowMore()
        {
            base.ShowMore();

            var req = new GetUserRecentActivitiesRequest(User.Value.Id, VisiblePages++ * ItemsPerPage);

            req.Success += activities =>
            {
                ShowMoreButton.FadeTo(activities.Count == ItemsPerPage ? 1 : 0);
                ShowMoreLoading.Hide();

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
            };

            Api.Queue(req);
        }
    }
}
