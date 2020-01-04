// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osuTK;

namespace osu.Game.Screens.Ranking.Pages
{
    public class LocalLeaderboardPage : ResultsPage
    {
        public LocalLeaderboardPage(ScoreInfo score, WorkingBeatmap beatmap = null)
            : base(score, beatmap)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Gray6,
                    RelativeSizeAxes = Axes.Both,
                },
                new BeatmapLeaderboard
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Beatmap = Beatmap.BeatmapInfo ?? Score.Beatmap,
                    Scale = new Vector2(0.7f)
                }
            };
        }
    }
}
