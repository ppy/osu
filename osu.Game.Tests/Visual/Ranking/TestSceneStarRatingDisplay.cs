// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
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
                ChildrenEnumerable = new[]
                {
                    1.23,
                    2.34,
                    3.45,
                    4.56,
                    5.67,
                    6.78,
                    10.11,
                }.Select(starRating => new StarRatingDisplay(new StarDifficulty(starRating, 0))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                })
            });
        }

        [Test]
        public void TestChangingStarRatingDisplay()
        {
            StarRatingDisplay starRating = null;

            AddStep("load display", () => Child = starRating = new StarRatingDisplay(new StarDifficulty(5.55, 1))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(3f),
            });

            AddRepeatStep("set random value", () =>
            {
                starRating.Current.Value = new StarDifficulty(RNG.NextDouble(0.0, 11.0), 1);
            }, 10);

            AddSliderStep("set exact stars", 0.0, 11.0, 5.55, d =>
            {
                if (starRating != null)
                    starRating.Current.Value = new StarDifficulty(d, 1);
            });
        }
    }
}
