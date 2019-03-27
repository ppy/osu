// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Direct;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Sections.Beatmaps
{
    public class PaginatedBeatmapContainer : PaginatedContainer
    {
        private const float panel_padding = 10f;
        private readonly BeatmapSetType type;
        private GetUserBeatmapsRequest request;

        public PaginatedBeatmapContainer(BeatmapSetType type, Bindable<User> user, string header, string missing = "None... yet.")
            : base(user, header, missing)
        {
            this.type = type;

            ItemsPerPage = 6;

            ItemsContainer.Spacing = new Vector2(panel_padding);
        }

        protected override void ShowMore()
        {
            base.ShowMore();

            request = new GetUserBeatmapsRequest(User.Value.Id, type, VisiblePages++ * ItemsPerPage);
            request.Success += sets => Schedule(() =>
            {
                ShowMoreButton.FadeTo(sets.Count == ItemsPerPage ? 1 : 0);
                ShowMoreLoading.Hide();

                if (!sets.Any() && VisiblePages == 1)
                {
                    MissingText.Show();
                    return;
                }

                foreach (var s in sets)
                {
                    if (!s.OnlineBeatmapSetID.HasValue)
                        continue;

                    var panel = new DirectGridPanel(s.ToBeatmapSet(Rulesets));
                    ItemsContainer.Add(panel);
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
