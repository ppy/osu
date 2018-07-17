// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays.Changelog.Header;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseTextBadgePair : OsuTestCase
    {
        private readonly Container container;
        private readonly Box bottomLine;
        private readonly TextBadgePair textBadgePair;

        public TestCaseTextBadgePair()
        {

            Add(container = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new OpenTK.Vector2(300, 40),
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = Color4.Gray,
                        RelativeSizeAxes = Axes.Both,
                    },
                    bottomLine = new Box // purple line
                    {
                        Colour = Color4.Purple,
                        RelativeSizeAxes = Axes.X,
                        Height = 3,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    textBadgePair = new TextBadgePair(Color4.White, "testing")
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                    }
                },
            });
            
            AddStep(@"deactivate", () => textBadgePair.Deactivate());
            AddStep(@"activate", () => textBadgePair.Activate());
            AddStep(@"purple text", () => textBadgePair.SetTextColour(Color4.Purple, 100));
            AddStep(@"white text", () => textBadgePair.SetTextColour(Color4.White, 100));
            AddStep(@"purple badge", () => textBadgePair.SetBadgeColour(Color4.Purple, 100));
            AddStep(@"white badge", () => textBadgePair.SetBadgeColour(Color4.White, 100));
            AddStep(@"hide text", () => textBadgePair.HideText(250));
            AddStep(@"show text", () => textBadgePair.ShowText(250));
        }

        //[BackgroundDependencyLoader]
        //private void load(OsuColour colours)
        //{

        //}

        //private enum BreadcrumbTab
        //{
        //    Click,
        //    The,
        //    Circles,
        //}
    }
}
