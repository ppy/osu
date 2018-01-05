// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        public readonly Leaderboard Leaderboard;

        private WorkingBeatmap beatmap;
        public WorkingBeatmap Beatmap
        {
            get
            {
                return beatmap;
            }
            set
            {
                beatmap = value;
                Leaderboard.Beatmap = beatmap?.BeatmapInfo;
                Details.Beatmap = beatmap?.BeatmapInfo;
            }
        }

        public BeatmapDetailArea()
        {
            AddRangeInternal(new Drawable[]
            {
                new BeatmapDetailAreaTabControl
                {
                    RelativeSizeAxes = Axes.X,
                    OnFilter = (tab, mods) =>
                    {
                        switch (tab)
                        {
                            case BeatmapDetailTab.Details:
                                Details.Show();
                                Leaderboard.Hide();
                                break;

                            default:
                                Details.Hide();
                                Leaderboard.Scope = (LeaderboardScope)tab - 1;
                                Leaderboard.Show();
                                break;
                        }
                    },
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = BeatmapDetailAreaTabControl.HEIGHT },
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
                Leaderboard = new Leaderboard
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
