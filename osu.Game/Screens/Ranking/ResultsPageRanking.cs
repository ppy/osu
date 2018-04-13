// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using OpenTK;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Ranking
{
    public class ResultsPageRanking : ResultsPage
    {
        public ResultsPageRanking(Score score, WorkingBeatmap beatmap = null) : base(score, beatmap)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = colours.GrayE,
                    RelativeSizeAxes = Axes.Both,
                },
                new Leaderboard
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
