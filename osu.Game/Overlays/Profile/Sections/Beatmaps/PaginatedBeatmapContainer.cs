// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Direct;
using osu.Game.Users;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Profile.Sections.Beatmaps
{
    public class PaginatedBeatmapContainer : PaginatedContainer
    {
        private readonly BeatmapSetType type;
        private readonly DirectPanelsContainer panelsContainer;

        private UserProfileOverlay overlay;

        public PaginatedBeatmapContainer(BeatmapSetType type, Bindable<User> user, string header, string missing = "None... yet.")
            : base(user, header, missing)
        {
            this.type = type;

            ItemsPerPage = 6;

            Add(panelsContainer = new DirectPanelsContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            });
        }

        [BackgroundDependencyLoader]
        private void load(UserProfileOverlay overlay)
        {
            if (overlay != null)
            {
                this.overlay = overlay;
                overlay.StateChanged += handleVisibilityChanged;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (overlay != null)
                overlay.StateChanged -= handleVisibilityChanged;
        }

        private void handleVisibilityChanged(Visibility visibility)
        {
            if (visibility == Visibility.Hidden && panelsContainer.CurrentPreview != null)
                panelsContainer.CurrentPreview.PreviewPlaying.Value = false;

        }

        protected override void ShowMore()
        {
            base.ShowMore();

            var req = new GetUserBeatmapsRequest(User.Value.Id, type, VisiblePages++ * ItemsPerPage);

            req.Success += sets =>
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

                    panelsContainer.AddPanel(s.ToBeatmapSet(Rulesets));
                }
            };

            Api.Queue(req);
        }
    }
}
