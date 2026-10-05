// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using osu.Game.Overlays.Dashboard.UserSearch;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneUserSearchDisplay : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        protected override bool UseOnlineAPI => true;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = new UserSearchDisplay();
        });
    }
}
