// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays.Changelog.Header;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseTextBadgePair : OsuTestCase
    {
        public TestCaseTextBadgePair()
        {
            Breadcrumb breadcrumb;

            Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 250,
                Height = 50,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = Color4.Gray,
                        Alpha = 0.5f,
                        RelativeSizeAxes = Axes.Both,
                    },
                    breadcrumb = new TestBadgePair(Color4.DeepSkyBlue, "Test")
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                }
            });

            AddStep(@"Deactivate", breadcrumb.Deactivate);
            AddStep(@"Activate", breadcrumb.Activate);
            AddStep(@"Hide text", () => breadcrumb.HideText(200));
            AddStep(@"Show text", () => breadcrumb.ShowText(200));
            AddStep(@"Different text", () => breadcrumb.ShowText(200, "This one's a little bit wider"));
            AddStep(@"Different text", () => breadcrumb.ShowText(200, "Ok?.."));
        }

        private class TestBadgePair : Breadcrumb
        {
            public TestBadgePair(ColourInfo badgeColour, string displayText = "Listing", bool startCollapsed = true)
                : base(badgeColour, displayText, startCollapsed)
            {
            }
        }
    }
}
