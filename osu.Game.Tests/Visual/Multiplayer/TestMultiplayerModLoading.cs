// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.
using System;
using NUnit.Framework;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestMultiplayerModLoading : OsuGameTestScene
    {
        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            SelectedMods.Value = Array.Empty<Mod>();
        });

        /// <summary>
        /// This is a regression test that tests whether a singleplayer mod can transfer over to a multiplayer screen.
        /// It should not carry over from these screens, prevents regression on https://github.com/ppy/osu/pull/17352 
        /// </summary>
        [Test]
        public void TestSingleplayerModsDontCarryToMultiplayerScreens()
        {
            PushAndConfirm(() => new PlaySongSelect());

            // Select Mods while a "singleplayer" screen is active
            var osuAutomationMod = new OsuModAutoplay();
            var expectedMods = new[] { osuAutomationMod };

            AddStep("Toggle on the automation Mod.", () => { SelectedMods.Value = expectedMods; });
            AddAssert("Mods are loaded before the multiplayer screen is pushed.", () => SelectedMods.Value == expectedMods);

            PushAndConfirm(() => new TestMultiplayerComponents());
            AddAssert("Mods are Empty After A Multiplayer Screen Loads", () => SelectedMods.Value.Count == 0);

            AddStep("Retoggle on the automation Mod.", () => { SelectedMods.Value = expectedMods; });
            AddAssert("Mods are loaded before the playlist screen is pushed", () => SelectedMods.Value == expectedMods);

            // TODO: Implement TestPlaylistComponents? 
            //PushAndConfirm(() => new TestPlaylistComponents());
            //AddAssert("Mods are Empty After Playlist Screen Loads", () => !SelectedMods.Value.Any());
        }
    }
}
