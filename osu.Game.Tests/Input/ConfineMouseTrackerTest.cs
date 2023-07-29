// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Input;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Input
{
    [HeadlessTest]
    public partial class ConfineMouseTrackerTest : OsuGameTestScene
    {
        [Resolved]
        private FrameworkConfigManager frameworkConfigManager { get; set; } = null!;

        [TestCase(WindowMode.Windowed)]
        [TestCase(WindowMode.Borderless)]
        public void TestDisableConfining(WindowMode windowMode)
        {
            setWindowModeTo(windowMode);
            setGameSideModeTo(OsuConfineMouseMode.Never);

            setLocalUserPlayingTo(false);
            frameworkSideModeIs(ConfineMouseMode.Never);

            setLocalUserPlayingTo(true);
            frameworkSideModeIs(ConfineMouseMode.Never);
        }

        [TestCase(WindowMode.Windowed)]
        [TestCase(WindowMode.Borderless)]
        public void TestConfiningDuringGameplay(WindowMode windowMode)
        {
            setWindowModeTo(windowMode);
            setGameSideModeTo(OsuConfineMouseMode.DuringGameplay);

            setLocalUserPlayingTo(false);
            frameworkSideModeIs(ConfineMouseMode.Never);

            setLocalUserPlayingTo(true);
            frameworkSideModeIs(ConfineMouseMode.Always);
        }

        [TestCase(WindowMode.Windowed)]
        [TestCase(WindowMode.Borderless)]
        public void TestConfineAlwaysUserSetting(WindowMode windowMode)
        {
            setWindowModeTo(windowMode);
            setGameSideModeTo(OsuConfineMouseMode.Always);

            setLocalUserPlayingTo(false);
            frameworkSideModeIs(ConfineMouseMode.Always);

            setLocalUserPlayingTo(true);
            frameworkSideModeIs(ConfineMouseMode.Always);
        }

        [Test]
        public void TestConfineAlwaysInFullscreen()
        {
            setGameSideModeTo(OsuConfineMouseMode.Never);

            setWindowModeTo(WindowMode.Fullscreen);

            setLocalUserPlayingTo(false);
            frameworkSideModeIs(ConfineMouseMode.Fullscreen);

            setLocalUserPlayingTo(true);
            frameworkSideModeIs(ConfineMouseMode.Fullscreen);

            setWindowModeTo(WindowMode.Windowed);

            // old state is restored
            gameSideModeIs(OsuConfineMouseMode.Never);
            frameworkSideModeIs(ConfineMouseMode.Never);
        }

        private void setWindowModeTo(WindowMode mode)
            // needs to go through .GetBindable().Value instead of .Set() due to default overrides
            => AddStep($"make window {mode}", () => frameworkConfigManager.GetBindable<WindowMode>(FrameworkSetting.WindowMode).Value = mode);

        private void setGameSideModeTo(OsuConfineMouseMode mode)
            => AddStep($"set {mode} game-side", () => Game.LocalConfig.SetValue(OsuSetting.ConfineMouseMode, mode));

        private void setLocalUserPlayingTo(bool playing)
            => AddStep($"local user {(playing ? "playing" : "not playing")}", () => Game.LocalUserPlaying.Value = playing);

        private void gameSideModeIs(OsuConfineMouseMode mode)
            => AddAssert($"mode is {mode} game-side", () => Game.LocalConfig.Get<OsuConfineMouseMode>(OsuSetting.ConfineMouseMode) == mode);

        private void frameworkSideModeIs(ConfineMouseMode mode)
            => AddAssert($"mode is {mode} framework-side", () => frameworkConfigManager.Get<ConfineMouseMode>(FrameworkSetting.ConfineMouseMode) == mode);
    }
}
