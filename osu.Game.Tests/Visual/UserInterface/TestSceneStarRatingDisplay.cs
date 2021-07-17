// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneStarRatingDisplay : OsuTestScene
    {
        [Test]
        public void TestOldColoursDisplay()
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

        [TestCase(52f, 20f)]
        [TestCase(52f, 16f)]
        [TestCase(50f, 14f)]
        public void TestNewColoursDisplay(float width, float height)
        {
            AddStep("load displays", () =>
            {
                Child = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(2f),
                    Direction = FillDirection.Horizontal,
                    ChildrenEnumerable = Enumerable.Range(0, 10).Select(i => new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Spacing = new Vector2(2f),
                        Direction = FillDirection.Vertical,
                        ChildrenEnumerable = Enumerable.Range(0, 10).Select(j => new StarRatingDisplay(new StarDifficulty(i + j * 0.1f, 0), true)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(width, height),
                        })
                    })
                };
            });
        }

        [Test]
        public void TestChangingStarRatingDisplay([Values(false, true)] bool useNewColours)
        {
            StarRatingDisplay starRating = null;

            AddStep("load display", () => Child = starRating = new StarRatingDisplay(new StarDifficulty(5.55, 1), useNewColours)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(52f, 20f),
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
