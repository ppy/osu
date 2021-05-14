// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Screens.Ranking.Expanded;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneStarRatingDisplay : OsuTestScene
    {
        [Test]
        public void TestDisplay()
        {
            AddStep("load displays", () => Child = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new StarRatingDisplay(new StarDifficulty(1.23, 0)) { Anchor = Anchor.Centre, Origin = Anchor.Centre },
                    new StarRatingDisplay(new StarDifficulty(2.34, 0)) { Anchor = Anchor.Centre, Origin = Anchor.Centre },
                    new StarRatingDisplay(new StarDifficulty(3.45, 0)) { Anchor = Anchor.Centre, Origin = Anchor.Centre },
                    new StarRatingDisplay(new StarDifficulty(4.56, 0)) { Anchor = Anchor.Centre, Origin = Anchor.Centre },
                    new StarRatingDisplay(new StarDifficulty(5.67, 0)) { Anchor = Anchor.Centre, Origin = Anchor.Centre },
                    new StarRatingDisplay(new StarDifficulty(6.78, 0)) { Anchor = Anchor.Centre, Origin = Anchor.Centre },
                    new StarRatingDisplay(new StarDifficulty(10.11, 0)) { Anchor = Anchor.Centre, Origin = Anchor.Centre },
                }
            });
        }

        [Test]
        public void TestNullStarRatingDisplay()
        {
            AddStep("load null", () => Child = new StarRatingDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(3f),
            });
        }

        [Test]
        public void TestChangingStarRatingDisplay()
        {
            StarRatingDisplay starRating = null;

            AddStep("load display", () => Child = starRating = new StarRatingDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(3f),
            });

            AddRepeatStep("change display value", () =>
            {
                starRating.Current.Value = new StarDifficulty(RNG.NextDouble(0.0, 11.0), RNG.Next(2000));
            }, 10);
        }
    }
}
