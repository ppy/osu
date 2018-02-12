// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Direct;
using osu.Game.Users;
using System.Linq;

namespace osu.Game.Overlays.Profile.Sections.Beatmaps
{
    public class PaginatedBeatmapContainer : PaginatedContainer
    {
        private const float panel_padding = 10f;

        private readonly BeatmapSetType type;

        private DirectPanel currentlyPlaying;
        private BeatmapManager manager;

        public PaginatedBeatmapContainer(BeatmapSetType type, Bindable<User> user, string header, string missing = "None... yet.")
            : base(user, header, missing)
        {
            this.type = type;

            ItemsPerPage = 6;

            ItemsContainer.Spacing = new Vector2(panel_padding);
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager manager)
        {
            this.manager = manager;

            manager.BeatmapSetAdded += set =>
            {
                var displayedSet = ItemsContainer.Children.OfType<GridPanel>().FirstOrDefault(p => p.SetInfo.OnlineBeatmapSetID == set.OnlineBeatmapSetID);

                displayedSet?.FadeOutDownloadIndicators();
            };
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

                    bool beatmapExists = manager.QueryBeatmapSet(b => b.OnlineBeatmapSetID == s.OnlineBeatmapSetID) != null;

                    var panel = new GridPanel(s.ToBeatmapSet(Rulesets), !beatmapExists);
                    ItemsContainer.Add(panel);

                    panel.PreviewPlaying.ValueChanged += isPlaying =>
                    {
                        if (!isPlaying) return;

                        if (currentlyPlaying != null && currentlyPlaying != panel)
                            currentlyPlaying.PreviewPlaying.Value = false;

                        currentlyPlaying = panel;
                    };
                }
            };

            Api.Queue(req);
        }

        private class GridPanel : DirectGridPanel
        {
            private const double fade_out_duration = 200;

            private bool downloadIndicatorsVisible;

            public GridPanel(BeatmapSetInfo value, bool downloadIndicatorsVisible = true) : base(value)
            {
                this.downloadIndicatorsVisible = downloadIndicatorsVisible;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                if (!downloadIndicatorsVisible)
                    DownloadButton.Alpha = 0;
            }

            protected override bool OnClick(InputState state)
            {
                ShowInformation(downloadIndicatorsVisible);
                PreviewPlaying.Value = false;
                return true;
            }

            public void FadeOutDownloadIndicators()
            {
                downloadIndicatorsVisible = false;
                DownloadButton.FadeOut(fade_out_duration).Expire();
                ProgressBar.FadeOut(fade_out_duration).Expire();
            }
        }
    }
}
