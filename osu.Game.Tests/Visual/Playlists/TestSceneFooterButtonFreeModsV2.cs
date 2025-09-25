// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestSceneFooterButtonFreeModsV2 : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private readonly FooterButtonFreeModsV2 button;

        public TestSceneFooterButtonFreeModsV2()
        {
            ModSelectOverlay modSelectOverlay;
            Add(modSelectOverlay = new TestModSelectOverlay());
            Add(button = new FooterButtonFreeModsV2(modSelectOverlay)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.CentreLeft,
                X = -100,
            });
        }

        [Test]
        public void TestAllMods()
        {
            AddStep("all mods", () => button.FreeMods.Value = new OsuRuleset().CreateAllMods().ToArray());
        }

        [Test]
        public void TestNoMods()
        {
            AddStep("no mods", () => button.FreeMods.Value = []);
        }

        [Test]
        public void TestFreestyle()
        {
            AddToggleStep("toggle freestyle", v => button.Freestyle.Value = v);
        }

        private partial class TestModSelectOverlay : UserModSelectOverlay
        {
            public TestModSelectOverlay()
                : base(OverlayColourScheme.Aquamarine)
            {
                IsValidMod = _ => true;
            }
        }
    }
}
