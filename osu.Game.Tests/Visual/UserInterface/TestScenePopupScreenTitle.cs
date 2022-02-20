// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestScenePopupScreenTitle : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Test]
        public void TestPopupScreenTitle()
        {
            AddStep("create content", () =>
            {
                Child = new PopupScreenTitle
                {
                    Title = "Popup Screen Title",
                    Description = string.Join(" ", Enumerable.Repeat("This is a description.", 20)),
                    Close = () => { }
                };
            });
        }

        [Test]
        public void TestDisabledExit()
        {
            AddStep("create content", () =>
            {
                Child = new PopupScreenTitle
                {
                    Title = "Popup Screen Title",
                    Description = "This is a description."
                };
            });
        }
    }
}
