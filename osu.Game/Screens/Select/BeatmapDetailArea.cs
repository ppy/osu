// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Screens.Select
{
    public class BeatmapDetailArea : Container
    {
        private const float details_padding = 10;

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        public readonly BeatmapDetails Details;
        public readonly BeatmapLeaderboard Leaderboard;

        private WorkingBeatmap beatmap;

        public WorkingBeatmap Beatmap
        {
            get => beatmap;
            set
            {
                beatmap = value;
                Details.Beatmap = beatmap?.BeatmapInfo;
                Leaderboard.Beatmap = beatmap is DummyWorkingBeatmap ? null : beatmap?.BeatmapInfo;
            }
        }

        public BeatmapDetailArea()
        {
            AddRangeInternal(new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = BeatmapDetailAreaTabControl.HEIGHT },
                },
                new BeatmapDetailAreaTabControl
                {
                    RelativeSizeAxes = Axes.X,
                    OnFilter = (tab, mods) =>
                    {
                        Leaderboard.FilterMods = mods;

                        switch (tab)
                        {
                            case BeatmapDetailTab.Details:
                                Details.Show();
                                Leaderboard.Hide();
                                break;

                            default:
                                Details.Hide();
                                Leaderboard.Scope = (BeatmapLeaderboardScope)tab - 1;
                                Leaderboard.Show();
                                break;
                        }
                    },
                },
            });

            AddRange(new Drawable[]
            {
                Details = new BeatmapDetails
                {
                    RelativeSizeAxes = Axes.X,
                    Alpha = 0,
                    Margin = new MarginPadding { Top = details_padding },
                },
                Leaderboard = new BeatmapLeaderboard
                {
                    RelativeSizeAxes = Axes.Both,
                }
            });
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            Details.Height = Math.Min(DrawHeight - details_padding * 3 - BeatmapDetailAreaTabControl.HEIGHT, 450);
        }
    }
}
