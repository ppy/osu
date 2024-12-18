// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Rankings;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneRankingsSortTabControl : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        public TestSceneRankingsSortTabControl()
        {
            Child = new RankingsSortTabControl
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        }
    }
}
