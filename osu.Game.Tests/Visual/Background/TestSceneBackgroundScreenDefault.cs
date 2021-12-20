// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics.Textures;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Skinning;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.Background
{
    [TestFixture]
    public class TestSceneBackgroundScreenDefault : OsuTestScene
    {
        private BackgroundScreenStack stack;
        private TestBackgroundScreenDefault screen;
        private Graphics.Backgrounds.Background getCurrentBackground() => screen.ChildrenOfType<Graphics.Backgrounds.Background>().FirstOrDefault();

        [Resolved]
        private SkinManager skins { get; set; }

        [Resolved]
        private OsuConfigManager config { get; set; }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create background stack", () => Child = stack = new BackgroundScreenStack());
            AddStep("push default screen", () => stack.Push(screen = new TestBackgroundScreenDefault()));
            AddUntilStep("wait for screen to load", () => screen.IsCurrentScreen());
        }

        [Test]
        public void TestBeatmapBackgroundTracksBeatmap()
        {
            setSupporter(true);
            setSourceMode(BackgroundSource.Beatmap);

            AddStep("change beatmap", () => Beatmap.Value = createTestWorkingBeatmapWithUniqueBackground());
            AddAssert("background changed", () => screen.CheckLastLoadChange() == true);

            Graphics.Backgrounds.Background last = null;

            AddUntilStep("wait for beatmap background to be loaded", () => getCurrentBackground()?.GetType() == typeof(BeatmapBackground));
            AddStep("store background", () => last = getCurrentBackground());

            AddStep("change beatmap", () => Beatmap.Value = createTestWorkingBeatmapWithUniqueBackground());

            AddUntilStep("wait for beatmap background to change", () => screen.CheckLastLoadChange() == true);

            AddUntilStep("background is new beatmap background", () => last != getCurrentBackground());
            AddStep("store background", () => last = getCurrentBackground());

            AddStep("change beatmap", () => Beatmap.Value = createTestWorkingBeatmapWithUniqueBackground());

            AddUntilStep("wait for beatmap background to change", () => screen.CheckLastLoadChange() == true);
            AddUntilStep("background is new beatmap background", () => last != getCurrentBackground());
        }

        [Test]
        public void TestBeatmapBackgroundTracksBeatmapWhenSuspended()
        {
            setSupporter(true);
            setSourceMode(BackgroundSource.Beatmap);

            AddStep("change beatmap", () => Beatmap.Value = createTestWorkingBeatmapWithUniqueBackground());
            AddAssert("background changed", () => screen.CheckLastLoadChange() == true);
            AddUntilStep("wait for beatmap background to be loaded", () => getCurrentBackground()?.GetType() == typeof(BeatmapBackground));

            BackgroundScreenBeatmap nestedScreen = null;

            // of note, this needs to be a type that doesn't match BackgroundScreenDefault else it is silently not pushed by the background stack.
            AddStep("push new background to stack", () => stack.Push(nestedScreen = new BackgroundScreenBeatmap(Beatmap.Value)));
            AddUntilStep("wait for screen to load", () => nestedScreen.IsLoaded && nestedScreen.IsCurrentScreen());
            AddUntilStep("previous background hidden", () => !screen.IsAlive);

            AddAssert("top level background hasn't changed yet", () => screen.CheckLastLoadChange() == null);

            AddStep("change beatmap", () => Beatmap.Value = createTestWorkingBeatmapWithUniqueBackground());

            AddAssert("top level background hasn't changed yet", () => screen.CheckLastLoadChange() == null);

            AddStep("pop screen back to top level", () => screen.MakeCurrent());

            AddAssert("top level background changed", () => screen.CheckLastLoadChange() == true);
        }

        [Test]
        public void TestBeatmapBackgroundIgnoresNoChangeWhenSuspended()
        {
            BackgroundScreenBeatmap nestedScreen = null;
            WorkingBeatmap originalWorking = null;

            setSupporter(true);
            setSourceMode(BackgroundSource.Beatmap);

            AddStep("change beatmap", () => originalWorking = Beatmap.Value = createTestWorkingBeatmapWithUniqueBackground());
            AddAssert("background changed", () => screen.CheckLastLoadChange() == true);
            AddUntilStep("wait for beatmap background to be loaded", () => getCurrentBackground()?.GetType() == typeof(BeatmapBackground));

            // of note, this needs to be a type that doesn't match BackgroundScreenDefault else it is silently not pushed by the background stack.
            AddStep("push new background to stack", () => stack.Push(nestedScreen = new BackgroundScreenBeatmap(Beatmap.Value)));
            AddUntilStep("wait for screen to load", () => nestedScreen.IsLoaded && nestedScreen.IsCurrentScreen());

            // we're testing a case where scheduling may be used to avoid issues, so ensure the scheduler is no longer running.
            AddUntilStep("wait for top level not alive", () => !screen.IsAlive);

            AddStep("change beatmap", () => Beatmap.Value = createTestWorkingBeatmapWithUniqueBackground());
            AddStep("change beatmap back", () => Beatmap.Value = originalWorking);

            AddAssert("top level background hasn't changed yet", () => screen.CheckLastLoadChange() == null);

            AddStep("pop screen back to top level", () => screen.MakeCurrent());

            AddStep("top level screen is current", () => screen.IsCurrentScreen());
            AddAssert("top level background reused existing", () => screen.CheckLastLoadChange() == false);
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
            setSourceMode(source);
            setSupporter(true);
            if (source == BackgroundSource.Skin)
                setCustomSkin();

            AddUntilStep("wait for beatmap background to be loaded", () => (getCurrentBackground())?.GetType() == backgroundType);
            AddAssert("next doesn't load new background", () => screen.Next() == false);
        }

        [Test]
        public void TestBackgroundCyclingOnDefaultSkin([Values] bool supporter)
        {
            setSourceMode(BackgroundSource.Skin);
            setSupporter(supporter);
            setDefaultSkin();

            AddUntilStep("wait for beatmap background to be loaded", () => (getCurrentBackground())?.GetType() == typeof(Graphics.Backgrounds.Background));
            AddAssert("next cycles background", () => screen.Next());
        }

        private void setSourceMode(BackgroundSource source) =>
            AddStep($"set background mode to {source}", () => config.SetValue(OsuSetting.MenuBackgroundSource, source));

        private void setSupporter(bool isSupporter) =>
            AddStep($"set supporter {isSupporter}", () => ((DummyAPIAccess)API).LocalUser.Value = new APIUser
            {
                IsSupporter = isSupporter,
                Id = API.LocalUser.Value.Id + 1,
            });

        private WorkingBeatmap createTestWorkingBeatmapWithUniqueBackground() => new UniqueBackgroundTestWorkingBeatmap(Audio);

        private class TestBackgroundScreenDefault : BackgroundScreenDefault
        {
            private bool? lastLoadTriggerCausedChange;

            public TestBackgroundScreenDefault()
                : base(false)
            {
            }

            public override bool Next()
            {
                bool didChange = base.Next();
                lastLoadTriggerCausedChange = didChange;
                return didChange;
            }

            public bool? CheckLastLoadChange()
            {
                bool? lastChange = lastLoadTriggerCausedChange;
                lastLoadTriggerCausedChange = null;
                return lastChange;
            }
        }

        private class UniqueBackgroundTestWorkingBeatmap : TestWorkingBeatmap
        {
            public UniqueBackgroundTestWorkingBeatmap(AudioManager audioManager)
                : base(new Beatmap(), null, audioManager)
            {
            }

            protected override Texture GetBackground() => new Texture(1, 1);
        }

        private void setCustomSkin()
        {
            // feign a skin switch. this doesn't do anything except force CurrentSkin to become a LegacySkin.
            AddStep("set custom skin", () => skins.CurrentSkinInfo.Value = new SkinInfo().ToLiveUnmanaged());
        }

        private void setDefaultSkin() => AddStep("set default skin", () => skins.CurrentSkinInfo.SetDefault());

        [TearDownSteps]
        public void TearDown() => setDefaultSkin();
    }
}
