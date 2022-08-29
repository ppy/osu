// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;
using System.Linq;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneGrowToFitContent : OsuGridTestScene
    {
        private readonly List<Container> parentContainers = new List<Container>();
        private readonly List<UprightAspectMaintainingContainer> childContainers = new List<UprightAspectMaintainingContainer>();
        private readonly List<OsuSpriteText> texts = new List<OsuSpriteText>();

        public TestSceneGrowToFitContent()
            : base(1, 2)
        {
            for (int i = 0; i < 2; i++)
            {
                OsuSpriteText text;
                UprightAspectMaintainingContainer childContainer;
                Container parentContainer = new Container
                {
                    Origin = Anchor.BottomRight,
                    Anchor = Anchor.BottomCentre,
                    AutoSizeAxes = Axes.Both,
                    Rotation = 45,
                    Y = -200,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.Red,
                        },
                        childContainer = new UprightAspectMaintainingContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colour4.Blue,
                                },
                                text = new OsuSpriteText
                                {
                                    Text = "Text",
                                    Font = OsuFont.GetFont(Typeface.Venera, weight: FontWeight.Bold, size: 40),
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.Centre,
                                },
                            }
                        },
                    }
                };

                Container cellInfo = new Container
                {
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Margin = new MarginPadding
                    {
                        Top = 100,
                    },
                    Child = new OsuSpriteText
                    {
                        Text = (i == 0) ? "GrowToFitContent == true" : "GrowToFitContent == false",
                        Font = OsuFont.GetFont(Typeface.Inter, weight: FontWeight.Bold, size: 40),
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                    },
                };

                parentContainers.Add(parentContainer);
                childContainers.Add(childContainer);
                texts.Add(text);
                Cell(i).Add(cellInfo);
                Cell(i).Add(parentContainer);
            }
        }

        [Test]
        public void TestResizeText()
        {
            AddStep("reset...", () =>
            {
                childContainers[0].GrowToFitContent = false;
                childContainers[1].GrowToFitContent = false;
            });

            AddStep("setup...", () =>
            {
                childContainers[0].GrowToFitContent = true;
                childContainers[1].GrowToFitContent = false;
            });

            for (int i = 0; i < 10; i++)
            {
                AddStep("Add Character", () =>
                {
                    foreach (int j in Enumerable.Range(0, parentContainers.Count))
                    {
                        texts[j].Text += ".";
                    }
                });
            }

            for (int i = 0; i < 10; i++)
            {
                AddStep("Remove Character", () =>
                {
                    foreach (int j in Enumerable.Range(0, parentContainers.Count))
                    {
                        string text = texts[j].Text.ToString();
                        texts[j].Text = text.Remove(text.Length - 1, 1);
                    }
                });
            }
        }

        [Test]
        public void TestScaleText()
        {
            AddStep("reset...", () =>
            {
                childContainers[0].GrowToFitContent = false;
                childContainers[1].GrowToFitContent = false;
            });

            AddStep("setup...", () =>
            {
                childContainers[0].GrowToFitContent = true;
                childContainers[1].GrowToFitContent = false;
            });

            for (int i = 0; i < 1; i++)
            {
                AddStep("Big text", scaleUp);

                AddWaitStep("wait...", 5);

                AddStep("Small text", scaleDown);
            }
        }

        private void scaleUp()
        {
            foreach (int j in Enumerable.Range(0, parentContainers.Count))
            {
                texts[j].ScaleTo(new Vector2(2, 2), 1000);
            }
        }

        private void scaleDown()
        {
            foreach (int j in Enumerable.Range(0, parentContainers.Count))
            {
                texts[j].ScaleTo(new Vector2(1, 1), 1000);
            }
        }
    }
}
