// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
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
                    ChildrenEnumerable = Enumerable.Range(0, 10).Select(i => new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(10f),
                        ChildrenEnumerable = Enumerable.Range(0, 10).Select(j =>
                        {
                            var colour = colours.ForStarDifficulty(1f * i + 0.1f * j);

                            return new FillFlowContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0f, 10f),
                                Children = new Drawable[]
                                {
                                    new CircularContainer
                                    {
                                        Masking = true,
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Size = new Vector2(75f, 25f),
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = colour,
                                            },
                                            new OsuSpriteText
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Colour = OsuColour.ForegroundTextColourFor(colour),
                                                Text = colour.ToHex(),
                                            },
                                        }
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Text = $"*{(1f * i + 0.1f * j):0.00}",
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
