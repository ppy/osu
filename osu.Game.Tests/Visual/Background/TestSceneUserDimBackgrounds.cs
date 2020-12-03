// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Select;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Background
{
    [TestFixture]
    public class TestSceneUserDimBackgrounds : OsuManualInputManagerTestScene
    {
        private DummySongSelect songSelect;
        private TestPlayerLoader playerLoader;
        private LoadBlockingTestPlayer player;
        private BeatmapManager manager;
        private RulesetStore rulesets;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, host, Beatmap.Default));
            Dependencies.Cache(new OsuConfigManager(LocalStorage));

            manager.Import(TestResources.GetTestBeatmapForImport()).Wait();

            Beatmap.SetDefault();
        }

        [SetUp]
        public virtual void SetUp() => Schedule(() =>
        {
            var stack = new OsuScreenStack { RelativeSizeAxes = Axes.Both };
            Child = stack;

            stack.Push(songSelect = new DummySongSelect());
        });

        /// <summary>
        /// Check if <see cref="PlayerLoader"/> properly triggers the visual settings preview when a user hovers over the visual settings panel.
        /// </summary>
        [Test]
        public void TestPlayerLoaderSettingsHover()
        {
            setupUserSettings();
            AddStep("Start player loader", () => songSelect.Push(playerLoader = new TestPlayerLoader(player = new LoadBlockingTestPlayer { BlockLoad = true })));
            AddUntilStep("Wait for Player Loader to load", () => playerLoader?.IsLoaded ?? false);
            AddAssert("Background retained from song select", () => songSelect.IsBackgroundCurrent());
            AddStep("Trigger background preview", () =>
            {
                InputManager.MoveMouseTo(playerLoader.ScreenPos);
                InputManager.MoveMouseTo(playerLoader.VisualSettingsPos);
            });
            AddUntilStep("Screen is dimmed and blur applied", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied());
            AddStep("Stop background preview", () => InputManager.MoveMouseTo(playerLoader.ScreenPos));
            AddUntilStep("Screen is undimmed and user blur removed", () => songSelect.IsBackgroundUndimmed() && playerLoader.IsBlurCorrect());
        }

        /// <summary>
        /// In the case of a user triggering the dim preview the instant player gets loaded, then moving the cursor off of the visual settings:
        /// The OnHover of PlayerLoader will trigger, which could potentially cause visual settings to be unapplied unless checked for in PlayerLoader.
        /// We need to check that in this scenario, the dim and blur is still properly applied after entering player.
        /// </summary>
        [Test]
        public void TestPlayerLoaderTransition()
        {
            performFullSetup();
            AddStep("Trigger hover event", () => playerLoader.TriggerOnHover());
            AddAssert("Background retained from song select", () => songSelect.IsBackgroundCurrent());
            AddUntilStep("Screen is dimmed and blur applied", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied());
        }

        /// <summary>
        /// Make sure the background is fully invisible (Alpha == 0) when the background should be disabled by the storyboard.
        /// </summary>
        [Test]
        public void TestStoryboardBackgroundVisibility()
        {
            performFullSetup();
            createFakeStoryboard();
            AddStep("Enable Storyboard", () =>
            {
                player.ReplacesBackground.Value = true;
                player.StoryboardEnabled.Value = true;
            });
            AddUntilStep("Background is invisible, storyboard is visible", () => songSelect.IsBackgroundInvisible() && player.IsStoryboardVisible);
            AddStep("Disable Storyboard", () =>
            {
                player.ReplacesBackground.Value = false;
                player.StoryboardEnabled.Value = false;
            });
            AddUntilStep("Background is visible, storyboard is invisible", () => songSelect.IsBackgroundVisible() && !player.IsStoryboardVisible);
        }

        /// <summary>
        /// When exiting player, the screen that it suspends/exits to needs to have a fully visible (Alpha == 1) background.
        /// </summary>
        [Test]
        public void TestStoryboardTransition()
        {
            performFullSetup();
            createFakeStoryboard();
            AddStep("Exit to song select", () => player.Exit());
            AddUntilStep("Background is visible", () => songSelect.IsBackgroundVisible());
        }

        /// <summary>
        /// Ensure <see cref="UserDimContainer"/> is properly accepting user-defined visual changes for a background.
        /// </summary>
        [Test]
        public void TestDisableUserDimBackground()
        {
            performFullSetup();
            AddUntilStep("Screen is dimmed and blur applied", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied());
            AddStep("Enable user dim", () => songSelect.DimEnabled.Value = false);
            AddUntilStep("Screen is undimmed and user blur removed", () => songSelect.IsBackgroundUndimmed() && songSelect.IsUserBlurDisabled());
            AddStep("Disable user dim", () => songSelect.DimEnabled.Value = true);
            AddUntilStep("Screen is dimmed and blur applied", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied());
        }

        /// <summary>
        /// Ensure <see cref="UserDimContainer"/> is properly accepting user-defined visual changes for a storyboard.
        /// </summary>
        [Test]
        public void TestDisableUserDimStoryboard()
        {
            performFullSetup();
            createFakeStoryboard();
            AddStep("Enable Storyboard", () =>
            {
                player.ReplacesBackground.Value = true;
                player.StoryboardEnabled.Value = true;
            });
            AddStep("Enable user dim", () => player.DimmableStoryboard.EnableUserDim.Value = true);
            AddStep("Set dim level to 1", () => songSelect.DimLevel.Value = 1f);
            AddUntilStep("Storyboard is invisible", () => !player.IsStoryboardVisible);
            AddStep("Disable user dim", () => player.DimmableStoryboard.EnableUserDim.Value = false);
            AddUntilStep("Storyboard is visible", () => player.IsStoryboardVisible);
        }

        /// <summary>
        /// Check if the visual settings container retains dim and blur when pausing
        /// </summary>
        [Test]
        public void TestPause()
        {
            performFullSetup(true);
            AddStep("Pause", () => player.Pause());
            AddUntilStep("Screen is dimmed and blur applied", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied());
            AddStep("Unpause", () => player.Resume());
            AddUntilStep("Screen is dimmed and blur applied", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied());
        }

        /// <summary>
        /// Check if the visual settings container removes user dim when suspending <see cref="Player"/> for <see cref="ResultsScreen"/>
        /// </summary>
        [Test]
        public void TestTransition()
        {
            performFullSetup();

            FadeAccessibleResults results = null;

            AddStep("Transition to Results", () => player.Push(results = new FadeAccessibleResults(new ScoreInfo
            {
                User = new User { Username = "osu!" },
                Beatmap = new TestBeatmap(Ruleset.Value).BeatmapInfo,
                Ruleset = Ruleset.Value,
            })));

            AddUntilStep("Wait for results is current", () => results.IsCurrentScreen());
            AddUntilStep("Screen is undimmed, original background retained", () =>
                songSelect.IsBackgroundUndimmed() && songSelect.IsBackgroundCurrent() && results.IsBlurCorrect());
        }

        /// <summary>
        /// Check if background gets undimmed and unblurred when leaving <see cref="Player"/>  for <see cref="PlaySongSelect"/>
        /// </summary>
        [Test]
        public void TestTransitionOut()
        {
            performFullSetup();
            AddStep("Exit to song select", () => player.Exit());
            AddUntilStep("Screen is undimmed and user blur removed", () => songSelect.IsBackgroundUndimmed() && songSelect.IsBlurCorrect());
        }

        /// <summary>
        /// Check if hovering on the visual settings dialogue after resuming from player still previews the background dim.
        /// </summary>
        [Test]
        public void TestResumeFromPlayer()
        {
            performFullSetup();
            AddStep("Move mouse to Visual Settings", () => InputManager.MoveMouseTo(playerLoader.VisualSettingsPos));
            AddStep("Resume PlayerLoader", () => player.Restart());
            AddUntilStep("Screen is dimmed and blur applied", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied());
            AddStep("Move mouse to center of screen", () => InputManager.MoveMouseTo(playerLoader.ScreenPos));
            AddUntilStep("Screen is undimmed and user blur removed", () => songSelect.IsBackgroundUndimmed() && playerLoader.IsBlurCorrect());
        }

        private void createFakeStoryboard() => AddStep("Create storyboard", () =>
        {
            player.StoryboardEnabled.Value = false;
            player.ReplacesBackground.Value = false;
            player.DimmableStoryboard.Add(new OsuSpriteText
            {
                Size = new Vector2(500, 50),
                Alpha = 1,
                Colour = Color4.White,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "THIS IS A STORYBOARD",
                Font = new FontUsage(size: 50)
            });
        });

        private void performFullSetup(bool allowPause = false)
        {
            setupUserSettings();

            AddStep("Start player loader", () => songSelect.Push(playerLoader = new TestPlayerLoader(player = new LoadBlockingTestPlayer(allowPause))));

            AddUntilStep("Wait for Player Loader to load", () => playerLoader.IsLoaded);
            AddStep("Move mouse to center of screen", () => InputManager.MoveMouseTo(playerLoader.ScreenPos));
            AddUntilStep("Wait for player to load", () => player.IsLoaded);
        }

        private void setupUserSettings()
        {
            AddUntilStep("Song select is current", () => songSelect.IsCurrentScreen());
            AddUntilStep("Song select has selection", () => songSelect.Carousel?.SelectedBeatmap != null);
            AddStep("Set default user settings", () =>
            {
                SelectedMods.Value = SelectedMods.Value.Concat(new[] { new OsuModNoFail() }).ToArray();
                songSelect.DimLevel.Value = 0.7f;
                songSelect.BlurLevel.Value = 0.4f;
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            rulesets?.Dispose();
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
            public readonly Bindable<double> DimLevel = new BindableDouble();
            public readonly Bindable<double> BlurLevel = new BindableDouble();

            public new BeatmapCarousel Carousel => base.Carousel;

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config)
            {
                config.BindWith(OsuSetting.DimLevel, DimLevel);
                config.BindWith(OsuSetting.BlurLevel, BlurLevel);
            }

            public bool IsBackgroundDimmed() => ((FadeAccessibleBackground)Background).CurrentColour == OsuColour.Gray(1f - ((FadeAccessibleBackground)Background).CurrentDim);

            public bool IsBackgroundUndimmed() => ((FadeAccessibleBackground)Background).CurrentColour == Color4.White;

            public bool IsUserBlurApplied() => ((FadeAccessibleBackground)Background).CurrentBlur == new Vector2((float)BlurLevel.Value * BackgroundScreenBeatmap.USER_BLUR_FACTOR);

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

        private class FadeAccessibleResults : ResultsScreen
        {
            public FadeAccessibleResults(ScoreInfo score)
                : base(score, true)
            {
            }

            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground(Beatmap.Value);

            public bool IsBlurCorrect() => ((FadeAccessibleBackground)Background).CurrentBlur == new Vector2(BACKGROUND_BLUR);
        }

        private class LoadBlockingTestPlayer : TestPlayer
        {
            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground(Beatmap.Value);

            public new DimmableStoryboard DimmableStoryboard => base.DimmableStoryboard;

            // Whether or not the player should be allowed to load.
            public bool BlockLoad;

            public Bindable<bool> StoryboardEnabled;
            public readonly Bindable<bool> ReplacesBackground = new Bindable<bool>();
            public readonly Bindable<bool> IsPaused = new Bindable<bool>();

            public LoadBlockingTestPlayer(bool allowPause = true)
                : base(allowPause)
            {
            }

            public bool IsStoryboardVisible => DimmableStoryboard.ContentDisplayed;

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
            protected override DimmableBackground CreateFadeContainer() => dimmable = new TestDimmableBackground { RelativeSizeAxes = Axes.Both };

            public Color4 CurrentColour => dimmable.CurrentColour;

            public float CurrentAlpha => dimmable.CurrentAlpha;

            public float CurrentDim => dimmable.DimLevel;

            public Vector2 CurrentBlur => Background.BlurSigma;

            private TestDimmableBackground dimmable;

            public FadeAccessibleBackground(WorkingBeatmap beatmap)
                : base(beatmap)
            {
            }
        }

        private class TestDimmableBackground : BackgroundScreenBeatmap.DimmableBackground
        {
            public Color4 CurrentColour => Content.Colour;
            public float CurrentAlpha => Content.Alpha;

            public new float DimLevel => base.DimLevel;
        }
    }
}
