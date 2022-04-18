// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Navigation
{
    public class TestFirstRunSetup : OsuTestScene
    {
        [Test]
        public void TestOverlay()
        {
            AddStep("add overlay", () =>
            {
                Child = new FirstRunSetupOverlay();
            });
        }
    }
}
