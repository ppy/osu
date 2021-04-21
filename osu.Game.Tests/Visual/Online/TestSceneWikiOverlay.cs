// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneWikiOverlay : OsuTestScene
    {
        private WikiOverlay wiki;

        [SetUp]
        public void SetUp() => Schedule(() => Child = wiki = new WikiOverlay());

        [Test]
        public void TestOverlay()
        {
            AddStep("Show", () => wiki.Show());
        }
    }
}
