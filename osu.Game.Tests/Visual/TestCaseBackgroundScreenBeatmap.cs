// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
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
            typeof(Player)
        };

        private DummySongSelect songSelect;
        private DimAccessiblePlayerLoader playerLoader;
        private DimAccessiblePlayer player;

        [Cached]
        private BackgroundScreenStack backgroundStack;

        private void performSetup()
        {
            AddUntilStep(() =>
            {
                if (!songSelect.IsCurrentScreen())
                {
                    songSelect.MakeCurrent();
                    return false;
                }
                return true;
            }, "Wait for song select is current");
            AddStep("Load new player to song select", () => songSelect.Push(player = new DimAccessiblePlayer { Ready = true }));
            AddUntilStep(() => player?.IsLoaded ?? false, "Wait for player to load");
        }

        public TestCaseBackgroundScreenBeatmap()
        {
            ScreenStack screen;

            InputManager.Add(backgroundStack = new BackgroundScreenStack {RelativeSizeAxes = Axes.Both});
            InputManager.Add(screen = new ScreenStack { RelativeSizeAxes = Axes.Both });

            AddStep("Load Song Select", () =>
            {
                songSelect?.MakeCurrent();
                songSelect?.Exit();

                LoadComponentAsync(new DummySongSelect(), p =>
                {
                    songSelect = p;
                    screen.Push(p);
                    songSelect.UpdateBindables();
                });
            });

            AddUntilStep(() => songSelect?.IsLoaded ?? false, "Wait for song select to load");
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

            AddStep("Start player loader", () => songSelect.Push(playerLoader = new DimAccessiblePlayerLoader(player = new DimAccessiblePlayer())));
            AddUntilStep(() => playerLoader?.IsLoaded ?? false, "Wait for Player Loader to load");
            AddAssert("Background retained from song select", () => songSelect.AssertBackgroundCurrent());
            AddStep("Trigger background preview", () =>
            {
                InputManager.MoveMouseTo(playerLoader.ScreenPos);
                InputManager.MoveMouseTo(playerLoader.VisualSettingsPos);
            });

            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is dimmed", () => songSelect.AssertDimmed());

            AddStep("Allow beatmap to load", () =>
            {
                player.Ready = true;
                InputManager.MoveMouseTo(playerLoader.ScreenPos);
            });

            // In the case of a user triggering the dim preview the instant player gets loaded, then moving the cursor off of the visual settings:
            // The OnHover of PlayerLoader will trigger, which could potentially trigger an undim unless checked for in PlayerLoader.
            // We need to check that in this scenario, the dim is still properly applied after entering player.
            AddUntilStep(() => player?.IsLoaded ?? false, "Wait for player to load");
            AddAssert("Background retained from song select", () => songSelect.AssertBackgroundCurrent());
            AddStep("Trigger background preview when loaded", () =>
            {
                InputManager.MoveMouseTo(playerLoader.VisualSettingsPos);
                InputManager.MoveMouseTo(playerLoader.ScreenPos);
            });
            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is dimmed", () => songSelect.AssertDimmed());

            // Make sure the background is fully invisible (not dimmed) when the background should be disabled by the storyboard.
            AddStep("Enable storyboard", () =>
            {
                player.ReplacesBackground.Value = true;
                player.StoryboardEnabled.Value = true;
            });
            AddWaitStep(5, "Wait for dim");
            AddAssert("Background is invisible", () => songSelect.AssertInvisible());
            AddStep("Disable storyboard", () => player.ReplacesBackground.Value = false);
            AddWaitStep(5, "Wait for dim");
            AddAssert("Background is visible", () => songSelect.AssertVisible());
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
            AddAssert("Screen is undimmed", () => songSelect.AssertUndimmed());
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
            AddAssert("Screen is dimmed", () => songSelect.AssertDimmed());
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
                if (!player.IsPaused)
                    player.Exit();
            });
            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is dimmed", () => songSelect.AssertDimmed());
        }

        /// <summary>
        /// Check if the fade container removes user dim when suspending player for results
        /// </summary>
        [Test]
        public void TransitionTest()
        {
            performSetup();
            AddStep("Transition to Results", () => player.Push(new FadeAccesibleResults(new ScoreInfo { User = new User { Username = "osu!" }})));
            AddWaitStep(5, "Wait for dim");
            AddAssert("Screen is undimmed", () => songSelect.AssertUndimmed());
            AddAssert("Background retained from song select", () => songSelect.AssertBackgroundCurrent());
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
            AddAssert("Screen is undimmed", () => songSelect.AssertUndimmed());
        }

        private class DummySongSelect : OsuScreen
        {
            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground();
            public readonly Bindable<bool> DimEnabled = new Bindable<bool>();

            public void UpdateBindables()
            {
                DimEnabled.BindTo(((FadeAccessibleBackground)Background).EnableUserDim);
            }

            public bool AssertDimmed()
            {
                return ((FadeAccessibleBackground)Background).AssertDimmed();
            }

            public bool AssertUndimmed()
            {
                return ((FadeAccessibleBackground)Background).AssertUndimmed();
            }

            public bool AssertInvisible()
            {
                return ((FadeAccessibleBackground)Background).AssertInvisible();
            }

            public bool AssertVisible()
            {
                return ((FadeAccessibleBackground)Background).AssertVisible();
            }

            /// <summary>
            /// Make sure every time a screen gets pushed, the background doesn't get replaced
            /// </summary>
            /// <returns>Whether or not the original background is still the current background</returns>
            public bool AssertBackgroundCurrent()
            {
                return ((FadeAccessibleBackground)Background).IsCurrentScreen();
            }
        }

        private class FadeAccesibleResults : SoloResults
        {
            public FadeAccesibleResults(ScoreInfo score) : base(score)
            {
            }

            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground();
        }

        private class DimAccessiblePlayer : Player
        {
            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground();

            public bool Ready;

            public Bindable<bool> StoryboardEnabled;
            public readonly Bindable<bool> ReplacesBackground = new Bindable<bool>();

            public bool IsPaused => RulesetContainer.IsPaused;

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config)
            {
                while (!Ready)
                    Thread.Sleep(1);
                StoryboardEnabled = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
                ReplacesBackground.BindTo(Background.StoryboardReplacesBackground);
            }
        }

        private class DimAccessiblePlayerLoader : PlayerLoader
        {
            public VisualSettings VisualSettingsPos => VisualSettings;
            public BackgroundScreen ScreenPos => Background;

            [Resolved]
            private BackgroundScreenStack stack { get; set; }
            public DimAccessiblePlayerLoader(Player player) : base(() => player)
            {
            }

            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground();
        }

        private class FadeAccessibleBackground : BackgroundScreenBeatmap
        {
            public bool AssertDimmed()
            {
                return FadeContainer.Colour == OsuColour.Gray(1 - (float)DimLevel);
            }

            public bool AssertUndimmed()
            {
                return FadeContainer.Colour == Color4.White;
            }

            public bool AssertInvisible()
            {
                return FadeContainer.Alpha == 0;
            }

            public bool AssertVisible()
            {
                return FadeContainer.Alpha == 1;
            }
        }
    }
}
