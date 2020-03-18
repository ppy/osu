// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.PlayerSettings;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePlayerLoader : ManualInputManagerTestScene
    {
        private TestPlayerLoader loader;
        private TestPlayerLoaderContainer container;
        private TestPlayer player;

        [Resolved]
        private AudioManager audioManager { get; set; }

        [Resolved]
        private SessionStatics sessionStatics { get; set; }

        /// <summary>
        /// Sets the input manager child to a new test player loader container instance.
        /// </summary>
        /// <param name="interactive">If the test player should behave like the production one.</param>
        /// <param name="beforeLoadAction">An action to run before player load but after bindable leases are returned.</param>
        /// <param name="afterLoadAction">An action to run after container load.</param>
        public void ResetPlayer(bool interactive, Action beforeLoadAction = null, Action afterLoadAction = null)
        {
            audioManager.Volume.SetDefault();

            InputManager.Clear();

            beforeLoadAction?.Invoke();
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

            foreach (var mod in SelectedMods.Value.OfType<IApplicableToTrack>())
                mod.ApplyToTrack(Beatmap.Value.Track);

            InputManager.Child = container = new TestPlayerLoaderContainer(
                loader = new TestPlayerLoader(() =>
                {
                    afterLoadAction?.Invoke();
                    return player = new TestPlayer(interactive, interactive);
                }));
        }

        /// <summary>
        /// When <see cref="PlayerLoader"/> exits early, it has to wait for the player load task
        /// to complete before running disposal on player. This previously caused an issue where mod
        /// speed adjustments were undone too late, causing cross-screen pollution.
        /// </summary>
        [Test]
        public void TestEarlyExit()
        {
            AddStep("load dummy beatmap", () => ResetPlayer(false, () => SelectedMods.Value = new[] { new OsuModNightcore() }));
            AddUntilStep("wait for current", () => loader.IsCurrentScreen());
            AddAssert("mod rate applied", () => Beatmap.Value.Track.Rate != 1);
            AddStep("exit loader", () => loader.Exit());
            AddUntilStep("wait for not current", () => !loader.IsCurrentScreen());
            AddAssert("player did not load", () => !player.IsLoaded);
            AddUntilStep("player disposed", () => loader.DisposalTask?.IsCompleted == true);
            AddAssert("mod rate still applied", () => Beatmap.Value.Track.Rate != 1);
        }

        [Test]
        public void TestBlockLoadViaMouseMovement()
        {
            AddStep("load dummy beatmap", () => ResetPlayer(false));
            AddUntilStep("wait for current", () => loader.IsCurrentScreen());

            AddUntilStep("wait for load ready", () =>
            {
                moveMouse();
                return player.LoadState == LoadState.Ready;
            });
            AddRepeatStep("move mouse", moveMouse, 20);

            AddAssert("loader still active", () => loader.IsCurrentScreen());
            AddUntilStep("loads after idle", () => !loader.IsCurrentScreen());

            void moveMouse()
            {
                InputManager.MoveMouseTo(
                    loader.VisualSettings.ScreenSpaceDrawQuad.TopLeft
                    + (loader.VisualSettings.ScreenSpaceDrawQuad.BottomRight - loader.VisualSettings.ScreenSpaceDrawQuad.TopLeft)
                    * RNG.NextSingle());
            }
        }

        [Test]
        public void TestBlockLoadViaFocus()
        {
            OsuFocusedOverlayContainer overlay = null;

            AddStep("load dummy beatmap", () => ResetPlayer(false));
            AddUntilStep("wait for current", () => loader.IsCurrentScreen());

            AddStep("show focused overlay", () => { container.Add(overlay = new ChangelogOverlay { State = { Value = Visibility.Visible } }); });
            AddUntilStep("overlay visible", () => overlay.IsPresent);

            AddUntilStep("wait for load ready", () => player.LoadState == LoadState.Ready);
            AddRepeatStep("twiddle thumbs", () => { }, 20);

            AddAssert("loader still active", () => loader.IsCurrentScreen());

            AddStep("hide overlay", () => overlay.Hide());
            AddUntilStep("loads after idle", () => !loader.IsCurrentScreen());
        }

        [Test]
        public void TestLoadContinuation()
        {
            SlowLoadPlayer slowPlayer = null;

            AddStep("load dummy beatmap", () => ResetPlayer(false));
            AddUntilStep("wait for current", () => loader.IsCurrentScreen());
            AddStep("mouse in centre", () => InputManager.MoveMouseTo(loader.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for player to be current", () => player.IsCurrentScreen());
            AddStep("load slow dummy beatmap", () =>
            {
                InputManager.Child = container = new TestPlayerLoaderContainer(
                    loader = new TestPlayerLoader(() => slowPlayer = new SlowLoadPlayer(false, false)));

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

            AddStep("load player", () => { ResetPlayer(true, () => SelectedMods.Value = new[] { gameMod = new TestMod() }); });

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
        public void TestModDisplayChanges()
        {
            var testMod = new TestMod();

            AddStep("load player", () => ResetPlayer(true));

            AddUntilStep("wait for loader to become current", () => loader.IsCurrentScreen());
            AddStep("set test mod in loader", () => loader.Mods.Value = new[] { testMod });
            AddAssert("test mod is displayed", () => (TestMod)loader.DisplayedMods.Single() == testMod);
        }

        [Test]
        public void TestMutedNotificationMasterVolume()
        {
            addVolumeSteps("master volume", () => audioManager.Volume.Value = 0, null, () => audioManager.Volume.IsDefault);
        }

        [Test]
        public void TestMutedNotificationTrackVolume()
        {
            addVolumeSteps("music volume", () => audioManager.VolumeTrack.Value = 0, null, () => audioManager.VolumeTrack.IsDefault);
        }

        [Test]
        public void TestMutedNotificationMuteButton()
        {
            addVolumeSteps("mute button", null, () => container.VolumeOverlay.IsMuted.Value = true, () => !container.VolumeOverlay.IsMuted.Value);
        }

        /// <remarks>
        /// Created for avoiding copy pasting code for the same steps.
        /// </remarks>
        /// <param name="volumeName">What part of the volume system is checked</param>
        /// <param name="beforeLoad">The action to be invoked to set the volume before loading</param>
        /// <param name="afterLoad">The action to be invoked to set the volume after loading</param>
        /// <param name="assert">The function to be invoked and checked</param>
        private void addVolumeSteps(string volumeName, Action beforeLoad, Action afterLoad, Func<bool> assert)
        {
            AddStep("reset notification lock", () => sessionStatics.GetBindable<bool>(Static.MutedAudioNotificationShownOnce).Value = false);

            AddStep("load player", () => ResetPlayer(false, beforeLoad, afterLoad));
            AddUntilStep("wait for player", () => player.LoadState == LoadState.Ready);

            AddAssert("check for notification", () => container.NotificationOverlay.UnreadCount.Value == 1);
            AddStep("click notification", () =>
            {
                var scrollContainer = (OsuScrollContainer)container.NotificationOverlay.Children.Last();
                var flowContainer = scrollContainer.Children.OfType<FillFlowContainer<NotificationSection>>().First();
                var notification = flowContainer.First();

                InputManager.MoveMouseTo(notification);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("check " + volumeName, assert);

            AddUntilStep("wait for player load", () => player.IsLoaded);
        }

        private class TestPlayerLoaderContainer : Container
        {
            [Cached]
            public readonly NotificationOverlay NotificationOverlay;

            [Cached]
            public readonly VolumeOverlay VolumeOverlay;

            public TestPlayerLoaderContainer(IScreen screen)
            {
                RelativeSizeAxes = Axes.Both;

                OsuScreenStack stack;

                InternalChildren = new Drawable[]
                {
                    stack = new OsuScreenStack
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    NotificationOverlay = new NotificationOverlay
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                    },
                    VolumeOverlay = new VolumeOverlay
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                    }
                };

                stack.Push(screen);
            }
        }

        private class TestPlayerLoader : PlayerLoader
        {
            public new VisualSettings VisualSettings => base.VisualSettings;

            public new Task DisposalTask => base.DisposalTask;

            public IReadOnlyList<Mod> DisplayedMods => MetadataInfo.Mods.Value;

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

        protected class SlowLoadPlayer : TestPlayer
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
