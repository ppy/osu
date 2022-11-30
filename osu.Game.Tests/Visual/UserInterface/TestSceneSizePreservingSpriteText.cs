// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneSizePreservingSpriteText : OsuGridTestScene
    {
        private readonly List<Container> parentContainers = new List<Container>();
        private readonly List<UprightAspectMaintainingContainer> childContainers = new List<UprightAspectMaintainingContainer>();
        private readonly OsuSpriteText osuSpriteText = new OsuSpriteText();
        private readonly SizePreservingSpriteText sizePreservingSpriteText = new SizePreservingSpriteText();

        public TestSceneSizePreservingSpriteText()
            : base(1, 2)
        {
            for (int i = 0; i < 2; i++)
            {
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
                        Text = (i == 0) ? "OsuSpriteText" : "SizePreservingSpriteText",
                        Font = OsuFont.GetFont(Typeface.Inter, weight: FontWeight.Bold, size: 40),
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                    },
                };

                parentContainers.Add(parentContainer);
                childContainers.Add(childContainer);
                Cell(i).Add(cellInfo);
                Cell(i).Add(parentContainer);
            }

            childContainers[0].Add(osuSpriteText);
            childContainers[1].Add(sizePreservingSpriteText);
            osuSpriteText.Font = sizePreservingSpriteText.Font = OsuFont.GetFont(Typeface.Venera, weight: FontWeight.Bold, size: 20);
        }

        protected override void Update()
        {
            base.Update();
            osuSpriteText.Text = sizePreservingSpriteText.Text = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }
    }
}
