using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile.Sections.Recent;
using osu.Game.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Overlays.Profile.Sections
{
    class PaginatedRecentActivityContainer : PaginatedContainer
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

                foreach (RecentActivity activity in activities)
                {
                    ItemsContainer.Add(new DrawableRecentActivity(activity, User));
                }
            };

            Api.Queue(req);
        }
    }
}
