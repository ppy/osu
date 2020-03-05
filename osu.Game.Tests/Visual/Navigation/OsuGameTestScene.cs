// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osuTK.Graphics;
using IntroSequence = osu.Game.Configuration.IntroSequence;

namespace osu.Game.Tests.Visual.Navigation
{
    /// <summary>
    /// A scene which tests full game flow.
    /// </summary>
    public abstract class OsuGameTestScene : ManualInputManagerTestScene
    {
        private GameHost host;

        protected TestOsuGame Game;

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            this.host = host;

            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Black,
            };
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Create new game instance", () =>
            {
                if (Game != null)
                {
                    Remove(Game);
                    Game.Dispose();
                }

                RecycleLocalStorage();

                // see MouseSettings
                var frameworkConfig = host.Dependencies.Get<FrameworkConfigManager>();
                frameworkConfig.GetBindable<double>(FrameworkSetting.CursorSensitivity).Disabled = false;

                CreateGame();
            });

            AddUntilStep("Wait for load", () => Game.IsLoaded);
            AddUntilStep("Wait for intro", () => Game.ScreenStack.CurrentScreen is IntroScreen);

            ConfirmAtMainMenu();
        }

        protected void CreateGame()
        {
            Game = new TestOsuGame(LocalStorage, API);
            Game.SetHost(host);

            // todo: this can be removed once we can run audio tracks without a device present
            // see https://github.com/ppy/osu/issues/1302
            Game.LocalConfig.Set(OsuSetting.IntroSequence, IntroSequence.Circles);

            Add(Game);
        }

        protected void PushAndConfirm(Func<Screen> newScreen)
        {
            Screen screen = null;
            AddStep("Push new screen", () => Game.ScreenStack.Push(screen = newScreen()));
            AddUntilStep("Wait for new screen", () => Game.ScreenStack.CurrentScreen == screen && screen.IsLoaded);
        }

        protected void ConfirmAtMainMenu() => AddUntilStep("Wait for main menu", () => Game.ScreenStack.CurrentScreen is MainMenu menu && menu.IsLoaded);

        public class TestOsuGame : OsuGame
        {
            public new ScreenStack ScreenStack => base.ScreenStack;

            public new BackButton BackButton => base.BackButton;

            public new BeatmapManager BeatmapManager => base.BeatmapManager;

            public new SettingsPanel Settings => base.Settings;

            public new MusicController MusicController => base.MusicController;

            public new OsuConfigManager LocalConfig => base.LocalConfig;

            public new Bindable<WorkingBeatmap> Beatmap => base.Beatmap;

            public new Bindable<RulesetInfo> Ruleset => base.Ruleset;

            // if we don't do this, when running under nUnit the version that gets populated is that of nUnit.
            public override string Version => "test game";

            protected override Loader CreateLoader() => new TestLoader();

            public new void PerformFromScreen(Action<IScreen> action, IEnumerable<Type> validScreens = null) => base.PerformFromScreen(action, validScreens);

            public TestOsuGame(Storage storage, IAPIProvider api)
            {
                Storage = storage;
                API = api;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                API.Login("Rhythm Champion", "osu!");

                Dependencies.Get<SessionStatics>().Set(Static.MutedAudioNotificationShownOnce, true);
            }
        }

        public class TestLoader : Loader
        {
            protected override ShaderPrecompiler CreateShaderPrecompiler() => new TestShaderPrecompiler();

            private class TestShaderPrecompiler : ShaderPrecompiler
            {
                protected override bool AllLoaded => true;
            }
        }
    }
}
