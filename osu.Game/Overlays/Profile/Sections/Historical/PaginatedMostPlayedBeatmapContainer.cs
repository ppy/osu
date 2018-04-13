// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class PaginatedMostPlayedBeatmapContainer : PaginatedContainer
    {
        public PaginatedMostPlayedBeatmapContainer(Bindable<User> user)
            :base(user, "Most Played Beatmaps", "No records. :(")
        {
            ItemsPerPage = 5;

            ItemsContainer.Direction = FillDirection.Vertical;
        }

        protected override void ShowMore()
        {
            base.ShowMore();

            var req = new GetUserMostPlayedBeatmapsRequest(User.Value.Id, VisiblePages++ * ItemsPerPage);

            req.Success += beatmaps =>
            {
                ShowMoreButton.FadeTo(beatmaps.Count == ItemsPerPage ? 1 : 0);
                ShowMoreLoading.Hide();

                if (!beatmaps.Any() && VisiblePages == 1)
                {
                    MissingText.Show();
                    return;
                }

                MissingText.Hide();

                foreach (var beatmap in beatmaps)
                {
                    ItemsContainer.Add(new DrawableMostPlayedRow(beatmap.GetBeatmapInfo(Rulesets), beatmap.PlayCount));
                }
            };

            Api.Queue(req);
        }
    }
}
