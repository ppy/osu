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
    public class TestSceneStarRatingDisplayV2 : OsuTestScene
    {
        [Test]
        public void TestDisplay()
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
                        ChildrenEnumerable = Enumerable.Range(0, 10).Select(j => new StarRatingDisplayV2(new StarDifficulty(i + j * 0.1f, 0))
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        })
                    })
                };
            });
        }

        [Test]
        public void TestChangingStarRatingDisplay()
        {
            StarRatingDisplayV2 starRating = null;

            AddStep("load display", () => Child = starRating = new StarRatingDisplayV2(new StarDifficulty(5.55, 1))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
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
