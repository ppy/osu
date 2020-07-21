// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Screens.Ranking.Expanded;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneStarRatingDisplay : OsuTestScene
    {
        public TestSceneStarRatingDisplay()
        {
            Child = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new StarRatingDisplay(new BeatmapInfo { StarDifficulty = 1.23 }),
                    new StarRatingDisplay(new BeatmapInfo { StarDifficulty = 2.34 }),
                    new StarRatingDisplay(new BeatmapInfo { StarDifficulty = 3.45 }),
                    new StarRatingDisplay(new BeatmapInfo { StarDifficulty = 4.56 }),
                    new StarRatingDisplay(new BeatmapInfo { StarDifficulty = 5.67 }),
                    new StarRatingDisplay(new BeatmapInfo { StarDifficulty = 6.78 }),
                    new StarRatingDisplay(new BeatmapInfo { StarDifficulty = 10.11 }),
                }
            };
        }
    }
}
