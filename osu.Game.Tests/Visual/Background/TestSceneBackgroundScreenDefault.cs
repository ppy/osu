// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Online.API;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Background
{
    [TestFixture]
    public class TestSceneBackgroundScreenDefault : OsuTestScene
    {
        private BackgroundScreenStack stack;
        private BackgroundScreenDefault screen;

        private Graphics.Backgrounds.Background getCurrentBackground() => screen.ChildrenOfType<Graphics.Backgrounds.Background>().FirstOrDefault();

        [Resolved]
        private OsuConfigManager config { get; set; }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create background stack", () => Child = stack = new BackgroundScreenStack());
            AddStep("push default screen", () => stack.Push(screen = new BackgroundScreenDefault(false)));
            AddUntilStep("wait for screen to load", () => screen.IsCurrentScreen());
        }

        [Test]
        public void TestBeatmapDoesntReloadOnNoChange()
        {
            BeatmapBackground last = null;

            setSourceMode(BackgroundSource.Beatmap);
            setSupporter(true);

            AddUntilStep("wait for beatmap background to be loaded", () => (last = getCurrentBackground() as BeatmapBackground) != null);
            AddAssert("next doesn't load new background", () => screen.Next() == false);

            // doesn't really need to be checked but might as well.
            AddWaitStep("wait a bit", 5);
            AddUntilStep("ensure same background instance", () => last == getCurrentBackground());
        }

        private void setSourceMode(BackgroundSource source) =>
            AddStep("set background mode to beatmap", () => config.SetValue(OsuSetting.MenuBackgroundSource, source));

        private void setSupporter(bool isSupporter) =>
            AddStep("set supporter", () => ((DummyAPIAccess)API).LocalUser.Value = new User { IsSupporter = isSupporter });
    }
}
