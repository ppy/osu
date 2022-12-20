// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
    public partial class TestSceneStarRatingDisplay : OsuTestScene
    {
        [TestCase(StarRatingDisplaySize.Regular)]
        [TestCase(StarRatingDisplaySize.Small)]
        public void TestDisplay(StarRatingDisplaySize size)
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
                    ChildrenEnumerable = Enumerable.Range(-1, 15).Select(i => new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Spacing = new Vector2(2f),
                        Direction = FillDirection.Vertical,
                        ChildrenEnumerable = Enumerable.Range(0, 10).Select(j => new StarRatingDisplay(new StarDifficulty(i * (i >= 11 ? 25f : 1f) + j * 0.1f, 0), size)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }),
                    })
                };
            });
        }

        [Test]
        public void TestSpectrum()
        {
            StarRatingDisplay starRating = null;

            AddStep("load display", () => Child = starRating = new StarRatingDisplay(new StarDifficulty(5.55, 1), animated: true)
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
