﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
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

                    var panel = new DirectGridPanel(s.ToBeatmapSet(Rulesets));
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
    }
}
