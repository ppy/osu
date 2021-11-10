// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Background
{
    [TestFixture]
    public class TestSceneBackgroundScreenDefault : OsuTestScene
    {
        private BackgroundScreenStack stack;
        private BackgroundScreenDefault screen;

        private Graphics.Backgrounds.Background getCurrentBackground() => screen.ChildrenOfType<Graphics.Backgrounds.Background>().FirstOrDefault();

        [Resolved]
        private SkinManager skins { get; set; }

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
        public void TestBackgroundTypeSwitch()
        {
            setSupporter(true);

            setSourceMode(BackgroundSource.Beatmap);
            AddUntilStep("is beatmap background", () => getCurrentBackground() is BeatmapBackground);

            setSourceMode(BackgroundSource.BeatmapWithStoryboard);
            AddUntilStep("is storyboard background", () => getCurrentBackground() is BeatmapBackgroundWithStoryboard);

            setSourceMode(BackgroundSource.Skin);
            AddUntilStep("is default background", () => getCurrentBackground().GetType() == typeof(Graphics.Backgrounds.Background));

            setCustomSkin();
            AddUntilStep("is skin background", () => getCurrentBackground() is SkinBackground);
        }

        [Test]
        public void TestTogglingSupporterTogglesBeatmapBackground()
        {
            setSourceMode(BackgroundSource.Beatmap);

            setSupporter(true);
            AddUntilStep("is beatmap background", () => getCurrentBackground() is BeatmapBackground);

            setSupporter(false);
            AddUntilStep("is default background", () => !(getCurrentBackground() is BeatmapBackground));

            setSupporter(true);
            AddUntilStep("is beatmap background", () => getCurrentBackground() is BeatmapBackground);
        }

        [TestCase(BackgroundSource.Beatmap, typeof(BeatmapBackground))]
        [TestCase(BackgroundSource.BeatmapWithStoryboard, typeof(BeatmapBackgroundWithStoryboard))]
        [TestCase(BackgroundSource.Skin, typeof(SkinBackground))]
        public void TestBackgroundDoesntReloadOnNoChange(BackgroundSource source, Type backgroundType)
        {
            Graphics.Backgrounds.Background last = null;

            setSourceMode(source);
            setSupporter(true);
            if (source == BackgroundSource.Skin)
                setCustomSkin();

            AddUntilStep("wait for beatmap background to be loaded", () => (last = getCurrentBackground())?.GetType() == backgroundType);
            AddAssert("next doesn't load new background", () => screen.Next() == false);

            // doesn't really need to be checked but might as well.
            AddWaitStep("wait a bit", 5);
            AddUntilStep("ensure same background instance", () => last == getCurrentBackground());
        }

        [Test]
        public void TestBackgroundCyclingOnDefaultSkin([Values] bool supporter)
        {
            Graphics.Backgrounds.Background last = null;

            setSourceMode(BackgroundSource.Skin);
            setSupporter(supporter);
            setDefaultSkin();

            AddUntilStep("wait for beatmap background to be loaded", () => (last = getCurrentBackground())?.GetType() == typeof(Graphics.Backgrounds.Background));
            AddAssert("next cycles background", () => screen.Next());

            // doesn't really need to be checked but might as well.
            AddWaitStep("wait a bit", 5);
            AddUntilStep("ensure different background instance", () => last != getCurrentBackground());
        }

        private void setSourceMode(BackgroundSource source) =>
            AddStep($"set background mode to {source}", () => config.SetValue(OsuSetting.MenuBackgroundSource, source));

        private void setSupporter(bool isSupporter) =>
            AddStep($"set supporter {isSupporter}", () => ((DummyAPIAccess)API).LocalUser.Value = new APIUser
            {
                IsSupporter = isSupporter,
                Id = API.LocalUser.Value.Id + 1,
            });

        private void setCustomSkin()
        {
            // feign a skin switch. this doesn't do anything except force CurrentSkin to become a LegacySkin.
            AddStep("set custom skin", () => skins.CurrentSkinInfo.Value = new SkinInfo { ID = 5 });
        }

        private void setDefaultSkin() => AddStep("set default skin", () => skins.CurrentSkinInfo.SetDefault());

        [TearDownSteps]
        public void TearDown() => setDefaultSkin();
    }
}
