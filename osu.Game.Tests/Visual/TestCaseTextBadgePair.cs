// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays.Changelog.Header;

namespace osu.Game.Tests.Visual
{
    public class TestCaseTextBadgePair : OsuTestCase
    {
        public TestCaseTextBadgePair()
        {
            Container container;
            TextBadgePair textBadgePair;

            Add(container = new Container
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
                    textBadgePair = new TextBadgePair(Color4.DeepSkyBlue, "Test")
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                }
            });

            AddStep(@"Deactivate", textBadgePair.Deactivate);
            AddStep(@"Activate", textBadgePair.Activate);
            AddStep(@"Hide text", () => textBadgePair.HideText(200));
            AddStep(@"Show text", () => textBadgePair.ShowText(200));
            AddStep(@"Different text", () => textBadgePair.ChangeText(200, "This one's a little bit wider"));
            AddWaitStep(1);
            AddStep(@"Different text", () => textBadgePair.ChangeText(200, "Ok?.."));
        }
    }
}
