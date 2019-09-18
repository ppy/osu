// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePlayerLoader : ManualInputManagerTestScene
    {
        private TestPlayerLoader loader;
        private TestPlayerLoaderContainer container;
        private TestPlayer player;

        [Resolved]
        private AudioManager audioManager { get; set; }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            ResetPlayer(false);
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
        });

        /// <summary>
        /// Sets the input manager child to a new test player loader container instance.
        /// </summary>
        /// <param name="interactive">If the test player should behave like the production one.</param>
        public void ResetPlayer(bool interactive)
        {
            player = new TestPlayer(interactive, interactive);
            loader = new TestPlayerLoader(() => player);
            container = new TestPlayerLoaderContainer(loader);
            InputManager.Child = container;
        }

        [Test]
        public void TestBlockLoadViaMouseMovement()
        {
            AddStep("load dummy beatmap", () => ResetPlayer(false));
            AddUntilStep("wait for current", () => loader.IsCurrentScreen());
            AddRepeatStep("move mouse", () => InputManager.MoveMouseTo(loader.VisualSettings.ScreenSpaceDrawQuad.TopLeft + (loader.VisualSettings.ScreenSpaceDrawQuad.BottomRight - loader.VisualSettings.ScreenSpaceDrawQuad.TopLeft) * RNG.NextSingle()), 20);
            AddAssert("loader still active", () => loader.IsCurrentScreen());
            AddUntilStep("loads after idle", () => !loader.IsCurrentScreen());
        }

        [Test]
        public void TestLoadContinuation()
        {
            Player player = null;
            SlowLoadPlayer slowPlayer = null;

            AddStep("load dummy beatmap", () => ResetPlayer(false));
            AddUntilStep("wait for current", () => loader.IsCurrentScreen());
            AddStep("mouse in centre", () => InputManager.MoveMouseTo(loader.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for player to be current", () => player.IsCurrentScreen());
            AddStep("load slow dummy beatmap", () =>
            {
                InputManager.Children = container = new TestPlayerLoaderContainer(loader = new TestPlayerLoader(() => slowPlayer = new SlowLoadPlayer(false, false)));
                Scheduler.AddDelayed(() => slowPlayer.AllowLoad.Set(), 5000);
            });

            AddUntilStep("wait for player to be current", () => slowPlayer.IsCurrentScreen());
        }

        [Test]
        public void TestModReinstantiation()
        {
            TestMod gameMod = null;
            TestMod playerMod1 = null;
            TestMod playerMod2 = null;

            AddStep("load player", () =>
            {
                Mods.Value = new[] { gameMod = new TestMod() };
                ResetPlayer(true);
            });

            AddUntilStep("wait for loader to become current", () => loader.IsCurrentScreen());
            AddStep("mouse in centre", () => InputManager.MoveMouseTo(loader.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for player to be current", () => player.IsCurrentScreen());
            AddStep("retrieve mods", () => playerMod1 = (TestMod)player.Mods.Value.Single());
            AddAssert("game mods not applied", () => gameMod.Applied == false);
            AddAssert("player mods applied", () => playerMod1.Applied);

            AddStep("restart player", () =>
            {
                var lastPlayer = player;
                player = null;
                lastPlayer.Restart();
            });

            AddUntilStep("wait for player to be current", () => player.IsCurrentScreen());
            AddStep("retrieve mods", () => playerMod2 = (TestMod)player.Mods.Value.Single());
            AddAssert("game mods not applied", () => gameMod.Applied == false);
            AddAssert("player has different mods", () => playerMod1 != playerMod2);
            AddAssert("player mods applied", () => playerMod2.Applied);
        }

        [Test]
        public void TestMutedNotification()
        {
            AddStep("set master volume to 0%", () => audioManager.Volume.Value = 0);
            AddStep("reset notification lock", () => PlayerLoader.ResetNotificationLock());
            //AddStep("reset notification overlay", () => notificationOverlay);
            AddStep("load player", () => ResetPlayer(false));
            AddUntilStep("wait for player", () => player.IsLoaded);

            AddAssert("check for notification", () => container.NotificationOverlay.UnreadCount.Value == 1);
            AddAssert("click notification", () =>
            {
                var scrollContainer = container.NotificationOverlay.Children.Last() as OsuScrollContainer;
                var flowContainer = scrollContainer.Children.First() as FillFlowContainer<NotificationSection>;
                return flowContainer.Children.First().First().Click();
            });
            AddAssert("check master volume", () => audioManager.Volume.IsDefault);

            AddStep("restart player", () =>
            {
                var lastPlayer = player;
                player = null;
                lastPlayer.Restart();
            });
        }

        private class TestPlayerLoaderContainer : Container
        {
            private TestPlayerLoader loader;

            [Cached]
            public NotificationOverlay NotificationOverlay { get; } = new NotificationOverlay
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            };

            [Cached]
            public VolumeOverlay VolumeOverlay { get; } = new VolumeOverlay
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
            };

            public TestPlayerLoaderContainer(TestPlayerLoader testPlayerLoader)
            {
                Children = new Drawable[]
                {
                    loader = testPlayerLoader,
                    NotificationOverlay,
                    VolumeOverlay
                };
            }
        }

        private class TestPlayerLoader : PlayerLoader
        {
            public new VisualSettings VisualSettings => base.VisualSettings;

            public TestPlayerLoader(Func<Player> createPlayer)
                : base(createPlayer)
            {
            }
        }

        private class TestMod : Mod, IApplicableToScoreProcessor
        {
            public override string Name => string.Empty;
            public override string Acronym => string.Empty;
            public override double ScoreMultiplier => 1;

            public bool Applied { get; private set; }

            public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
            {
                Applied = true;
            }

            public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;
        }

        private class TestPlayer : Visual.TestPlayer
        {
            public new Bindable<IReadOnlyList<Mod>> Mods => base.Mods;

            public TestPlayer(bool allowPause = true, bool showResults = true)
                : base(allowPause, showResults)
            {
            }
        }

        protected class SlowLoadPlayer : Visual.TestPlayer
        {
            public readonly ManualResetEventSlim AllowLoad = new ManualResetEventSlim(false);

            public SlowLoadPlayer(bool allowPause = true, bool showResults = true)
                : base(allowPause, showResults)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                if (!AllowLoad.Wait(TimeSpan.FromSeconds(10)))
                    throw new TimeoutException();
            }
        }
    }
}
