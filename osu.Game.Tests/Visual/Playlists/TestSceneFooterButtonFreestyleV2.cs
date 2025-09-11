// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestSceneFooterButtonFreestyleV2 : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        public TestSceneFooterButtonFreestyleV2()
        {
            Add(new FooterButtonFreestyleV2
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.CentreLeft,
                X = -100,
            });
        }
    }
}
