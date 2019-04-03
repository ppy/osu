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

namespace osu.Game.Tests.Visual.Background
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
        private TestPlayerLoader playerLoader;
        private TestPlayer player;
        private DatabaseContextFactory factory;
        private BeatmapManager manager;
        private RulesetStore rulesets;

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

            manager.Import(TestResources.GetTestBeatmapForImport());

            Beatmap.SetDefault();
        }

        [SetUp]
        public virtual void SetUp() => Schedule(() =>
        {
            Child = new OsuScreenStack(songSelect = new DummySongSelect())
            {
                RelativeSizeAxes = Axes.Both
            };
        });

        /// <summary>
        /// Check if <see cref="PlayerLoader"/> properly triggers the visual settings preview when a user hovers over the visual settings panel.
        /// </summary>
        [Test]
        public void PlayerLoaderSettingsHoverTest()
        {
            setupUserSettings();
            AddStep("Start player loader", () => songSelect.Push(playerLoader = new TestPlayerLoader(player = new TestPlayer { BlockLoad = true })));
            AddUntilStep("Wait for Player Loader to load", () => playerLoader?.IsLoaded ?? false);
            AddAssert("Background retained from song select", () => songSelect.IsBackgroundCurrent());
            AddStep("Trigger background preview", () =>
            {
                InputManager.MoveMouseTo(playerLoader.ScreenPos);
                InputManager.MoveMouseTo(playerLoader.VisualSettingsPos);
            });
            waitForDim();
            AddAssert("Screen is dimmed and blur applied", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied());
            AddStep("Stop background preview", () => InputManager.MoveMouseTo(playerLoader.ScreenPos));
            waitForDim();
            AddAssert("Screen is undimmed and user blur removed", () => songSelect.IsBackgroundUndimmed() && playerLoader.IsBlurCorrect());
        }

        /// <summary>
        /// In the case of a user triggering the dim preview the instant player gets loaded, then moving the cursor off of the visual settings:
        /// The OnHover of PlayerLoader will trigger, which could potentially cause visual settings to be unapplied unless checked for in PlayerLoader.
        /// We need to check that in this scenario, the dim and blur is still properly applied after entering player.
        /// </summary>
        [Test]
        public void PlayerLoaderTransitionTest()
        {
            performFullSetup();
            AddStep("Trigger hover event", () => playerLoader.TriggerOnHover());
            AddAssert("Background retained from song select", () => songSelect.IsBackgroundCurrent());
            waitForDim();
            AddAssert("Screen is dimmed and blur applied", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied());
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
        /// Check if the <see cref="UserDimContainer"/> is properly accepting user-defined visual changes at all.
        /// </summary>
        [Test]
        public void DisableUserDimTest()
        {
            performFullSetup();
            waitForDim();
            AddAssert("Screen is dimmed and blur applied", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied());
            AddStep("EnableUserDim disabled", () => songSelect.DimEnabled.Value = false);
            waitForDim();
            AddAssert("Screen is undimmed and user blur removed", () => songSelect.IsBackgroundUndimmed() && songSelect.IsUserBlurDisabled());
            AddStep("EnableUserDim enabled", () => songSelect.DimEnabled.Value = true);
            waitForDim();
            AddAssert("Screen is dimmed and blur applied", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied());
        }

        /// <summary>
        /// Check if the visual settings container retains dim and blur when pausing
        /// </summary>
        [Test]
        public void PauseTest()
        {
            performFullSetup(true);
            AddStep("Pause", () => player.Pause());
            waitForDim();
            AddAssert("Screen is dimmed and blur applied", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied());
            AddStep("Unpause", () => player.Resume());
            waitForDim();
            AddAssert("Screen is dimmed and blur applied", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied());
        }

        /// <summary>
        /// Check if the visual settings container removes user dim when suspending <see cref="Player"/> for <see cref="SoloResults"/>
        /// </summary>
        [Test]
        public void TransitionTest()
        {
            performFullSetup();
            var results = new FadeAccessibleResults(new ScoreInfo { User = new User { Username = "osu!" } });
            AddStep("Transition to Results", () => player.Push(results));
            AddUntilStep("Wait for results is current", results.IsCurrentScreen);
            waitForDim();
            AddAssert("Screen is undimmed, original background retained", () =>
                songSelect.IsBackgroundUndimmed() && songSelect.IsBackgroundCurrent() && results.IsBlurCorrect());
        }

        /// <summary>
        /// Check if background gets undimmed and unblurred when leaving <see cref="Player"/>  for <see cref="PlaySongSelect"/>
        /// </summary>
        [Test]
        public void TransitionOutTest()
        {
            performFullSetup();
            AddStep("Exit to song select", () => player.Exit());
            waitForDim();
            AddAssert("Screen is undimmed and user blur removed", () => songSelect.IsBackgroundUndimmed() && songSelect.IsBlurCorrect());
        }

        /// <summary>
        /// Check if hovering on the visual settings dialogue after resuming from player still previews the background dim.
        /// </summary>
        [Test]
        public void ResumeFromPlayerTest()
        {
            performFullSetup();
            AddStep("Move mouse to Visual Settings", () => InputManager.MoveMouseTo(playerLoader.VisualSettingsPos));
            AddStep("Resume PlayerLoader", () => player.Restart());
            waitForDim();
            AddAssert("Screen is dimmed and blur applied", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied());
            AddStep("Move mouse to center of screen", () => InputManager.MoveMouseTo(playerLoader.ScreenPos));
            waitForDim();
            AddAssert("Screen is undimmed and user blur removed", () => songSelect.IsBackgroundUndimmed() && playerLoader.IsBlurCorrect());
        }

        private void waitForDim() => AddWaitStep("Wait for dim", 5);

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

            AddStep("Start player loader", () => songSelect.Push(playerLoader = new TestPlayerLoader(player = new TestPlayer(allowPause))));

            AddUntilStep("Wait for Player Loader to load", () => playerLoader.IsLoaded);
            AddStep("Move mouse to center of screen", () => InputManager.MoveMouseTo(playerLoader.ScreenPos));
            AddUntilStep("Wait for player to load", () => player.IsLoaded);
        }

        private void setupUserSettings()
        {
            AddUntilStep("Song select has selection", () => songSelect.Carousel.SelectedBeatmap != null);
            AddStep("Set default user settings", () =>
            {
                Beatmap.Value.Mods.Value = Beatmap.Value.Mods.Value.Concat(new[] { new OsuModNoFail() });
                songSelect.DimLevel.Value = 0.7f;
                songSelect.BlurLevel.Value = 0.4f;
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

            public bool IsBackgroundUndimmed() => ((FadeAccessibleBackground)Background).CurrentColour == Color4.White;

            public bool IsUserBlurApplied() => ((FadeAccessibleBackground)Background).CurrentBlur == new Vector2((float)BlurLevel.Value * 25);

            public bool IsUserBlurDisabled() => ((FadeAccessibleBackground)Background).CurrentBlur == new Vector2(0);

            public bool IsBackgroundInvisible() => ((FadeAccessibleBackground)Background).CurrentAlpha == 0;

            public bool IsBackgroundVisible() => ((FadeAccessibleBackground)Background).CurrentAlpha == 1;

            public bool IsBlurCorrect() => ((FadeAccessibleBackground)Background).CurrentBlur == new Vector2(BACKGROUND_BLUR);

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

            public bool IsBlurCorrect() => ((FadeAccessibleBackground)Background).CurrentBlur == new Vector2(BACKGROUND_BLUR);
        }

        private class TestPlayer : Player
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

            public UserDimContainer CurrentStoryboardContainer => StoryboardContainer;

            // Whether or not the player should be allowed to load.
            public bool BlockLoad;

            public Bindable<bool> StoryboardEnabled;
            public readonly Bindable<bool> ReplacesBackground = new Bindable<bool>();
            public readonly Bindable<bool> IsPaused = new Bindable<bool>();

            public TestPlayer(bool allowPause = true)
                : base(allowPause)
            {
            }

            public bool IsStoryboardVisible() => ((TestUserDimContainer)CurrentStoryboardContainer).CurrentAlpha == 1;

            public bool IsStoryboardInvisible() => ((TestUserDimContainer)CurrentStoryboardContainer).CurrentAlpha <= 1;

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config, CancellationToken token)
            {
                while (BlockLoad && !token.IsCancellationRequested)
                    Thread.Sleep(1);

                StoryboardEnabled = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
                ReplacesBackground.BindTo(Background.StoryboardReplacesBackground);
                DrawableRuleset.IsPaused.BindTo(IsPaused);
            }
        }

        private class TestPlayerLoader : PlayerLoader
        {
            public VisualSettings VisualSettingsPos => VisualSettings;
            public BackgroundScreen ScreenPos => Background;

            public TestPlayerLoader(Player player)
                : base(() => player)
            {
            }

            public void TriggerOnHover() => OnHover(new HoverEvent(new InputState()));

            public bool IsBlurCorrect() => ((FadeAccessibleBackground)Background).CurrentBlur == new Vector2(BACKGROUND_BLUR);

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
