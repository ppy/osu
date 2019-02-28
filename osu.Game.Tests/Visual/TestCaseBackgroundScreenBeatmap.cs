// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Screens.Select;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseBackgroundScreenBeatmap : ManualInputManagerTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ScreenWithBeatmapBackground),
            typeof(PlayerLoader),
            typeof(Player),
            typeof(UserDimContainer),
            typeof(OsuScreen)
        };

        private DummySongSelect songSelect;
        private DimAccessiblePlayerLoader playerLoader;
        private DimAccessiblePlayer player;
        private DatabaseContextFactory factory;
        private BeatmapManager manager;
        private RulesetStore rulesets;

        private ScreenStackCacheContainer screenStackContainer;

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            factory = new DatabaseContextFactory(LocalStorage);
            factory.ResetDatabase();

            using (var usage = factory.Get())
                usage.Migrate();

            factory.ResetDatabase();

            using (var usage = factory.Get())
                usage.Migrate();

            Dependencies.Cache(rulesets = new RulesetStore(factory));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, factory, rulesets, null, null, host, Beatmap.Default));
            Dependencies.Cache(new OsuConfigManager(LocalStorage));

            Beatmap.SetDefault();
        }

        [SetUp]
        public virtual void SetUp()
        {
            Schedule(() =>
            {
                manager.Delete(manager.GetAllUsableBeatmapSets());
                var temp = TestResources.GetTestBeatmapForImport();
                manager.Import(temp);
                Child = screenStackContainer = new ScreenStackCacheContainer { RelativeSizeAxes = Axes.Both };
                screenStackContainer.ScreenStack.Push(songSelect = new DummySongSelect());
            });
        }

        /// <summary>
        /// Check if <see cref="PlayerLoader"/> properly triggers background dim previews when a user hovers over the visual settings panel.
        /// </summary>
        [Test]
        public void PlayerLoaderSettingsHoverTest()
        {
            setupUserSettings();
            AddStep("Start player loader", () => songSelect.Push(playerLoader = new DimAccessiblePlayerLoader(player = new DimAccessiblePlayer())));
            AddUntilStep(() => playerLoader?.IsLoaded ?? false, "Wait for Player Loader to load");
            AddAssert("Background retained from song select", () => songSelect.IsBackgroundCurrent());
            AddStep("Trigger background preview", () =>
            {
                InputManager.MoveMouseTo(playerLoader.ScreenPos);
                InputManager.MoveMouseTo(playerLoader.VisualSettingsPos);
            });
            waitForDim();
            AddAssert("Screen is dimmed", () => songSelect.IsBackgroundDimmed());
            AddStep("Stop background preview", () => InputManager.MoveMouseTo(playerLoader.ScreenPos));
            waitForDim();
            AddAssert("Screen is undimmed", () => songSelect.IsBackgroundUndimmed());
        }

        /// <summary>
        /// In the case of a user triggering the dim preview the instant player gets loaded, then moving the cursor off of the visual settings:
        /// The OnHover of PlayerLoader will trigger, which could potentially trigger an undim unless checked for in PlayerLoader.
        /// We need to check that in this scenario, the dim is still properly applied after entering player.
        /// </summary>
        [Test]
        public void PlayerLoaderTransitionTest()
        {
            performFullSetup();
            AddStep("Trigger hover event", () => playerLoader.TriggerOnHover());
            AddAssert("Background retained from song select", () => songSelect.IsBackgroundCurrent());
            waitForDim();
            AddAssert("Screen is dimmed and unblurred", () => songSelect.IsBackgroundDimmed() && songSelect.IsBackgroundUnblurred());
        }

        /// <summary>
        /// Make sure the background is fully invisible (Alpha == 0) when the background should be disabled by the storyboard.
        /// </summary>
        [Test]
        public void StoryboardBackgroundVisibilityTest()
        {
            performFullSetup();
            createFakeStoryboard();
            AddStep("Storyboard Enabled", () =>
            {
                player.ReplacesBackground.Value = true;
                player.StoryboardEnabled.Value = true;
            });
            waitForDim();
            AddAssert("Background is invisible, storyboard is visible", () => songSelect.IsBackgroundInvisible() && player.IsStoryboardVisible());
            AddStep("Storyboard Disabled", () =>
            {
                player.ReplacesBackground.Value = false;
                player.StoryboardEnabled.Value = false;
            });
            waitForDim();
            AddAssert("Background is visible, storyboard is invisible", () => songSelect.IsBackgroundVisible() && player.IsStoryboardInvisible());
        }

        /// <summary>
        /// When exiting player, the screen that it suspends/exits to needs to have a fully visible (Alpha == 1) background.
        /// </summary>
        [Test]
        public void StoryboardTransitionTest()
        {
            performFullSetup();
            createFakeStoryboard();
            AddStep("Exit to song select", () => player.Exit());
            waitForDim();
            AddAssert("Background is visible", () => songSelect.IsBackgroundVisible());
        }

        /// <summary>
        /// Check if the <see cref="UserDimContainer"/> is properly accepting user dim changes at all.
        /// </summary>
        [Test]
        public void DisableUserDimTest()
        {
            performFullSetup();
            waitForDim();
            AddAssert("Screen is dimmed", () => songSelect.IsBackgroundDimmed());
            AddStep("EnableUserDim disabled", () => songSelect.DimEnabled.Value = false);
            waitForDim();
            AddAssert("Screen is undimmed", () => songSelect.IsBackgroundUndimmed());
            AddStep("EnableUserDim enabled", () => songSelect.DimEnabled.Value = true);
            waitForDim();
            AddAssert("Screen is dimmed", () => songSelect.IsBackgroundDimmed());
        }

        /// <summary>
        /// Check if the fade container retains dim when pausing
        /// </summary>
        [Test]
        public void PauseTest()
        {
            performFullSetup(true);
            AddStep("Pause", () => player.CurrentPauseContainer.Pause());
            waitForDim();
            AddAssert("Screen is dimmed", () => songSelect.IsBackgroundDimmed());
            AddStep("Unpause", () => player.CurrentPauseContainer.Resume());
            waitForDim();
            AddAssert("Screen is dimmed", () => songSelect.IsBackgroundDimmed());
        }

        /// <summary>
        /// Check if the fade container removes user dim when suspending <see cref="Player"/> for <see cref="SoloResults"/>
        /// </summary>
        [Test]
        public void TransitionTest()
        {
            performFullSetup();
            AddStep("Transition to Results", () => player.Push(new FadeAccessibleResults(new ScoreInfo { User = new User { Username = "osu!" } })));
            waitForDim();
            AddAssert("Screen is undimmed and is original background", () => songSelect.IsBackgroundUndimmed() && songSelect.IsBackgroundCurrent());
        }

        /// <summary>
        /// Check if background gets undimmed when leaving <see cref="Player"/>  for <see cref="PlaySongSelect"/>
        /// </summary>
        [Test]
        public void TransitionOutTest()
        {
            performFullSetup();
            AddStep("Exit to song select", () => player.Exit());
            waitForDim();
            AddAssert("Screen is undimmed", () => songSelect.IsBackgroundUndimmed());
        }

        private void waitForDim() => AddWaitStep(5, "Wait for dim");

        private void createFakeStoryboard() => AddStep("Create storyboard", () =>
        {
            player.StoryboardEnabled.Value = false;
            player.ReplacesBackground.Value = false;
            player.CurrentStoryboardContainer.Add(new SpriteText
            {
                Size = new Vector2(250, 50),
                Alpha = 1,
                Colour = Color4.Tomato,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "THIS IS A STORYBOARD",
            });
        });

        private void performFullSetup(bool allowPause = false)
        {
            setupUserSettings();

            AddStep("Start player loader", () =>
            {
                songSelect.Push(playerLoader = new DimAccessiblePlayerLoader(player = new DimAccessiblePlayer
                {
                    AllowPause = allowPause,
                    Ready = true,
                }));
            });
            AddUntilStep(() => playerLoader.IsLoaded, "Wait for Player Loader to load");
            AddStep("Move mouse to center of screen", () => InputManager.MoveMouseTo(playerLoader.ScreenPos));
            AddUntilStep(() => player.IsLoaded, "Wait for player to load");
        }

        private void setupUserSettings()
        {
            AddUntilStep(() => songSelect.Carousel.SelectedBeatmap != null, "Song select has selection");
            AddStep("Set default user settings", () =>
            {
                Beatmap.Value.Mods.Value = Beatmap.Value.Mods.Value.Concat(new[] { new OsuModNoFail() });
                songSelect.DimLevel.Value = 0.7f;
                songSelect.BlurLevel.Value = 0.0f;
            });
        }

        private class DummySongSelect : PlaySongSelect
        {
            protected override BackgroundScreen CreateBackground()
            {
                FadeAccessibleBackground background = new FadeAccessibleBackground(Beatmap.Value);
                DimEnabled.BindTo(background.EnableUserDim);
                return background;
            }

            public readonly Bindable<bool> DimEnabled = new Bindable<bool>();
            public readonly Bindable<double> DimLevel = new Bindable<double>();
            public readonly Bindable<double> BlurLevel = new Bindable<double>();

            public new BeatmapCarousel Carousel => base.Carousel;

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config)
            {
                config.BindWith(OsuSetting.DimLevel, DimLevel);
                config.BindWith(OsuSetting.BlurLevel, BlurLevel);
            }

            public bool IsBackgroundDimmed() => ((FadeAccessibleBackground)Background).CurrentColour == OsuColour.Gray(1 - (float)DimLevel.Value);

            public bool IsBackgroundUnblurred() => ((FadeAccessibleBackground)Background).CurrentBlur == new Vector2(0);

            public bool IsBackgroundUndimmed() => ((FadeAccessibleBackground)Background).CurrentColour == Color4.White;

            public bool IsBackgroundInvisible() => ((FadeAccessibleBackground)Background).CurrentAlpha == 0;

            public bool IsBackgroundVisible() => ((FadeAccessibleBackground)Background).CurrentAlpha == 1;

            /// <summary>
            /// Make sure every time a screen gets pushed, the background doesn't get replaced
            /// </summary>
            /// <returns>Whether or not the original background (The one created in DummySongSelect) is still the current background</returns>
            public bool IsBackgroundCurrent() => ((FadeAccessibleBackground)Background).IsCurrentScreen();
        }

        private class FadeAccessibleResults : SoloResults
        {
            public FadeAccessibleResults(ScoreInfo score)
                : base(score)
            {
            }

            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground(Beatmap.Value);
        }

        private class DimAccessiblePlayer : Player
        {
            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground(Beatmap.Value);

            protected override UserDimContainer CreateStoryboardContainer()
            {
                return new TestUserDimContainer(true)
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 1,
                    EnableUserDim = { Value = true }
                };
            }

            public PauseContainer CurrentPauseContainer => PauseContainer;

            public UserDimContainer CurrentStoryboardContainer => StoryboardContainer;

            // Whether or not the player should be allowed to load.
            public bool Ready;

            public Bindable<bool> StoryboardEnabled;
            public readonly Bindable<bool> ReplacesBackground = new Bindable<bool>();
            public readonly Bindable<bool> IsPaused = new Bindable<bool>();

            public bool IsStoryboardVisible() => ((TestUserDimContainer)CurrentStoryboardContainer).CurrentAlpha == 1;

            public bool IsStoryboardInvisible() => ((TestUserDimContainer)CurrentStoryboardContainer).CurrentAlpha <= 1;

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config)
            {
                while (!Ready)
                    Thread.Sleep(1);
                StoryboardEnabled = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
                ReplacesBackground.BindTo(Background.StoryboardReplacesBackground);
                RulesetContainer.IsPaused.BindTo(IsPaused);
            }
        }

        private class ScreenStackCacheContainer : Container
        {
            [Cached]
            private BackgroundScreenStack backgroundScreenStack;

            public readonly ScreenStack ScreenStack;

            public ScreenStackCacheContainer()
            {
                Add(backgroundScreenStack = new BackgroundScreenStack { RelativeSizeAxes = Axes.Both });
                Add(ScreenStack = new ScreenStack { RelativeSizeAxes = Axes.Both });
            }
        }

        private class DimAccessiblePlayerLoader : PlayerLoader
        {
            public VisualSettings VisualSettingsPos => VisualSettings;
            public BackgroundScreen ScreenPos => Background;

            public DimAccessiblePlayerLoader(Player player)
                : base(() => player)
            {
            }

            public void TriggerOnHover() => OnHover(new HoverEvent(new InputState()));

            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground(Beatmap.Value);
        }

        private class FadeAccessibleBackground : BackgroundScreenBeatmap
        {
            protected override UserDimContainer CreateFadeContainer() => fadeContainer = new TestUserDimContainer { RelativeSizeAxes = Axes.Both };

            public Color4 CurrentColour => fadeContainer.CurrentColour;
            public float CurrentAlpha => fadeContainer.CurrentAlpha;

            public Vector2 CurrentBlur => Background.BlurSigma;

            private TestUserDimContainer fadeContainer;

            public FadeAccessibleBackground(WorkingBeatmap beatmap)
                : base(beatmap)
            {
            }
        }

        private class TestUserDimContainer : UserDimContainer
        {
            public Color4 CurrentColour => DimContainer.Colour;
            public float CurrentAlpha => DimContainer.Alpha;

            public TestUserDimContainer(bool isStoryboard = false)
                : base(isStoryboard)
            {
            }
        }
    }
}
