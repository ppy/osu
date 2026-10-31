// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
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
using osu.Game.Storyboards.Drawables;
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
        private UpdateCounter storyboardUpdateCounter;
        private readonly TestColouredDimmableSprite orphanedSprite = new TestColouredDimmableSprite();
        private IShader originalShader;
        private IShader colouredShader;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio, ShaderManager shaders)
        {
            BeatmapStore beatmapStore;

            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(new OsuConfigManager(LocalStorage));
            Dependencies.CacheAs(beatmapStore = new RealmDetachedBeatmapStore());
            Dependencies.Cache(Realm);

            manager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();

            Add(beatmapStore);

            Beatmap.SetDefault();

            originalShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
            colouredShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ColouredDimmableTexture");

            Add(orphanedSprite);
        }

        public override void SetUpSteps()
        {
            SetUpStepsWithBlur();
        }

        public void SetUpStepsWithBlur(bool allowBlur = true)
        {
            base.SetUpSteps();

            AddStep("push song select", () => Stack.Push(songSelect = new DummySongSelect(allowBlur)));
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
            AddUntilStep("Background is fully dimmed, storyboard is visible", () => songSelect.IsBackgroundVisible() && songSelect.IsBackgroundFullyDimmed() && player.IsStoryboardVisible);
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
        public void TestStoryboardUpdatesWhenDimmed()
        {
            performFullSetup();
            createFakeStoryboard();

            AddStep("Enable fully dimmed storyboard", () =>
            {
                player.StoryboardReplacesBackground.Value = true;
                player.StoryboardEnabled.Value = true;
                player.DimmableStoryboard.IgnoreUserSettings.Value = false;
                songSelect.DimLevel.Value = 1f;
            });

            AddUntilStep("Storyboard is invisible", () => !player.IsStoryboardVisible);

            AddWaitStep("wait some", 20);

            AddUntilStep("Storyboard is always present", () => player.ChildrenOfType<DrawableStoryboard>().Single().AlwaysPresent, () => Is.True);
            AddUntilStep("Dimmable storyboard content is being updated", () => storyboardUpdateCounter.StoryboardContentLastUpdated, () => Is.EqualTo(Time.Current).Within(100));
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

            AddStep("Transition to Results", () =>
            {
                player.ValidForResume = false;
                player.Push(results = new FadeAccessibleResults(TestResources.CreateTestScoreInfo()));
            });

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
            SetUpStepsWithBlur(false);

            performFullSetup();

            AddUntilStep("Screen is dimmed and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsBackgroundColourOffset());
            AddUntilStep("BufferedContainer is not initialized", () => songSelect.IsBufferedContainerNull());
            AddUntilStep("Sprite is dimmed", () => songSelect.IsSpriteDimmed());

            SetUpStepsWithBlur(true);

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
            UpdateCounter updateCounter = new UpdateCounter();
            double lastUpdate = 0.0;

            performFullSetup();

            AddStep("Add update counter to BufferedContainer", () => songSelect.AddToBufferedContainer(updateCounter));

            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset());
            AddUntilStep("BufferedContainer is initialized", () => !songSelect.IsBufferedContainerNull());

            AddStep("Start tracking redraws", () => lastUpdate = updateCounter.StoryboardContentLastUpdated);

            AddStep("Undim the screen", () =>
            {
                songSelect.DimLevel.Value = 0.0;
                songSelect.DimColour.Value = 0.0;
            });
            AddUntilStep("Screen is undimmed and blur applied", () => songSelect.IsBackgroundUndimmed() && songSelect.IsUserBlurApplied());
            AddUntilStep("Redraw wasn't required", () => lastUpdate == updateCounter.StoryboardContentLastUpdated);

            AddStep("Dim the screen", () =>
            {
                songSelect.DimLevel.Value = 0.7;
                songSelect.DimColour.Value = 0.5;
            });
            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset());
            AddUntilStep("Redraw wasn't required", () => lastUpdate == updateCounter.StoryboardContentLastUpdated);
        }

        /// <summary>
        /// Check if storyboard interacts properly with background.
        /// </summary>
        [Test]
        public void TestStoryboardEffectOnBackground()
        {
            performFullSetup();
            createFakeStoryboardWithElements();

            AddStep("Enable Storyboard", () =>
            {
                player.StoryboardEnabled.Value = true;
            });

            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset());

            AddStep("Replace background", () =>
            {
                player.StoryboardReplacesBackground.Value = true;
            });

            AddUntilStep("Background is fully dimmed and dim colour adjusted", () => songSelect.IsBackgroundFullyDimmed() && songSelect.IsBackgroundColourOffset());

            AddStep("Ignore user settings", () =>
            {
                player.ApplyToBackground(b => b.IgnoreUserSettings.Value = true);
                player.DimmableStoryboard.IgnoreUserSettings.Value = true;
            });

            AddUntilStep("Background is black", () => songSelect.IsBackgroundBlack());
        }

        [Test]
        public void TestStoryboardBackgroundNotOffsetWhenReplacedButNotDimmed()
        {
            performFullSetup();
            createFakeStoryboard();

            AddStep("Enable undimmed storyboard with replaced background", () =>
            {
                player.StoryboardReplacesBackground.Value = true;
                player.StoryboardEnabled.Value = true;
                player.DimmableStoryboard.IgnoreUserSettings.Value = false;
                songSelect.DimLevel.Value = 0f;
                songSelect.DimColour.Value = 1f;
            });

            AddUntilStep("Background is black", () => songSelect.IsBackgroundBlack());

            AddStep("Enable dimming", () =>
            {
                songSelect.DimLevel.Value = 0.5f;
            });

            AddUntilStep("Background's colour is offset", () => songSelect.IsBackgroundColourOffset());
        }

        /// <summary>
        /// Check if both the Sprite and BufferedContainer select correct colour offset source.
        /// </summary>
        [Test]
        public void TestDimmingOffsetSources()
        {
            SetUpStepsWithBlur(false);

            performFullSetup();

            AddUntilStep("Screen is dimmed and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsBackgroundColourOffset());
            AddUntilStep("BufferedContainer is not initialized", () => songSelect.IsBufferedContainerNull());
            AddUntilStep("Sprite's offset source is DimmableBackground", () => songSelect.IsSpritesOffsetSourceDimmableBackground());

            SetUpStepsWithBlur(true);

            performFullSetup();

            AddUntilStep("Screen is dimmed, blur applied and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsUserBlurApplied() && songSelect.IsBackgroundColourOffset());
            AddUntilStep("BufferedContainer is initialized", () => !songSelect.IsBufferedContainerNull());
            AddUntilStep("BufferedContainer's offset source is DimmableBackground", () => songSelect.IsBufferedContainersOffsetSourceDimmableBackground());
            AddUntilStep("Sprite's offset source is BufferedContainer", () => songSelect.IsSpritesOffsetSourceBufferedContainer());
        }

        /// <summary>
        /// Check if storyboard elements select correct colour offset source.
        /// </summary>
        [Test]
        public void TestStoryboardOffsetSources()
        {
            performFullSetup();
            createFakeStoryboardWithElements();

            AddStep("Enable Storyboard", () =>
            {
                player.StoryboardEnabled.Value = true;
            });

            AddUntilStep("Sprite's source is DimmableStoryboard", () => testStoryboard.IsSpritesOffsetSource(player.DimmableStoryboard));
            AddUntilStep("Additive sprite's source is DimmableStoryboard", () => testStoryboard.IsAdditiveSpritesOffsetSource(player.DimmableStoryboard));
            AddUntilStep("Animation's source is DimmableStoryboard", () => testStoryboard.IsAnimationsOffsetSource(player.DimmableStoryboard));
            AddUntilStep("Video's source is DimmableStoryboard", () => testStoryboard.IsVideosOffsetSource(player.DimmableStoryboard));
        }

        /// <summary>
        /// CurrentColour and CurrentColourOffset are interpolated in linear, then converted to sRGB
        /// They're then added together, which can cause total brightness to go above or below 1.0
        /// </summary>
        [Test]
        public void TestBrightnessStabilityDuringTransition()
        {
            ManualClock manualClock = new ManualClock();
            double startTime = 0.0;

            performFullSetup();

            AddStep("Undim the screen and set DimColour", () =>
            {
                songSelect.DimLevel.Value = 0.0;
                songSelect.DimColour.Value = 1.0;
            });
            AddUntilStep("Screen is undimmed", () => songSelect.IsBackgroundUndimmed());

            AddStep("Prepare clock", () =>
            {
                manualClock.CurrentTime = songSelect.GetCurrentBackgroundTime();
                songSelect.SetBackgroundClock(new FramedClock(manualClock));
            });

            AddStep("Dim the screen", () =>
            {
                startTime = manualClock.CurrentTime;
                songSelect.DimLevel.Value = 1.0;
            });

            for (int i = 1; i < 8; i++)
            {
                double offset = i * UserDimContainer.BACKGROUND_FADE_DURATION / 8;
                AddStep("Set time", () => manualClock.CurrentTime = startTime + offset);
                AddAssert("Transition is in progress", () => !songSelect.IsBackgroundDimmed() && !songSelect.IsBackgroundUndimmed());
                AddAssert("Brightness is one", () => songSelect.IsBrightnessOne());
            }
        }

        [Test]
        public void TestStoryboardBrightnessStabilityDuringTransition()
        {
            ManualClock manualClock = new ManualClock();
            double startTime = 0.0;

            performFullSetup();
            createFakeStoryboardWithElements();

            AddStep("Enable Storyboard", () =>
            {
                player.StoryboardEnabled.Value = true;
            });

            AddStep("Undim the screen and set DimColour", () =>
            {
                songSelect.DimLevel.Value = 0.0;
                songSelect.DimColour.Value = 1.0;
            });
            AddUntilStep("Screen is undimmed", () => songSelect.IsBackgroundUndimmed());

            AddStep("Prepare clock", () =>
            {
                manualClock.CurrentTime = player.DimmableStoryboard.Time.Current;
                player.DimmableStoryboard.Clock = new FramedClock(manualClock);
            });

            AddStep("Dim the screen", () =>
            {
                startTime = manualClock.CurrentTime;
                songSelect.DimLevel.Value = 1.0;
            });

            for (int i = 1; i < 8; i++)
            {
                double offset = i * UserDimContainer.BACKGROUND_FADE_DURATION / 8;
                AddStep("Set time", () => manualClock.CurrentTime = startTime + offset);
                AddAssert("Brightness is one", () => player.IsStoryboardBrightnessOne());
            }
        }

        [Test]
        public void TestParentDimming()
        {
            performFullSetup();

            AddUntilStep("Screen is dimmed and dim colour adjusted", () => songSelect.IsBackgroundDimmed() && songSelect.IsBackgroundColourOffset());

            AddStep("Dim the background directly", () => songSelect.SetBackgroundColour(new Colour4(0.5f, 0.5f, 0.5f, 1f)));

            AddUntilStep("Background is dimmed", () => songSelect.GetBackgroundColour() == new Colour4(0.5f, 0.5f, 0.5f, 1f));

            AddAssert("Parent colour is applied correctly", () => songSelect.IsBackgroundFinalColourCorrect());
        }

        [Test]
        public void TestSpriteShaders()
        {
            performFullSetup();
            createFakeStoryboardWithElements();

            AddStep("Enable Storyboard", () =>
            {
                player.StoryboardEnabled.Value = true;
            });

            AddUntilStep("Opaque sprite is using coloured shader", () => testStoryboard.IsSpriteUsingShader(colouredShader));
            AddUntilStep("Additive sprite is using original shader", () => testStoryboard.IsAdditiveSpriteUsingShader(originalShader));

            AddAssert("No source on orphaned sprite", () => orphanedSprite.ColouredDimmableHelper.Source == null);
            AddUntilStep("Orphaned sprite is using original shader", () => orphanedSprite.IsUsingShader(originalShader));
        }

        private void createFakeStoryboard() => AddStep("Create storyboard", () =>
        {
            player.StoryboardEnabled.Value = false;
            player.StoryboardReplacesBackground.Value = false;
            player.DimmableStoryboard.AddRange(new Drawable[]
            {
                storyboardUpdateCounter = new UpdateCounter(),
                new OsuSpriteText
                {
                    Size = new Vector2(500, 50),
                    Alpha = 1,
                    Colour = Colour4.White,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "THIS IS A STORYBOARD",
                    Font = new FontUsage(size: 50)
                }
            });
        });

        private void createFakeStoryboardWithElements() => AddStep("Create storyboard with elements", () =>
        {
            player.StoryboardEnabled.Value = false;
            player.StoryboardReplacesBackground.Value = false;

            testStoryboard = new TestStoryboard();

            player.DrawableStoryboard = testStoryboard.CreateDrawable();

            player.DimmableStoryboard.Add(player.DrawableStoryboard);
            player.DimmableStoryboard.OverlayLayerContainer.Add(player.DrawableStoryboard.OverlayLayer.CreateProxy());
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
            AddUntilStep("Song select has selection", () => songSelect.Carousel?.CurrentGroupedBeatmap != null);
            AddStep("Set default user settings", () =>
            {
                SelectedMods.Value = new[] { new OsuModNoFail() };
                songSelect.DimLevel.Value = 0.7f;
                songSelect.BlurLevel.Value = songSelect.AllowBlur ? 0.4f : 0f;
                songSelect.DimColour.Value = 0.5f;
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            rulesets?.Dispose();
        }

        private partial class DummySongSelect : SoloSongSelect
        {
            public readonly bool AllowBlur;
            private FadeAccessibleBackground background;

            protected override BackgroundScreen CreateBackground()
            {
                background = new FadeAccessibleBackground(Beatmap.Value, AllowBlur);
                IgnoreUserSettings.BindTo(background.IgnoreUserSettings);
                return background;
            }

            public readonly Bindable<bool> IgnoreUserSettings = new Bindable<bool>();
            public readonly Bindable<double> DimLevel = new BindableDouble();
            public readonly Bindable<double> BlurLevel = new BindableDouble();
            public readonly Bindable<double> DimColour = new BindableDouble();

            public BeatmapCarousel Carousel => this.ChildrenOfType<BeatmapCarousel>().SingleOrDefault();

            public DummySongSelect(bool allowBlur = true)
            {
                AllowBlur = allowBlur;
            }

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

            public bool IsBackgroundUndimmed() => CurrentColourAlmostEqualsParentColour() && background.CurrentColourOffset == Colour4.Black;

            public bool CurrentColourAlmostEqualsParentColour()
            {
                float scale = ParentColourScale();
                return Precision.AlmostEquals(background.CurrentColour.R, background.ParentDrawColour.R * scale, 0.01f) &&
                       Precision.AlmostEquals(background.CurrentColour.G, background.ParentDrawColour.G * scale, 0.01f) &&
                       Precision.AlmostEquals(background.CurrentColour.B, background.ParentDrawColour.B * scale, 0.01f) &&
                       Precision.AlmostEquals(background.CurrentColour.A, background.ParentDrawColour.A, 0.01f);
            }

            public float ParentColourScale() => background.IgnoreUserSettings.Value ? (1f - background.DimWhenUserSettingsIgnored.Value) : 1f;

            public Colour4 BackgroundTargetColourOffset() =>
                background.ParentDrawColour == Colour4.White ? background.TargetColourOffset : (background.ParentDrawColour.ToLinear() * background.TargetColourOffset.ToLinear()).ToSRGB();

            public bool IsBackgroundColourOffset() => background.CurrentColourOffset == BackgroundTargetColourOffset();

            public void SetBackgroundColour(Colour4 colour)
            {
                background.Colour = colour;
            }

            public Colour4 GetBackgroundColour() => background.Colour;

            public bool IsBackgroundFinalColourCorrect()
            {
                // See ColouredUserDimContainer for more info
                Colour4 colour = new Colour4(
                    1f - background.CurrentDim,
                    1f - background.CurrentDim,
                    1f - background.CurrentDim,
                    1f
                );

                Colour4 targetColour = colour + background.TargetColourOffset;
                targetColour = new Colour4(targetColour.R, targetColour.G, targetColour.B, 1f);

                Colour4 finalColour = (background.ParentDrawColour.ToLinear() * targetColour.ToLinear()).ToSRGB();

                Colour4 actualColour = background.CurrentColour + background.CurrentColourOffset;

                return Precision.AlmostEquals(finalColour.R, actualColour.R, 0.001f) &&
                       Precision.AlmostEquals(finalColour.G, actualColour.G, 0.001f) &&
                       Precision.AlmostEquals(finalColour.B, actualColour.B, 0.001f);
            }

            public bool IsUserBlurApplied() => Precision.AlmostEquals(background.CurrentBlur, new Vector2((float)BlurLevel.Value * BackgroundScreenBeatmap.USER_BLUR_FACTOR), 0.1f);

            public bool IsUserBlurDisabled() => background.CurrentBlur == new Vector2(0);

            public bool IsBackgroundVisible() => background.CurrentAlpha == 1;

            public bool IsBackgroundBlur() => Precision.AlmostBigger(background.CurrentBlur.X, 0, 0.1f);

            public bool CheckBackgroundBlur(Vector2 expected) => Precision.AlmostEquals(background.CurrentBlur, expected, 0.1f);

            /// <summary>
            /// Make sure every time a screen gets pushed, the background doesn't get replaced
            /// </summary>
            /// <returns>Whether the original background (The one created in DummySongSelect) is still the current background</returns>
            public bool IsBackgroundCurrent() => background?.IsCurrentScreen() == true;

            public bool IsSpriteDimmed() => background.IsSpriteDimmed;

            public bool IsBufferedContainerDimmed() => background.IsBufferedContainerDimmed;

            public bool IsBufferedContainerNull() => background.IsBufferedContainerNull;

            public void AddToBufferedContainer(Drawable drawable) => background.AddToBufferedContainer(drawable);

            public bool IsSpritesOffsetSourceDimmableBackground() => background.IsSpritesOffsetSourceDimmableBackground;

            public bool IsSpritesOffsetSourceBufferedContainer() => background.IsSpritesOffsetSourceBufferedContainer;

            public bool IsBufferedContainersOffsetSourceDimmableBackground() => background.IsBufferedContainersOffsetSourceDimmableBackground;

            public bool IsBrightnessOne() => Precision.AlmostEquals(background.CurrentColour.R + background.CurrentColourOffset.R, 1f, 0.01f);

            public double GetCurrentBackgroundTime() => background.Time.Current;

            public void SetBackgroundClock(IFrameBasedClock newClock) => background.Clock = newClock;
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

            public DrawableStoryboard DrawableStoryboard;

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

            public bool IsStoryboardBrightnessOne() => Precision.AlmostEquals(((Colour4)DrawableStoryboard.DrawColourInfo.Colour).R + DimmableStoryboard.DrawColourOffset.R, 1f, 0.01f);

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
            protected override DimmableBackground CreateFadeContainer() => dimmable = new TestDimmableBackground();

            // CurrentColour can be used to track how much the texture was dimmed, and
            // CurrentColourOffset can be used to track how much colour was added as an offset.
            //
            // Two separate variables are needed because just one Colour variable would be ambiguous, for example:
            // if Colour == Colour4.White, that could mean either that
            // DimLevel == 0.0, or
            // DimLevel == 1.0 and DimColour == Colour4.White.
            public Colour4 CurrentColour => colouredDimmableBufferedContainer?.FrameBufferDrawColour?.Colour ?? colouredDimmableSprite.DrawColourInfo.Colour;

            public Colour4 CurrentColourOffset => dimmable.DrawColourOffset;

            public bool IsSpriteDimmed => colouredDimmableSprite.DrawColourInfo.Colour != Colour4.White;

            public bool IsBufferedContainerDimmed => colouredDimmableBufferedContainer != null && colouredDimmableBufferedContainer.FrameBufferDrawColour?.Colour != Colour4.White;

            public bool IsBufferedContainerNull => colouredDimmableBufferedContainer == null;

            public Colour4 ParentDrawColour => DrawColourInfo.Colour;

            public float CurrentAlpha => dimmable.CurrentAlpha;

            public float CurrentDim => dimmable.DimLevel;

            public Colour4 TargetColourOffset => new Colour4(
                dimmable.DimColour.R * dimmable.DimLevelNoStoryboard,
                dimmable.DimColour.G * dimmable.DimLevelNoStoryboard,
                dimmable.DimColour.B * dimmable.DimLevelNoStoryboard,
                dimmable.DimColour.A
            );

            public Vector2 CurrentBlur => Background?.BlurSigma ?? Vector2.Zero;

            public bool IsSpritesOffsetSource(IColouredDimmableSource source) => ReferenceEquals(colouredDimmableSprite.ColouredDimmableHelper.Source, source);

            public bool IsSpritesOffsetSourceDimmableBackground => IsSpritesOffsetSource(dimmable);

            public bool IsSpritesOffsetSourceBufferedContainer => IsSpritesOffsetSource(colouredDimmableBufferedContainer);

            public bool IsBufferedContainersOffsetSourceDimmableBackground => ReferenceEquals(colouredDimmableBufferedContainer.ColouredDimmableHelper.Source, dimmable);

            public void AddToBufferedContainer(Drawable drawable) => colouredDimmableBufferedContainer.Add(drawable);

            private ColouredDimmableSprite colouredDimmableSprite => dimmable.ChildrenOfType<ColouredDimmableSprite>().Single();

            private ColouredDimmableBufferedContainer colouredDimmableBufferedContainer => dimmable.ChildrenOfType<ColouredDimmableBufferedContainer>().SingleOrDefault();

            private TestDimmableBackground dimmable;

            public FadeAccessibleBackground(WorkingBeatmap beatmap, bool allowBlur = true)
                : base(beatmap)
            {
                if (!allowBlur)
                {
                    // `dimmable` is assigned in `CreateFadeContainer`, which is called in
                    // base constructor, but ReSharper cannot see it
                    // ReSharper disable once PossibleNullReferenceException
                    dimmable.BlurAmount.UnbindAll();
                    dimmable.BlurAmount.Value = 0f;
                }
            }
        }

        private partial class UpdateCounter : Drawable
        {
            public double StoryboardContentLastUpdated;

            protected override void Update()
            {
                base.Update();
                StoryboardContentLastUpdated = Time.Current;
            }
        }

        private partial class TestDimmableBackground : BackgroundScreenBeatmap.DimmableBackground
        {
            public float CurrentAlpha => Content.Alpha;

            public new float DimLevel => base.DimLevel;

            public new float DimLevelNoStoryboard => base.DimLevelNoStoryboard;

            public new Colour4 DimColour => base.DimColour;
        }

        public partial class TestColouredDimmableSprite : ColouredDimmableSprite
        {
            public bool IsUsingShader(IShader shader) => shader != null && ReferenceEquals(shader, TextureShader);
        }

        private partial class TestStoryboard : Storyboard
        {
            private readonly StoryboardSprite testStoryboardSprite;
            private readonly StoryboardSprite testStoryboardAdditiveSprite;
            private readonly StoryboardAnimation testStoryboardAnimation;

            private TestDrawableStoryboard testDrawableStoryboard;

            public TestStoryboard()
            {
                GetLayer("Foreground").Add(testStoryboardSprite = new StoryboardSprite(
                    source: StoryboardElementSource.Beatmap,
                    path: "Resources/Textures/test-image.png",
                    origin: Anchor.Centre,
                    initialPosition: new Vector2(200, 200)
                ));
                testStoryboardSprite.Commands.AddAlpha(Easing.InBounce, -3000, 1000, 0, 1);

                GetLayer("Foreground").Add(testStoryboardAdditiveSprite = new StoryboardSprite(
                    source: StoryboardElementSource.Beatmap,
                    path: "Resources/Textures/test-image.png",
                    origin: Anchor.Centre,
                    initialPosition: new Vector2(500, 200)
                ));
                testStoryboardAdditiveSprite.Commands.AddAlpha(Easing.InBounce, -3000, 1000, 0, 1);
                testStoryboardAdditiveSprite.Commands.AddBlendingParameters(
                    Easing.None,
                    -3000,
                    -3000,
                    BlendingParameters.Additive,
                    BlendingParameters.Additive
                );

                GetLayer("Foreground").Add(testStoryboardAnimation = new StoryboardAnimation(
                    source: StoryboardElementSource.Beatmap,
                    path: "Resources/Textures/test-image.png",
                    origin: Anchor.Centre,
                    initialPosition: new Vector2(0, 0),
                    frameCount: 10000,
                    frameDelay: 1000 / 60f,
                    loopType: AnimationLoopType.LoopForever
                ));
                testStoryboardAnimation.Commands.AddAlpha(Easing.InBounce, -3000, 1000, 0, 1);

                StoryboardVideo testStoryboardVideo;
                GetLayer("Video").Add(testStoryboardVideo = new StoryboardVideo(
                    source: StoryboardElementSource.Beatmap,
                    path: "Resources/Videos/test-video.mp4",
                    offset: 0
                ));
                testStoryboardVideo.Commands.AddAlpha(Easing.InBounce, -3000, 1000, 0, 1);
            }

            private DrawableStoryboardSprite drawableSprite => testDrawableStoryboard.ChildrenOfType<DrawableStoryboardSprite>().Single(d => d.Sprite == testStoryboardSprite);

            private DrawableStoryboardSprite drawableAdditiveSprite => testDrawableStoryboard.ChildrenOfType<DrawableStoryboardSprite>().Single(d => d.Sprite == testStoryboardAdditiveSprite);

            private DrawableStoryboardAnimation drawableAnimation => testDrawableStoryboard.ChildrenOfType<DrawableStoryboardAnimation>().Single(d => d.Animation == testStoryboardAnimation);

            private ColouredDimmableSprite drawableAnimationSprite => drawableAnimation.ChildrenOfType<ColouredDimmableSprite>().Single();

            private ColouredDimmableVideoSprite drawableVideoSprite => testDrawableStoryboard.ChildrenOfType<ColouredDimmableVideoSprite>().Single();

            public bool IsSpritesOffsetSource(IColouredDimmableSource source) => ReferenceEquals(drawableSprite.ColouredDimmableHelper.Source, source);

            public bool IsAdditiveSpritesOffsetSource(IColouredDimmableSource source) => ReferenceEquals(drawableAdditiveSprite.ColouredDimmableHelper.Source, source);

            public bool IsAnimationsOffsetSource(IColouredDimmableSource source) => ReferenceEquals(drawableAnimationSprite.ColouredDimmableHelper.Source, source);

            public bool IsVideosOffsetSource(IColouredDimmableSource source) => ReferenceEquals(drawableVideoSprite.ColouredDimmableHelper.Source, source);

            public bool IsSpriteUsingShader(IShader shader) => shader != null && ReferenceEquals(shader, drawableSprite.TextureShader);

            public bool IsAdditiveSpriteUsingShader(IShader shader) => shader != null && ReferenceEquals(shader, drawableAdditiveSprite.TextureShader);

            public override DrawableStoryboard CreateDrawable(IReadOnlyList<Mod> mods = null)
            {
                return testDrawableStoryboard = new TestDrawableStoryboard(this, mods);
            }

            private partial class TestDrawableStoryboard : DrawableStoryboard
            {
                public TestDrawableStoryboard(TestStoryboard storyboard, IReadOnlyList<Mod> mods)
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
    }
}
