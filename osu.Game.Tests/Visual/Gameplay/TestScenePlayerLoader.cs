// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Utils;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestScenePlayerLoader : ScreenTestScene
    {
        private TestPlayerLoader loader;
        private TestPlayer player;

        private bool epilepsyWarning;

        [Resolved]
        private AudioManager audioManager { get; set; }

        [Resolved]
        private SessionStatics sessionStatics { get; set; }

        [Resolved]
        private OsuConfigManager config { get; set; }

        [Cached(typeof(INotificationOverlay))]
        private readonly NotificationOverlay notificationOverlay;

        [Cached]
        private readonly VolumeOverlay volumeOverlay;

        [Cached(typeof(BatteryInfo))]
        private readonly LocalBatteryInfo batteryInfo = new LocalBatteryInfo();

        private readonly ChangelogOverlay changelogOverlay;

        private double savedTrackVolume;
        private double savedMasterVolume;
        private bool savedMutedState;

        public TestScenePlayerLoader()
        {
            AddRange(new Drawable[]
            {
                notificationOverlay = new NotificationOverlay
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                },
                volumeOverlay = new VolumeOverlay
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                },
                changelogOverlay = new ChangelogOverlay()
            });
        }

        [SetUp]
        public void Setup() => Schedule(() => player = null);

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("read all notifications", () =>
            {
                notificationOverlay.Show();
                notificationOverlay.Hide();
            });

            AddUntilStep("wait for no notifications", () => notificationOverlay.UnreadCount.Value, () => Is.EqualTo(0));
        }

        /// <summary>
        /// Sets the input manager child to a new test player loader container instance.
        /// </summary>
        /// <param name="interactive">If the test player should behave like the production one.</param>
        /// <param name="beforeLoadAction">An action to run before player load but after bindable leases are returned.</param>
        private void resetPlayer(bool interactive, Action beforeLoadAction = null)
        {
            beforeLoadAction?.Invoke();

            prepareBeatmap();

            LoadScreen(loader = new TestPlayerLoader(() => player = new TestPlayer(interactive, interactive)));
        }

        private void prepareBeatmap()
        {
            var workingBeatmap = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

            // Add intro time to test quick retry skipping (TestQuickRetry).
            workingBeatmap.BeatmapInfo.AudioLeadIn = 60000;

            // Turn on epilepsy warning to test warning display (TestEpilepsyWarning).
            workingBeatmap.BeatmapInfo.EpilepsyWarning = epilepsyWarning;

            Beatmap.Value = workingBeatmap;

            foreach (var mod in SelectedMods.Value.OfType<IApplicableToTrack>())
                mod.ApplyToTrack(Beatmap.Value.Track);
        }

        [Test]
        public void TestEarlyExitBeforePlayerConstruction()
        {
            AddStep("load dummy beatmap", () => resetPlayer(false, () => SelectedMods.Value = new[] { new OsuModNightcore() }));
            AddUntilStep("wait for current", () => loader.IsCurrentScreen());
            AddStep("exit loader", () => loader.Exit());
            AddUntilStep("wait for not current", () => !loader.IsCurrentScreen());
            AddAssert("player did not load", () => player == null);
            AddUntilStep("player disposed", () => loader.DisposalTask == null);
            AddAssert("mod rate still applied", () => Beatmap.Value.Track.Rate != 1);
        }

        /// <summary>
        /// When <see cref="PlayerLoader"/> exits early, it has to wait for the player load task
        /// to complete before running disposal on player. This previously caused an issue where mod
        /// speed adjustments were undone too late, causing cross-screen pollution.
        /// </summary>
        [Test]
        public void TestEarlyExitAfterPlayerConstruction()
        {
            AddStep("load dummy beatmap", () => resetPlayer(false, () => SelectedMods.Value = new[] { new OsuModNightcore() }));
            AddUntilStep("wait for current", () => loader.IsCurrentScreen());
            AddAssert("mod rate applied", () => Beatmap.Value.Track.Rate != 1);
            AddUntilStep("wait for non-null player", () => player != null);
            AddStep("exit loader", () => loader.Exit());
            AddUntilStep("wait for not current", () => !loader.IsCurrentScreen());
            AddAssert("player did not load", () => !player.IsLoaded);
            AddUntilStep("player disposed", () => loader.DisposalTask?.IsCompleted == true);
            AddAssert("mod rate still applied", () => Beatmap.Value.Track.Rate != 1);
        }

        [Test]
        public void TestBlockLoadViaMouseMovement()
        {
            AddStep("load dummy beatmap", () => resetPlayer(false));
            AddUntilStep("wait for current", () => loader.IsCurrentScreen());

            AddUntilStep("wait for load ready", () =>
            {
                moveMouse();
                return player?.LoadState == LoadState.Ready;
            });

            AddRepeatStep("move mouse", moveMouse, 20);

            AddAssert("loader still active", () => loader.IsCurrentScreen());
            AddUntilStep("loads after idle", () => !loader.IsCurrentScreen());

            void moveMouse()
            {
                notificationOverlay.State.Value = Visibility.Hidden;

                InputManager.MoveMouseTo(
                    loader.VisualSettings.ScreenSpaceDrawQuad.TopLeft
                    + (loader.VisualSettings.ScreenSpaceDrawQuad.BottomRight - loader.VisualSettings.ScreenSpaceDrawQuad.TopLeft)
                    * RNG.NextSingle());
            }
        }

        [Test]
        public void TestBlockLoadViaFocus()
        {
            AddStep("load dummy beatmap", () => resetPlayer(false));
            AddUntilStep("wait for current", () => loader.IsCurrentScreen());

            AddStep("show focused overlay", () => changelogOverlay.Show());
            AddUntilStep("overlay visible", () => changelogOverlay.IsPresent);

            AddUntilStep("wait for load ready", () => player?.LoadState == LoadState.Ready);
            AddRepeatStep("twiddle thumbs", () => { }, 20);

            AddAssert("loader still active", () => loader.IsCurrentScreen());

            AddStep("hide overlay", () => changelogOverlay.Hide());
            AddUntilStep("loads after idle", () => !loader.IsCurrentScreen());
        }

        [Test]
        public void TestLoadContinuation()
        {
            SlowLoadPlayer slowPlayer = null;

            AddStep("load slow dummy beatmap", () =>
            {
                prepareBeatmap();
                slowPlayer = new SlowLoadPlayer(false, false);
                LoadScreen(loader = new TestPlayerLoader(() => slowPlayer));
            });

            AddStep("schedule slow load", () => Scheduler.AddDelayed(() => slowPlayer.AllowLoad.Set(), 5000));

            AddUntilStep("wait for player to be current", () => slowPlayer.IsCurrentScreen());
        }

        [Test]
        public void TestModReinstantiation()
        {
            TestMod gameMod = null;
            TestMod playerMod1 = null;
            TestMod playerMod2 = null;

            AddStep("load player", () => { resetPlayer(true, () => SelectedMods.Value = new[] { gameMod = new TestMod() }); });

            AddUntilStep("wait for loader to become current", () => loader.IsCurrentScreen());
            AddStep("mouse in centre", () => InputManager.MoveMouseTo(loader.ScreenSpaceDrawQuad.Centre));
            AddUntilStep("wait for player to be current", () => player.IsCurrentScreen());
            AddStep("retrieve mods", () => playerMod1 = (TestMod)player.GameplayState.Mods.Single());
            AddAssert("game mods not applied", () => gameMod.Applied == false);
            AddAssert("player mods applied", () => playerMod1.Applied);

            AddStep("restart player", () =>
            {
                var lastPlayer = player;
                player = null;
                lastPlayer.Restart();
            });

            AddUntilStep("wait for player to be current", () => player.IsCurrentScreen());
            AddStep("retrieve mods", () => playerMod2 = (TestMod)player.GameplayState.Mods.Single());
            AddAssert("game mods not applied", () => gameMod.Applied == false);
            AddAssert("player has different mods", () => playerMod1 != playerMod2);
            AddAssert("player mods applied", () => playerMod2.Applied);
        }

        [Test]
        public void TestModDisplayChanges()
        {
            var testMod = new TestMod();

            AddStep("load player", () => resetPlayer(true));

            AddUntilStep("wait for loader to become current", () => loader.IsCurrentScreen());
            AddStep("set test mod in loader", () => loader.Mods.Value = new[] { testMod });
            AddAssert("test mod is displayed", () => (TestMod)loader.DisplayedMods.Single() == testMod);
        }

        [Test]
        public void TestMutedNotificationHighMasterVolume()
        {
            addVolumeSteps("master and music volumes", () =>
            {
                audioManager.Volume.Value = 0.6;
                audioManager.VolumeTrack.Value = 0.15;
            }, () => Precision.AlmostEquals(audioManager.Volume.Value, 0.6) && Precision.AlmostEquals(audioManager.VolumeTrack.Value, 0.83));
        }

        [Test]
        public void TestMutedNotificationLowMasterVolume()
        {
            addVolumeSteps("master and music volumes", () =>
            {
                audioManager.Volume.Value = 0.01;
                audioManager.VolumeTrack.Value = 0.15;
            }, () => Precision.AlmostEquals(audioManager.Volume.Value, 0.5) && Precision.AlmostEquals(audioManager.VolumeTrack.Value, 1));
        }

        [Test]
        public void TestMutedNotificationMuteButton()
        {
            addVolumeSteps("mute button", () =>
            {
                // Importantly, in the case the volume is muted but the user has a volume level set, it should be retained.
                audioManager.Volume.Value = 0.5f;
                audioManager.VolumeTrack.Value = 0.5f;
                volumeOverlay.IsMuted.Value = true;
            }, () => !volumeOverlay.IsMuted.Value && audioManager.Volume.Value == 0.5f && audioManager.VolumeTrack.Value == 0.5f);
        }

        /// <remarks>
        /// Created for avoiding copy pasting code for the same steps.
        /// </remarks>
        /// <param name="volumeName">What part of the volume system is checked</param>
        /// <param name="beforeLoad">The action to be invoked to set the volume before loading</param>
        /// <param name="assert">The function to be invoked and checked</param>
        private void addVolumeSteps(string volumeName, Action beforeLoad, Func<bool> assert)
        {
            AddStep("reset notification lock", () => sessionStatics.GetBindable<bool>(Static.MutedAudioNotificationShownOnce).Value = false);

            AddStep("load player", () => resetPlayer(false, beforeLoad));
            AddUntilStep("wait for player", () => player?.LoadState == LoadState.Ready);

            saveVolumes();

            AddAssert("check for notification", () => notificationOverlay.UnreadCount.Value, () => Is.EqualTo(1));

            clickNotification();

            AddAssert("check " + volumeName, assert);

            restoreVolumes();

            AddUntilStep("wait for player load", () => player.IsLoaded);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestEpilepsyWarning(bool warning)
        {
            saveVolumes();
            setFullVolume();

            AddStep("enable storyboards", () => config.SetValue(OsuSetting.ShowStoryboard, true));
            AddStep("change epilepsy warning", () => epilepsyWarning = warning);
            AddStep("load dummy beatmap", () => resetPlayer(false));

            AddUntilStep("wait for current", () => loader.IsCurrentScreen());

            AddAssert($"epilepsy warning {(warning ? "present" : "absent")}", () => (getWarning() != null) == warning);

            if (warning)
            {
                AddUntilStep("sound volume decreased", () => Beatmap.Value.Track.AggregateVolume.Value == 0.25);
                AddUntilStep("sound volume restored", () => Beatmap.Value.Track.AggregateVolume.Value == 1);
            }

            restoreVolumes();
        }

        [Test]
        public void TestEpilepsyWarningWithDisabledStoryboard()
        {
            saveVolumes();
            setFullVolume();

            AddStep("disable storyboards", () => config.SetValue(OsuSetting.ShowStoryboard, false));
            AddStep("change epilepsy warning", () => epilepsyWarning = true);
            AddStep("load dummy beatmap", () => resetPlayer(false));

            AddUntilStep("wait for current", () => loader.IsCurrentScreen());

            AddUntilStep("epilepsy warning absent", () => getWarning() == null);

            restoreVolumes();
        }

        [Test]
        public void TestEpilepsyWarningEarlyExit()
        {
            saveVolumes();
            setFullVolume();

            AddStep("enable storyboards", () => config.SetValue(OsuSetting.ShowStoryboard, true));
            AddStep("set epilepsy warning", () => epilepsyWarning = true);
            AddStep("load dummy beatmap", () => resetPlayer(false));

            AddUntilStep("wait for current", () => loader.IsCurrentScreen());

            AddUntilStep("wait for epilepsy warning", () => getWarning().Alpha > 0);
            AddUntilStep("warning is shown", () => getWarning().State.Value == Visibility.Visible);

            AddStep("exit early", () => loader.Exit());

            AddUntilStep("warning is hidden", () => getWarning().State.Value == Visibility.Hidden);
            AddUntilStep("sound volume restored", () => Beatmap.Value.Track.AggregateVolume.Value == 1);

            restoreVolumes();
        }

        [TestCase(true, 1.0, false)] // on battery, above cutoff --> no warning
        [TestCase(false, 0.1, false)] // not on battery, below cutoff --> no warning
        [TestCase(true, 0.25, true)] // on battery, at cutoff --> warning
        [TestCase(true, null, false)] // on battery, level unknown --> no warning
        public void TestLowBatteryNotification(bool onBattery, double? chargeLevel, bool shouldWarn)
        {
            AddStep("reset notification lock", () => sessionStatics.GetBindable<bool>(Static.LowBatteryNotificationShownOnce).Value = false);

            // set charge status and level
            AddStep("load player", () => resetPlayer(false, () =>
            {
                batteryInfo.SetOnBattery(onBattery);
                batteryInfo.SetChargeLevel(chargeLevel);
            }));
            AddUntilStep("wait for player", () => player?.LoadState == LoadState.Ready);

            if (shouldWarn)
                clickNotification();
            else
                AddAssert("notification not triggered", () => notificationOverlay.UnreadCount.Value == 0);

            AddUntilStep("wait for player load", () => player.IsLoaded);
        }

        private void restoreVolumes()
        {
            AddStep("restore previous volumes", () =>
            {
                audioManager.VolumeTrack.Value = savedTrackVolume;
                audioManager.Volume.Value = savedMasterVolume;
                volumeOverlay.IsMuted.Value = savedMutedState;
            });
        }

        private void setFullVolume()
        {
            AddStep("set volumes to 100%", () =>
            {
                audioManager.VolumeTrack.Value = 1;
                audioManager.Volume.Value = 1;
                volumeOverlay.IsMuted.Value = false;
            });
        }

        private void saveVolumes()
        {
            AddStep("save previous volumes", () =>
            {
                savedTrackVolume = audioManager.VolumeTrack.Value;
                savedMasterVolume = audioManager.Volume.Value;
                savedMutedState = volumeOverlay.IsMuted.Value;
            });
        }

        [Test]
        public void TestQuickRetry()
        {
            TestPlayer getCurrentPlayer() => loader.CurrentPlayer as TestPlayer;
            bool checkSkipButtonVisible() => player.ChildrenOfType<SkipOverlay>().FirstOrDefault()?.IsButtonVisible == true;

            TestPlayer previousPlayer = null;

            AddStep("load dummy beatmap", () => resetPlayer(false));

            AddUntilStep("wait for current", () => getCurrentPlayer()?.IsCurrentScreen() == true);
            AddStep("store previous player", () => previousPlayer = getCurrentPlayer());

            AddStep("Restart map normally", () => getCurrentPlayer().Restart());
            AddUntilStep("wait for load", () => getCurrentPlayer()?.LoadedBeatmapSuccessfully == true);

            AddUntilStep("restart completed", () => getCurrentPlayer() != null && getCurrentPlayer() != previousPlayer);
            AddStep("store previous player", () => previousPlayer = getCurrentPlayer());

            AddUntilStep("skip button visible", checkSkipButtonVisible);

            AddStep("press quick retry key", () => InputManager.PressKey(Key.Tilde));
            AddUntilStep("restart completed", () => getCurrentPlayer() != null && getCurrentPlayer() != previousPlayer);
            AddStep("release quick retry key", () => InputManager.ReleaseKey(Key.Tilde));

            AddUntilStep("wait for player", () => getCurrentPlayer()?.LoadState == LoadState.Ready);

            AddUntilStep("time reached zero", () => getCurrentPlayer()?.GameplayClockContainer.CurrentTime > 0);
            AddUntilStep("skip button not visible", () => !checkSkipButtonVisible());
        }

        private void clickNotification()
        {
            Notification notification = null;

            AddUntilStep("wait for notification", () => (notification = notificationOverlay.ChildrenOfType<Notification>().FirstOrDefault()) != null);
            AddStep("open notification overlay", () => notificationOverlay.Show());
            AddStep("click notification", () => notification.TriggerClick());
        }

        private EpilepsyWarning getWarning() => loader.ChildrenOfType<EpilepsyWarning>().SingleOrDefault(w => w.IsAlive);

        private partial class TestPlayerLoader : PlayerLoader
        {
            public new VisualSettings VisualSettings => base.VisualSettings;

            public new Task DisposalTask => base.DisposalTask;

            public IReadOnlyList<Mod> DisplayedMods => MetadataInfo.Mods.Value;

            public TestPlayerLoader(Func<Player> createPlayer)
                : base(createPlayer)
            {
            }
        }

        private class TestMod : OsuModDoubleTime, IApplicableToScoreProcessor
        {
            public bool Applied { get; private set; }

            public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
            {
                Applied = true;
            }

            public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;
        }

        protected partial class SlowLoadPlayer : TestPlayer
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

        /// <summary>
        /// Mutable dummy BatteryInfo class for <see cref="TestScenePlayerLoader.TestLowBatteryNotification"/>
        /// </summary>
        /// <inheritdoc/>
        private class LocalBatteryInfo : BatteryInfo
        {
            private bool onBattery;
            private double? chargeLevel;

            public override bool OnBattery => onBattery;

            public override double? ChargeLevel => chargeLevel;

            public void SetOnBattery(bool value)
            {
                onBattery = value;
            }

            public void SetChargeLevel(double? value)
            {
                chargeLevel = value;
            }
        }
    }
}
