// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Wiki;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneWikiHeader : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Orange);

        public TestSceneWikiHeader()
        {
            Child = new WikiHeader
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        }
    }
}
