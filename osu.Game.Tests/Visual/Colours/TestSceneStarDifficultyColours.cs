// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Tests.Visual.Colours
{
    public partial class TestSceneStarDifficultyColours : OsuTestScene
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Test]
        public void TestColours()
        {
            AddStep("load colour displays", () =>
            {
                Child = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5f),
                    ChildrenEnumerable = Enumerable.Range(0, 15).Select(i => new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(10f),
                        ChildrenEnumerable = Enumerable.Range(0, 10).Select(j =>
                        {
                            float difficulty = 1f * i + 0.1f * j;
                            var colour = colours.ForStarDifficulty(difficulty);
                            var textColour = colours.ForStarDifficultyText(difficulty);

                            return new FillFlowContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0f, 5f),
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Font = FontUsage.Default.With(size: 10),
                                        Text = $"BG: {colour.ToHex()}",
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Font = FontUsage.Default.With(size: 10),
                                        Text = $"Text: {textColour.ToHex()}",
                                    },
                                    new StarRatingDisplay(new StarDifficulty(difficulty, 0))
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                    }
                                }
                            };
                        })
                    })
                };
            });
        }
    }
}
