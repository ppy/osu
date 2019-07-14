// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Select.Details;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Screens.Select
{
    public class BeatmapDetailArea : Container
    {
        private const float padding = 10;

        public readonly BeatmapDetails Details;
        public readonly BeatmapLeaderboard Leaderboard;
        public readonly UserTopScoreContainer TopScore;

        private WorkingBeatmap beatmap;

        public WorkingBeatmap Beatmap
        {
            get => beatmap;
            set
            {
                beatmap = value;
                Leaderboard.Beatmap = beatmap?.BeatmapInfo;
                Details.Beatmap = beatmap?.BeatmapInfo;
                TopScore.Hide();
            }
        }

        public BeatmapDetailArea()
        {
            Children = new Drawable[]
            {
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
                new Container
                {
                    Padding = new MarginPadding { Top = BeatmapDetailAreaTabControl.HEIGHT, Bottom = padding },
                    RelativeSizeAxes = Axes.Both,
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new Dimension[]
                        {
                            new Dimension(GridSizeMode.Distributed),
                            new Dimension(GridSizeMode.AutoSize),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        Details = new BeatmapDetails
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Alpha = 0,
                                            Padding = new MarginPadding { Top = padding },
                                        },
                                        Leaderboard = new BeatmapLeaderboard
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        }
                                    }
                                }
                            },
                            new Drawable[]
                            {
                                TopScore = new UserTopScoreContainer
                                {
                                    Score = { BindTarget = Leaderboard.TopScore }
                                }
                            }
                        },
                    },
                }
            };
        }
    }
}
