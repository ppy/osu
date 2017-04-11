// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Modes.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using OpenTK;

namespace osu.Game.Screens.Ranking
{
    internal class ResultsRankingPage : ResultsPage
    {
        private readonly BeatmapInfo beatmap;

        public ResultsRankingPage(Score score, BeatmapInfo beatmap = null) : base(score)
        {
            this.beatmap = beatmap;
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
                    Beatmap = beatmap ?? Score.Beatmap,
                    Scale = new Vector2(0.7f)
                }
            };
        }
    }
}