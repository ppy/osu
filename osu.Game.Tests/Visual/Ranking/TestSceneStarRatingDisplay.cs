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
                    new StarRatingDisplay(new StarDifficulty(1.23, 0)),
                    new StarRatingDisplay(new StarDifficulty(2.34, 0)),
                    new StarRatingDisplay(new StarDifficulty(3.45, 0)),
                    new StarRatingDisplay(new StarDifficulty(4.56, 0)),
                    new StarRatingDisplay(new StarDifficulty(5.67, 0)),
                    new StarRatingDisplay(new StarDifficulty(6.78, 0)),
                    new StarRatingDisplay(new StarDifficulty(10.11, 0)),
                }
            };
        }
    }
}
