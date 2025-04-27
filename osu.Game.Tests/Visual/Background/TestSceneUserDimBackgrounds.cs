// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Video;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Select;
using osu.Game.Storyboards;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Background
{
    [TestFixture]
    public partial class TestSceneUserDimBackgrounds : ScreenTestScene
    {
        private DummySongSelect songSelect;
        private TestPlayerLoader playerLoader;
        private LoadBlockingTestPlayer player;
        private BeatmapManager manager;
        private RulesetStore rulesets;
        private TestStoryboard testStoryboard;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            DetachedBeatmapStore detachedBeatmapStore;

            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(new OsuConfigManager(LocalStorage));
            Dependencies.Cache(detachedBeatmapStore = new DetachedBeatmapStore());
            Dependencies.Cache(Realm);

            manager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();

            Add(detachedBeatmapStore);

            Beatmap.SetDefault();
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("push song select", () => Stack.Push(songSelect = new DummySongSelect()));
        }

        /// <summary>
        /// User settings should always be ignored on song select screen.
        /// </summary>
        [Test]
        public void TestUserSettingsIgnoredOnSongSelect()
        {
            setupUserSettings();
            AddUntilStep("Screen is undimmed", () => songSelect.IsBackgroundUndimmed());
            AddUntilStep("Screen using background blur", () => songSelect.IsBackgroundBlur());
            performFullSetup();
            AddStep("Exit to song select", () => player.Exit());
            AddUntilStep("Screen is undimmed", () => songSelect.IsBackgroundUndimmed());
            AddUntilStep("Screen using background blur", () => songSelect.IsBackgroundBlur());
        }

        /// <summary>
        /// Check if <see cref="PlayerLoader"/> properly triggers the visual settings preview when a user hovers over the visual settings panel.
        /// </summary>
        [Test]
        public void TestPlayerLoaderSettingsHover()
        {
            setupUserSettings();
            AddStep("Start player loader", () => songSelect.Push(playerLoader = new TestPlayerLoader(player = new LoadBlockingTestPlayer { BlockLoad = true })));
            AddUntilStep("Wait for Player Loader to load", () => playerLoader?.IsLoaded ?? false);
            AddAssert("Background retained from song select", () =>
            {
                InputManager.MoveMouseTo(playerLoader);
                return songSelect.IsBackgroundCurrent();
            });

            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () =>
            {
                InputManager.MoveMouseTo(playerLoader.VisualSettingsPos);
                return songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset();
            });

            AddStep("Stop background preview", () => InputManager.MoveMouseTo(playerLoader.ScreenPos));
            AddUntilStep("Screen is undimmed and user blur removed", () => songSelect.IsBackgroundUndimmed() && songSelect.CheckBackgroundBlur(playerLoader.ExpectedBackgroundBlur));
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
            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset());
        }

        /// <summary>
        /// Make sure the background is fully invisible (Alpha == 0) when the background should be disabled by the storyboard.
        /// </summary>
        [Test]
        public void TestStoryboardBackgroundVisibility()
        {
            performFullSetup();
            AddAssert("Background retained from song select", () => songSelect.IsBackgroundCurrent());
            createFakeStoryboard();
            AddStep("Enable Storyboard", () =>
            {
                player.StoryboardReplacesBackground.Value = true;
                player.StoryboardEnabled.Value = true;
            });
            AddUntilStep("Background is black, storyboard is visible", () => songSelect.IsBackgroundVisible() && songSelect.IsBackgroundBlack() && player.IsStoryboardVisible);
            AddStep("Disable Storyboard", () =>
            {
                player.StoryboardReplacesBackground.Value = false;
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
            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset());
            AddStep("Disable user dim", () => songSelect.IgnoreUserSettings.Value = true);
            AddUntilStep("Screen is undimmed and user blur removed", () => songSelect.IsBackgroundUndimmed() && songSelect.IsUserBlurDisabled());
            AddStep("Enable user dim", () => songSelect.IgnoreUserSettings.Value = false);
            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset());
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
                player.StoryboardReplacesBackground.Value = true;
                player.StoryboardEnabled.Value = true;
            });
            AddStep("Enable user dim", () => player.DimmableStoryboard.IgnoreUserSettings.Value = false);
            AddStep("Set dim level to 1", () => songSelect.DimLevel.Value = 1f);
            AddUntilStep("Storyboard is invisible", () => !player.IsStoryboardVisible);
            AddStep("Disable user dim", () => player.DimmableStoryboard.IgnoreUserSettings.Value = true);
            AddUntilStep("Storyboard is visible", () => player.IsStoryboardVisible);
        }

        [Test]
        public void TestStoryboardIgnoreUserSettings()
        {
            performFullSetup();
            createFakeStoryboard();
            AddStep("Enable replacing background", () => player.StoryboardReplacesBackground.Value = true);

            AddUntilStep("Storyboard is invisible", () => !player.IsStoryboardVisible);
            AddUntilStep("Background is visible", () => songSelect.IsBackgroundVisible());

            AddStep("Ignore user settings", () =>
            {
                player.ApplyToBackground(b => b.IgnoreUserSettings.Value = true);
                player.DimmableStoryboard.IgnoreUserSettings.Value = true;
            });
            AddUntilStep("Storyboard is visible", () => player.IsStoryboardVisible);
            AddUntilStep("Background is dimmed", () => songSelect.IsBackgroundVisible() && songSelect.IsBackgroundBlack());

            AddStep("Disable background replacement", () => player.StoryboardReplacesBackground.Value = false);
            AddUntilStep("Storyboard is visible", () => player.IsStoryboardVisible);
            AddUntilStep("Background is visible", () => songSelect.IsBackgroundVisible() && !songSelect.IsBackgroundBlack());
        }

        /// <summary>
        /// Check if the visual settings container retains dim and blur when pausing
        /// </summary>
        [Test]
        public void TestPause()
        {
            performFullSetup(true);
            AddStep("Pause", () => player.Pause());
            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset());
            AddStep("Unpause", () => player.Resume());
            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset());
        }

        /// <summary>
        /// Check if the visual settings container removes user dim when suspending <see cref="Player"/> for <see cref="ResultsScreen"/>
        /// </summary>
        [Test]
        public void TestTransition()
        {
            performFullSetup();

            FadeAccessibleResults results = null;

            AddStep("Transition to Results", () => player.Push(results = new FadeAccessibleResults(TestResources.CreateTestScoreInfo())));

            AddUntilStep("Wait for results is current", () => results.IsCurrentScreen());

            AddUntilStep("Screen is undimmed, original background retained", () =>
                songSelect.IsBackgroundUndimmed() && songSelect.IsBackgroundCurrent() && songSelect.CheckBackgroundBlur(results.ExpectedBackgroundBlur));
        }

        /// <summary>
        /// Check if hovering on the visual settings dialogue after resuming from player still previews the background dim.
        /// </summary>
        [Test]
        public void TestResumeFromPlayer()
        {
            performFullSetup();
            AddStep("Move mouse to Visual Settings location", () => InputManager.MoveMouseTo(playerLoader.ScreenSpaceDrawQuad.TopRight
                                                                                             + new Vector2(-playerLoader.VisualSettingsPos.ScreenSpaceDrawQuad.Width,
                                                                                                 playerLoader.VisualSettingsPos.ScreenSpaceDrawQuad.Height / 2
                                                                                             )));
            AddStep("Resume PlayerLoader", () => player.Restart());
            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset());
            AddStep("Move mouse to center of screen", () => InputManager.MoveMouseTo(playerLoader.ScreenPos));
            AddUntilStep("Screen is undimmed and user blur removed", () => songSelect.IsBackgroundUndimmed() && songSelect.CheckBackgroundBlur(playerLoader.ExpectedBackgroundBlur));
        }

        /// <summary>
        /// Check if both the Sprite and BufferedContainer can handle dimming and don't handle it at the same time.
        /// </summary>
        [Test]
        public void TestDimmingHandlers()
        {
            AddStep("Disallow blurring", () => TestBeatmapBackground.AllowBlur = false);

            SetUpSteps();

            performFullSetup();

            AddUntilStep("Screen is dimmed and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsBackgroundColourOffset());
            AddUntilStep("BufferedContainer is not initialized", () => songSelect.IsBufferedContainerNull());
            AddUntilStep("Sprite is dimmed", () => songSelect.IsSpriteDimmed());

            AddStep("Allow blurring", () => TestBeatmapBackground.AllowBlur = true);

            SetUpSteps();

            performFullSetup();

            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset());
            AddUntilStep("BufferedContainer is initialized", () => !songSelect.IsBufferedContainerNull());
            AddUntilStep("BufferedContainer is dimmed", () => songSelect.IsBufferedContainerDimmed());
            AddUntilStep("Sprite is not dimmed", () => !songSelect.IsSpriteDimmed());
        }

        /// <summary>
        /// Check if BufferedContainer redraws the framebuffer on dim changes.
        /// </summary>
        [Test]
        public void TestNoBufferRedrawOnDimChange()
        {
            performFullSetup();

            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset());
            AddUntilStep("BufferedContainer is initialized", () => !songSelect.IsBufferedContainerNull());

            AddStep("Start tracking redraws", () => songSelect.RequiredRedraw = false);

            AddStep("Undim the screen", () =>
            {
                songSelect.DimLevel.Value = 0.0;
                songSelect.DimColour.Value = 0.0;
            });
            AddUntilStep("Screen is undimmed and blur applied", () => songSelect.IsBackgroundUndimmed() && songSelect.IsUserBlurApplied());
            AddUntilStep("Redraw wasn't required", () => !songSelect.RequiredRedraw);

            AddStep("Dim the screen", () =>
            {
                songSelect.DimLevel.Value = 0.7;
                songSelect.DimColour.Value = 0.5;
            });
            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset());
            AddUntilStep("Redraw wasn't required", () => !songSelect.RequiredRedraw);
        }

        /// <summary>
        /// Check if storyboard elements receive correct DrawColourOffset.
        /// </summary>
        [Test]
        public void TestStoryboardElementsDimmedWithColourOffset()
        {
            performFullSetup();
            createFakeStoryboardWithElements();

            AddStep("Enable Storyboard", () =>
            {
                player.StoryboardEnabled.Value = true;
            });

            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset());
            verifyStoryboardColourOffset();
            verifyAdditiveSpriteIsNotOffset();

            AddStep("Replace background", () =>
            {
                player.StoryboardReplacesBackground.Value = true;
            });

            AddUntilStep("Background is fully dimmed and dim colour adjusted", () => songSelect.IsBackgroundFullyDimmed() && songSelect.IsBackgroundColourOffset());
            verifyStoryboardColourOffset();
            verifyAdditiveSpriteIsNotOffset();

            AddStep("Ignore user settings", () =>
            {
                player.ApplyToBackground(b => b.IgnoreUserSettings.Value = true);
                player.DimmableStoryboard.IgnoreUserSettings.Value = true;
            });

            AddUntilStep("Background is black", () => songSelect.IsBackgroundBlack());
            verifyStoryboardColourNotOffset();
            verifyAdditiveSpriteIsNotOffset();
        }

        private void createFakeStoryboard() => AddStep("Create storyboard", () =>
        {
            player.StoryboardEnabled.Value = false;
            player.StoryboardReplacesBackground.Value = false;
            player.DimmableStoryboard.Add(new OsuSpriteText
            {
                Size = new Vector2(500, 50),
                Alpha = 1,
                Colour = Colour4.White,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "THIS IS A STORYBOARD",
                Font = new FontUsage(size: 50)
            });
        });

        private void createFakeStoryboardWithElements() => AddStep("Create storyboard with elements", () =>
        {
            player.StoryboardEnabled.Value = false;
            player.StoryboardReplacesBackground.Value = false;

            testStoryboard = new TestStoryboard();

            DrawableStoryboard drawableStoryboard = testStoryboard.CreateDrawable();

            player.DimmableStoryboard.Add(drawableStoryboard);
            player.DimmableStoryboard.OverlayLayerContainer.Add(drawableStoryboard.OverlayLayer.CreateProxy());
        });

        private void verifyStoryboardColourOffset() => AddUntilStep("Storyboard elements colour is offset", () =>
        {
            Colour4? spriteColourOffset = testStoryboard?.GetSpriteColourOffset();
            if (spriteColourOffset == null)
                return false;

            Colour4? animationColourOffset = testStoryboard?.GetAnimationColourOffset();
            if (animationColourOffset == null)
                return false;

            Colour4? videoColourOffset = testStoryboard?.GetVideoColourOffset();
            if (videoColourOffset == null)
                return false;

            Colour4 targetColourOffset = player.StoryboardTargetColourOffset();

            return spriteColourOffset == targetColourOffset && animationColourOffset == targetColourOffset && videoColourOffset == targetColourOffset;
        });

        private void verifyStoryboardColourNotOffset() => AddUntilStep("Storyboard elements colour is not offset", () =>
        {
            Colour4? spriteColourOffset = testStoryboard?.GetSpriteColourOffset();
            if (spriteColourOffset == null)
                return false;

            Colour4? animationColourOffset = testStoryboard?.GetAnimationColourOffset();
            if (animationColourOffset == null)
                return false;

            Colour4? videoColourOffset = testStoryboard?.GetVideoColourOffset();
            if (videoColourOffset == null)
                return false;

            return spriteColourOffset == Colour4.Black && animationColourOffset == Colour4.Black && videoColourOffset == Colour4.Black;
        });

        private void verifyAdditiveSpriteIsNotOffset() => AddUntilStep("Additive sprite is not offset", () =>
        {
            Colour4? additiveSpriteColourOffset = testStoryboard?.GetAdditiveSpriteColourOffset();
            if (additiveSpriteColourOffset == null)
                return false;

            return additiveSpriteColourOffset == Colour4.Black;
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
            AddUntilStep("Song select has selection", () => songSelect.Carousel?.SelectedBeatmapInfo != null);
            AddStep("Set default user settings", () =>
            {
                SelectedMods.Value = new[] { new OsuModNoFail() };
                songSelect.DimLevel.Value = 0.7f;
                songSelect.BlurLevel.Value = 0.4f;
                songSelect.DimColour.Value = 0.5f;
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            rulesets?.Dispose();
        }

        private partial class DummySongSelect : PlaySongSelect
        {
            private FadeAccessibleBackground background;

            protected override BackgroundScreen CreateBackground()
            {
                background = new FadeAccessibleBackground(Beatmap.Value);
                IgnoreUserSettings.BindTo(background.IgnoreUserSettings);
                return background;
            }

            public readonly Bindable<bool> IgnoreUserSettings = new Bindable<bool>();
            public readonly Bindable<double> DimLevel = new BindableDouble();
            public readonly Bindable<double> BlurLevel = new BindableDouble();
            public readonly Bindable<double> DimColour = new BindableDouble();

            public new BeatmapCarousel Carousel => base.Carousel;

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config)
            {
                config.BindWith(OsuSetting.DimLevel, DimLevel);
                config.BindWith(OsuSetting.BlurLevel, BlurLevel);
                config.BindWith(OsuSetting.DimColour, DimColour);
            }

            public bool IsBackgroundBlack() => background.CurrentColour == Colour4.Black && background.CurrentColourOffset == Colour4.Black;

            public bool IsBackgroundDimmed()
            {
                Colour4 targetDrawColour = new Colour4(
                    1f - background.CurrentDim,
                    1f - background.CurrentDim,
                    1f - background.CurrentDim,
                    1f
                );

                return background.CurrentColour == targetDrawColour;
            }

            public bool IsBackgroundFullyDimmed() => background.CurrentColour == Colour4.Black;

            public bool IsBackgroundUndimmed() => background.CurrentColour == background.ParentDrawColour && background.CurrentColourOffset == Colour4.Black;

            public Colour4 BackgroundTargetColourOffset() => background.ParentDrawColour * background.CurrentDimColour;

            public bool IsBackgroundColourOffset() => background.CurrentColourOffset == BackgroundTargetColourOffset();

            public bool IsUserBlurApplied() => Precision.AlmostEquals(background.CurrentBlur, new Vector2((float)BlurLevel.Value * BackgroundScreenBeatmap.USER_BLUR_FACTOR), 0.1f);

            public bool IsUserBlurDisabled() => background.CurrentBlur == new Vector2(0);

            public bool IsBackgroundVisible() => background.CurrentAlpha == 1;

            public bool IsBackgroundBlur() => Precision.AlmostEquals(background.CurrentBlur, new Vector2(BACKGROUND_BLUR), 0.1f);

            public bool CheckBackgroundBlur(Vector2 expected) => Precision.AlmostEquals(background.CurrentBlur, expected, 0.1f);

            /// <summary>
            /// Make sure every time a screen gets pushed, the background doesn't get replaced
            /// </summary>
            /// <returns>Whether or not the original background (The one created in DummySongSelect) is still the current background</returns>
            public bool IsBackgroundCurrent() => background?.IsCurrentScreen() == true;

            public bool IsSpriteDimmed() => background.IsSpriteDimmed;

            public bool IsBufferedContainerDimmed() => background.IsBufferedContainerDimmed;

            public bool IsBufferedContainerNull() => background.IsBufferedContainerNull;

            public bool RequiredRedraw
            {
                get => background.RequiredRedraw;
                set => background.RequiredRedraw = value;
            }
        }

        private partial class FadeAccessibleResults : ResultsScreen
        {
            public FadeAccessibleResults(ScoreInfo score)
                : base(score)
            {
                AllowRetry = true;
            }

            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground(Beatmap.Value);

            public Vector2 ExpectedBackgroundBlur => new Vector2(BACKGROUND_BLUR);
        }

        private partial class LoadBlockingTestPlayer : TestPlayer
        {
            protected override BackgroundScreen CreateBackground() =>
                new FadeAccessibleBackground(Beatmap.Value);

            public override void OnEntering(ScreenTransitionEvent e)
            {
                base.OnEntering(e);

                ApplyToBackground(b => StoryboardReplacesBackground.BindTo(b.StoryboardReplacesBackground));
            }

            public new DimmableStoryboard DimmableStoryboard => base.DimmableStoryboard;

            private TestDimmableStoryboard testDimmableStoryboard;

            protected override DimmableStoryboard CreateDimmableStoryboard(Storyboard storyboard, IReadOnlyList<Mod> mods) => testDimmableStoryboard = new TestDimmableStoryboard(storyboard, mods) { RelativeSizeAxes = Axes.Both };

            // Whether the player should be allowed to load.
            public bool BlockLoad;

            public Bindable<bool> StoryboardEnabled;
            public readonly Bindable<bool> StoryboardReplacesBackground = new Bindable<bool>();
            public readonly Bindable<bool> IsPaused = new Bindable<bool>();

            public LoadBlockingTestPlayer(bool allowPause = true)
                : base(allowPause)
            {
            }

            public bool IsStoryboardVisible => DimmableStoryboard.ContentDisplayed;

            public Colour4 StoryboardTargetColourOffset() => testDimmableStoryboard.TargetColourOffset();

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config, CancellationToken token)
            {
                while (BlockLoad && !token.IsCancellationRequested)
                    Thread.Sleep(1);

                if (!LoadedBeatmapSuccessfully)
                    return;

                StoryboardEnabled = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
                DrawableRuleset.IsPaused.BindTo(IsPaused);
            }
        }

        private partial class TestPlayerLoader : PlayerLoader
        {
            private FadeAccessibleBackground background;

            public VisualSettings VisualSettingsPos => VisualSettings;
            public BackgroundScreen ScreenPos => background;

            public TestPlayerLoader(Player player)
                : base(() => player)
            {
            }

            public void TriggerOnHover() => OnHover(new HoverEvent(new InputState()));

            public Vector2 ExpectedBackgroundBlur => new Vector2(BACKGROUND_BLUR);

            protected override BackgroundScreen CreateBackground() => background = new FadeAccessibleBackground(Beatmap.Value);
        }

        private partial class FadeAccessibleBackground : BackgroundScreenBeatmap
        {
            protected override DimmableBackground CreateFadeContainer() => dimmable = new TestDimmableBackground { RelativeSizeAxes = Axes.Both };

            protected override BeatmapBackground CreateBeatmapBackground(WorkingBeatmap beatmap) => beatmapBackground = new TestBeatmapBackground(beatmap);

            public Colour4 CurrentColour => beatmapBackground.CurrentColour;

            public Colour4 CurrentColourOffset => beatmapBackground.CurrentColourOffset;

            public bool IsSpriteDimmed => beatmapBackground.IsSpriteDimmed;

            public bool IsBufferedContainerDimmed => beatmapBackground.IsBufferedContainerDimmed;

            public bool IsBufferedContainerNull => beatmapBackground.IsBufferedContainerNull;

            public Colour4 ParentDrawColour => DrawColourInfo.Colour;

            public float CurrentAlpha => dimmable.CurrentAlpha;

            public float CurrentDim => dimmable.DimLevel;

            public Colour4 CurrentDimColour => dimmable.DimColour;

            public Vector2 CurrentBlur => Background?.BlurSigma ?? Vector2.Zero;

            public bool RequiredRedraw
            {
                get => beatmapBackground.RequiredRedraw;
                set => beatmapBackground.RequiredRedraw = value;
            }

            private TestDimmableBackground dimmable;

            private TestBeatmapBackground beatmapBackground;

            public FadeAccessibleBackground(WorkingBeatmap beatmap)
                : base(beatmap)
            {
            }
        }

        private partial class TestDimmableBackground : BackgroundScreenBeatmap.DimmableBackground
        {
            public float CurrentAlpha => Content.Alpha;

            public new float DimLevel => base.DimLevel;

            public new Colour4 DimColour => base.DimColour;
        }

        private partial class TestBeatmapBackground : BeatmapBackground
        {
            public static bool AllowBlur = true;

            // BeatmapBackground shader uses mix function to apply dimming with colour, which can be extended as:
            // mix(TextureColour, DimColour, DimLevel) = TextureColour * (1 - DimLevel) + DimColour * DimLevel
            // The result is then multiplied by vertex colour (supplied by DrawColourInfo.Colour),
            // which can apply some external dimming that shouldn't be affected by DimColour
            // (for example - opening settings during replay, pausing, etc.):
            // FinalColour = DrawColourInfo.Colour * mix(TextureColour, DimColour, DimLevel)
            // FinalColour = DrawColourInfo.Colour * TextureColour * (1 - DimLevel) + DrawColourInfo.Colour * DimColour * DimLevel
            //
            // These two parts can be split into separate variables:
            // CurrentColour = DrawColourInfo.Colour * TextureColour * (1 - DimLevel)
            // CurrentColourOffset = DrawColourInfo.Colour * DimColour * DimLevel
            //
            // CurrentColour can be used to track how much the texture was dimmed, and
            // CurrentColourOffset can be used to track how much colour was added as an offset.
            //
            // Two separate variables are needed because just one Colour variable would be ambiguous, for example:
            // if Colour == Colour4.White, that could mean either that
            // DimLevel == 0.0, or
            // DimLevel == 1.0 and DimColour == Colour4.White.
            public Colour4 CurrentColour => ColouredDimmableBufferedContainer?.FrameBufferDrawColour?.Colour ?? ColouredDimmableSprite.DrawColourInfo.Colour;

            public Colour4 CurrentColourOffset => ColouredDimmableBufferedContainer?.FrameBufferDrawColourOffset ?? (ColouredDimmableSprite as IColouredDimmable).DrawColourOffset;

            public bool IsSpriteDimmed => ColouredDimmableSprite.DrawColourInfo.Colour != Colour4.White;

            public bool IsBufferedContainerDimmed => ColouredDimmableBufferedContainer != null && ColouredDimmableBufferedContainer.FrameBufferDrawColour?.Colour != Colour4.White;

            public bool IsBufferedContainerNull => ColouredDimmableBufferedContainer == null;

            public bool RequiredRedraw
            {
                get => dimmableBufferedContainer.RequiredRedraw;
                set => dimmableBufferedContainer.RequiredRedraw = value;
            }

            private TestDimmableBufferedContainer dimmableBufferedContainer;

            public TestBeatmapBackground(WorkingBeatmap beatmap)
                : base(beatmap)
            {
            }

            public override void BlurTo(Vector2 newBlurSigma, double duration = 0, Easing easing = Easing.None)
            {
                if (AllowBlur)
                {
                    base.BlurTo(newBlurSigma, duration, easing);
                }
            }

            protected override BufferedContainer CreateBufferedContainer()
            {
                return ColouredDimmableBufferedContainer = dimmableBufferedContainer = new TestDimmableBufferedContainer(cachedFrameBuffer: true)
                {
                    RelativeSizeAxes = Axes.Both,
                    RedrawOnScale = false,
                    Child = Sprite,
                };
            }
        }

        private partial class TestDimmableBufferedContainer : BeatmapBackground.DimmableBufferedContainer
        {
            // Ideally this one would be tracked inside of the DrawNode and set to true
            // when framebuffer is redrawn, but DrawNode's are a bit broken in the
            // headless testing mode, so an indirect check is used (see RequiresChildrenUpdate).
            public bool RequiredRedraw;

            public TestDimmableBufferedContainer(RenderBufferFormat[] formats = null, bool pixelSnapping = false, bool cachedFrameBuffer = false)
                : base(formats, pixelSnapping, cachedFrameBuffer)
            {
            }

            protected override bool RequiresChildrenUpdate
            {
                get
                {
                    // A bit hacky, but at least it doesn't require exposing BufferedContainer.updateVersion
                    bool requiresChildrenUpdate = base.RequiresChildrenUpdate;
                    RequiredRedraw |= requiresChildrenUpdate;
                    return requiresChildrenUpdate;
                }
            }
        }

        private partial class TestStoryboard : Storyboard
        {
            private readonly TestStoryboardSprite testStoryboardSprite;
            private readonly TestStoryboardSprite testStoryboardAdditiveSprite;
            private readonly TestStoryboardAnimation testStoryboardAnimation;
            private readonly TestStoryboardVideo testStoryboardVideo;

            public TestStoryboard()
            {
                GetLayer("Foreground").Add(testStoryboardSprite = new TestStoryboardSprite(
                    path: "Resources/Textures/test-image.png",
                    origin: Anchor.Centre,
                    initialPosition: new Vector2(200, 200),
                    additiveBlending: false
                ));

                GetLayer("Foreground").Add(testStoryboardAdditiveSprite = new TestStoryboardSprite(
                    path: "Resources/Textures/test-image.png",
                    origin: Anchor.Centre,
                    initialPosition: new Vector2(500, 200),
                    additiveBlending: true
                ));

                GetLayer("Foreground").Add(testStoryboardAnimation = new TestStoryboardAnimation(
                    path: "Resources/Textures/test-image.png",
                    origin: Anchor.Centre,
                    initialPosition: new Vector2(0, 0),
                    frameCount: 10000,
                    frameDelay: 1000 / 60f,
                    loopType: AnimationLoopType.LoopForever
                ));

                GetLayer("Video").Add(testStoryboardVideo = new TestStoryboardVideo(
                    path: "Resources/Videos/test-video.mp4",
                    offset: 0
                ));
            }

            public Colour4? GetSpriteColourOffset() => testStoryboardSprite?.GetDrawColourOffset();
            public Colour4? GetAdditiveSpriteColourOffset() => testStoryboardAdditiveSprite?.GetDrawColourOffset();
            public Colour4? GetAnimationColourOffset() => testStoryboardAnimation?.GetDrawColourOffset();
            public Colour4? GetVideoColourOffset() => testStoryboardVideo?.GetDrawColourOffset();

            public override DrawableStoryboard CreateDrawable(IReadOnlyList<Mod>? mods = null)
            {
                return new TestDrawableStoryboard(this, mods);
            }

            private partial class TestDrawableStoryboard : DrawableStoryboard
            {
                public TestDrawableStoryboard(TestStoryboard storyboard, IReadOnlyList<Mod>? mods)
                    : base(storyboard, mods)
                {
                }

                protected override IResourceStore<byte[]> CreateResourceLookupStore() => new ResourcesTextureStore();

                internal class ResourcesTextureStore : IResourceStore<byte[]>
                {
                    private readonly DllResourceStore store;

                    public ResourcesTextureStore()
                    {
                        store = TestResources.GetStore();
                    }

                    public void Dispose() => store.Dispose();

                    public byte[] Get(string name) => store.Get(name);

                    public Task<byte[]> GetAsync(string name, CancellationToken cancellationToken = new CancellationToken()) => store.GetAsync(name, cancellationToken);

                    public Stream GetStream(string name) => store.GetStream(name);

                    public IEnumerable<string> GetAvailableResources() => store.GetAvailableResources();
                }
            }
        }

        private partial class TestStoryboardSprite : StoryboardSprite
        {
            private DrawableStoryboardSprite drawableStoryboardSprite;
            private readonly bool additiveBlending;

            public TestStoryboardSprite(string path, Anchor origin, Vector2 initialPosition, bool additiveBlending)
                : base(path, origin, initialPosition)
            {
                this.additiveBlending = additiveBlending;
                Commands.AddAlpha(Easing.InBounce, -3000, 1000, 0, 1);
            }

            public Colour4? GetDrawColourOffset() => (drawableStoryboardSprite as IColouredDimmable)?.DrawColourOffset;

            public override Drawable CreateDrawable() => drawableStoryboardSprite = new DrawableStoryboardSprite(this)
            {
                Blending = additiveBlending ? BlendingParameters.Additive : BlendingParameters.None
            };
        }

        private partial class TestStoryboardAnimation : StoryboardAnimation
        {
            private TestDrawableStoryboardAnimation testDrawableStoryboardAnimation;

            public TestStoryboardAnimation(string path, Anchor origin, Vector2 initialPosition, int frameCount, double frameDelay, AnimationLoopType loopType)
                : base(path, origin, initialPosition, frameCount, frameDelay, loopType)
            {
                Commands.AddAlpha(Easing.InBounce, -3000, 1000, 0, 1);
            }

            public Colour4? GetDrawColourOffset() => testDrawableStoryboardAnimation?.GetDrawColourOffset();

            public override Drawable CreateDrawable() => testDrawableStoryboardAnimation = new TestDrawableStoryboardAnimation(this);

            public partial class TestDrawableStoryboardAnimation : DrawableStoryboardAnimation
            {
                private BeatmapBackground.DimmableSprite dimmableSprite;

                public TestDrawableStoryboardAnimation(StoryboardAnimation storyboardAnimation)
                    : base(storyboardAnimation)
                {
                }

                public Colour4? GetDrawColourOffset() => (dimmableSprite as IColouredDimmable)?.DrawColourOffset;

                protected override Sprite CreateSprite() => dimmableSprite = new BeatmapBackground.DimmableSprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            }
        }

        private partial class TestStoryboardVideo : StoryboardVideo
        {
            private TestDrawableStoryboardVideo testDrawableStoryboardVideo;

            public TestStoryboardVideo(string path, double offset)
                : base(path, offset)
            {
                Commands.AddAlpha(Easing.InBounce, -3000, 1000, 0, 1);
            }

            public Colour4? GetDrawColourOffset() => testDrawableStoryboardVideo?.GetDrawColourOffset();

            public override Drawable CreateDrawable() => testDrawableStoryboardVideo = new TestDrawableStoryboardVideo(this);

            public partial class TestDrawableStoryboardVideo : DrawableStoryboardVideo
            {
                private TestDrawableVideo testDrawableVideo;

                public TestDrawableStoryboardVideo(StoryboardVideo storyboardVideo)
                    : base(storyboardVideo)
                {
                }

                public Colour4? GetDrawColourOffset() => testDrawableVideo?.GetDrawColourOffset();

                protected override DrawableVideo CreateDrawableVideo(Stream stream, bool startAtCurrentTime) => testDrawableVideo = new TestDrawableVideo(stream, startAtCurrentTime)
                {
                    RelativeSizeAxes = RelativeSizeAxes,
                    FillMode = FillMode.Fill,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                };

                protected partial class TestDrawableVideo : DrawableVideo
                {
                    private DrawableVideoSprite drawableVideoSprite;

                    public TestDrawableVideo(Stream stream, bool startAtCurrentTime)
                        : base(stream, startAtCurrentTime)
                    {
                    }

                    public Colour4? GetDrawColourOffset() => (drawableVideoSprite as IColouredDimmable)?.DrawColourOffset;

                    protected override VideoSprite CreateSprite() => drawableVideoSprite = new DrawableVideoSprite(this);
                }
            }
        }

        private partial class TestDimmableStoryboard : DimmableStoryboard
        {
            public TestDimmableStoryboard(Storyboard storyboard, IReadOnlyList<Mod> mods)
                : base(storyboard, mods)
            {
            }

            public Colour4 TargetColourOffset() => DimColour * DrawColourInfo.Colour;
        }
    }
}
