// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Tests.Beatmaps;
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
            typeof(UserDimContainer)
        };

        private DummySongSelect songSelect;
        private DimAccessiblePlayerLoader playerLoader;
        private DimAccessiblePlayer player;
        private readonly ScreenStack screen;

        [Cached]
        private BackgroundScreenStack backgroundStack;

        public TestCaseBackgroundScreenBeatmap()
        {
            InputManager.Add(backgroundStack = new BackgroundScreenStack { RelativeSizeAxes = Axes.Both });
            InputManager.Add(screen = new ScreenStack { RelativeSizeAxes = Axes.Both });

            AddStep("Create beatmap", () =>
            {
                Beatmap.Value = new TestWorkingBeatmap(new Beatmap<OsuHitObject>
                {
                    HitObjects =
                    {
                        new HitCircle
                        {
                            StartTime = 3000,
                            Position = new Vector2(0, 0),
                        },
                        new HitCircle
                        {
                            StartTime = 15000,
                            Position = new Vector2(0, 0),
                        }
                    },
                });
            });
        }

        /// <summary>
        /// Check if PlayerLoader properly triggers background dim previews when a user hovers over the visual settings panel.
        /// </summary>
        [Test]
        public void PlayerLoaderSettingsHoverTest()
        {
            createSongSelect();
            AddStep("Start player loader", () => songSelect.Push(playerLoader = new DimAccessiblePlayerLoader(player = new DimAccessiblePlayer())));
            AddUntilStep(() => playerLoader?.IsLoaded ?? false, "Wait for Player Loader to load");
            AddAssert("Background retained from song select", () => songSelect.IsBackgroundCurrent());
            AddStep("Trigger background preview", () =>
            {
                InputManager.MoveMouseTo(playerLoader.ScreenPos);
                InputManager.MoveMouseTo(playerLoader.VisualSettingsPos);
            });

            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is dimmed", () => songSelect.IsBackgroundDimmed());
        }

        /// <summary>
        /// In the case of a user triggering the dim preview the instant player gets loaded, then moving the cursor off of the visual settings:
        /// The OnHover of PlayerLoader will trigger, which could potentially trigger an undim unless checked for in PlayerLoader.
        /// We need to check that in this scenario, the dim is still properly applied after entering player.
        /// </summary>
        [Test]
        public void PlayerLoaderTransitionTest()
        {
            createSongSelect();
            AddStep("Start player loader", () => songSelect.Push(playerLoader = new DimAccessiblePlayerLoader(player = new DimAccessiblePlayer())));
            AddUntilStep(() => playerLoader?.IsLoaded ?? false, "Wait for Player Loader to load");
            AddStep("Allow beatmap to load", () =>
            {
                player.Ready = true;
                InputManager.MoveMouseTo(playerLoader.ScreenPos);
            });
            AddUntilStep(() => player?.IsLoaded ?? false, "Wait for player to load");
            AddAssert("Background retained from song select", () => songSelect.IsBackgroundCurrent());
            AddStep("Trigger background preview when loaded", () =>
            {
                InputManager.MoveMouseTo(playerLoader.VisualSettingsPos);
                InputManager.MoveMouseTo(playerLoader.ScreenPos);
            });
            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is dimmed", () => songSelect.IsBackgroundDimmed());
        }

        /// <summary>
        /// Make sure the background is fully invisible (Alpha == 0) when the background should be disabled by the storyboard.
        /// </summary>
        [Test]
        public void StoryboardBackgroundVisibilityTest()
        {
            performSetup();
            AddStep("Enable storyboard", () =>
            {
                player.ReplacesBackground.Value = true;
                player.StoryboardEnabled.Value = true;
            });
            AddWaitStep(5, "Wait for dim");
            AddAssert("Background is invisible", () => songSelect.IsBackgroundInvisible());
            AddStep("Disable storyboard", () => player.ReplacesBackground.Value = false);
            AddWaitStep(5, "Wait for dim");
            AddAssert("Background is visible", () => songSelect.IsBackgroundVisible());
        }

        /// <summary>
        /// When exiting player, the screen that it suspends/exits to needs to have a fully visible (Alpha == 1) background.
        /// </summary>
        [Test]
        public void StoryboardTransitionTest()
        {
            performSetup();
            AddStep("Enable storyboard", () =>
            {
                player.ReplacesBackground.Value = true;
                player.StoryboardEnabled.Value = true;
            });
            AddUntilStep(() =>
            {
                if (!songSelect.IsCurrentScreen())
                {
                    songSelect.MakeCurrent();
                    return false;
                }

                return true;
            }, "Wait for song select is current");
            AddWaitStep(5, "Wait for dim");
            AddAssert("Background is visible", () => songSelect.IsBackgroundVisible());
        }

        /// <summary>
        /// Check if the fade container is properly being reset when screen dim is disabled.
        /// </summary>
        [Test]
        public void DisableUserDimTest()
        {
            performSetup();
            AddStep("Test User Undimming", () => songSelect.DimEnabled.Value = false);
            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is undimmed", () => songSelect.IsBackgroundUndimmed());
        }

        /// <summary>
        /// Check if the fade container is properly being faded when screen dim is enabled.
        /// </summary>
        [Test]
        public void EnableUserDimTest()
        {
            performSetup();
            AddStep("Test User Dimming", () => songSelect.DimEnabled.Value = true);
            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is dimmed", () => songSelect.IsBackgroundDimmed());
        }

        /// <summary>
        /// Check if the fade container retains dim when pausing
        /// </summary>
        [Test]
        public void PauseTest()
        {
            performSetup();
            AddStep("Transition to Pause", () =>
            {
                if (!player.IsPaused.Value)
                    player.Exit();
            });
            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is dimmed", () => songSelect.IsBackgroundDimmed());
        }

        /// <summary>
        /// Check if the fade container removes user dim when suspending player for results
        /// </summary>
        [Test]
        public void TransitionTest()
        {
            performSetup();
            AddStep("Transition to Results", () => player.Push(new FadeAccesibleResults(new ScoreInfo { User = new User { Username = "osu!" } })));
            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is undimmed", () => songSelect.IsBackgroundUndimmed());
            AddAssert("Background retained from song select", () => songSelect.IsBackgroundCurrent());
        }

        /// <summary>
        /// Check if background gets undimmed when leaving the player for the previous screen
        /// </summary>
        [Test]
        public void TransitionOutTest()
        {
            performSetup();
            AddUntilStep(() =>
            {
                if (!songSelect.IsCurrentScreen())
                {
                    songSelect.MakeCurrent();
                    return false;
                }

                return true;
            }, "Wait for song select is current");
            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is undimmed", () => songSelect.IsBackgroundUndimmed());
        }

        private void performSetup()
        {
            createSongSelect();

            AddStep("Load new player to song select", () => songSelect.Push(player = new DimAccessiblePlayer { Ready = true }));
            AddUntilStep(() => player?.IsLoaded ?? false, "Wait for player to load");
        }

        private void createSongSelect()
        {
            AddStep("Create song select if required", () =>
            {
                if (songSelect == null)
                {
                    LoadComponentAsync(new DummySongSelect(), p =>
                    {
                        songSelect = p;
                        screen.Push(p);
                        songSelect.UpdateBindables();
                    });
                }
            });
            AddUntilStep(() => songSelect?.IsLoaded ?? false, "Wait for song select to load");
            AddUntilStep(() =>
            {
                if (!songSelect.IsCurrentScreen())
                {
                    songSelect.MakeCurrent();
                    return false;
                }

                return true;
            }, "Wait for song select is current");
        }

        private class DummySongSelect : OsuScreen
        {
            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground();
            public readonly Bindable<bool> DimEnabled = new Bindable<bool>();
            private readonly Bindable<double> dimLevel = new Bindable<double>();

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config)
            {
                config.BindWith(OsuSetting.DimLevel, dimLevel);
            }

            public void UpdateBindables()
            {
                DimEnabled.BindTo(((FadeAccessibleBackground)Background).EnableUserDim);
            }

            public bool IsBackgroundDimmed() => ((FadeAccessibleBackground)Background).CurrentColour == OsuColour.Gray(1 - (float)dimLevel.Value);

            public bool IsBackgroundUndimmed() => ((FadeAccessibleBackground)Background).CurrentColour == Color4.White;

            public bool IsBackgroundInvisible() => ((FadeAccessibleBackground)Background).CurrentAlpha == 0;

            public bool IsBackgroundVisible() => ((FadeAccessibleBackground)Background).CurrentAlpha == 1;

            /// <summary>
            /// Make sure every time a screen gets pushed, the background doesn't get replaced
            /// </summary>
            /// <returns>Whether or not the original background (The one created in DummySongSelect) is still the current background</returns>
            public bool IsBackgroundCurrent() => ((FadeAccessibleBackground)Background).IsCurrentScreen();
        }

        private class FadeAccesibleResults : SoloResults
        {
            public FadeAccesibleResults(ScoreInfo score)
                : base(score)
            {
            }

            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground();
        }

        private class DimAccessiblePlayer : Player
        {
            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground();

            // Whether or not the player should be allowed to load.
            public bool Ready;

            public Bindable<bool> StoryboardEnabled;
            public readonly Bindable<bool> ReplacesBackground = new Bindable<bool>();
            public readonly Bindable<bool> IsPaused = new Bindable<bool>();

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

        private class DimAccessiblePlayerLoader : PlayerLoader
        {
            public VisualSettings VisualSettingsPos => VisualSettings;
            public BackgroundScreen ScreenPos => Background;

            public DimAccessiblePlayerLoader(Player player)
                : base(() => player)
            {
            }

            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground();
        }

        private class FadeAccessibleBackground : BackgroundScreenBeatmap
        {
            protected override UserDimContainer CreateFadeContainer() => new TestUserDimContainer { RelativeSizeAxes = Axes.Both };

            public Color4 CurrentColour => ((TestUserDimContainer)FadeContainer).CurrentColour;
            public float CurrentAlpha => ((TestUserDimContainer)FadeContainer).CurrentAlpha;

            private class TestUserDimContainer : UserDimContainer
            {
                public Color4 CurrentColour => DimContainer.Colour;
                public float CurrentAlpha => DimContainer.Alpha;
            }
        }
    }
}
