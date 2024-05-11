// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public partial class MultiplayerReadyButton : ReadyButton
    {
        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        [CanBeNull]
        private MultiplayerRoom room => multiplayerClient.Room;

        private Sample countdownTickSample;
        private Sample countdownWarnSample;
        private Sample countdownWarnFinalSample;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            countdownTickSample = audio.Samples.Get(@"Multiplayer/countdown-tick");
            countdownWarnSample = audio.Samples.Get(@"Multiplayer/countdown-warn");
            countdownWarnFinalSample = audio.Samples.Get(@"Multiplayer/countdown-warn-final");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            multiplayerClient.RoomUpdated += onRoomUpdated;
            onRoomUpdated();
        }

        private MultiplayerCountdown countdown;
        private double countdownChangeTime;
        private ScheduledDelegate countdownUpdateDelegate;

        private void onRoomUpdated() => Scheduler.AddOnce(() =>
        {
            MultiplayerCountdown newCountdown = room?.ActiveCountdowns.SingleOrDefault(c => c is MatchStartCountdown);

            if (newCountdown != countdown)
            {
                countdown = newCountdown;
                countdownChangeTime = Time.Current;
            }

            scheduleNextCountdownUpdate();

            updateButtonText();
            updateButtonColour();
        });

        private void scheduleNextCountdownUpdate()
        {
            countdownUpdateDelegate?.Cancel();

            if (countdown != null)
            {
                // The remaining time on a countdown may be at a fractional portion between two seconds.
                // We want to align certain audio/visual cues to the point at which integer seconds change.
                // To do so, we schedule to the next whole second. Note that scheduler invocation isn't
                // guaranteed to be accurate, so this may still occur slightly late, but even in such a case
                // the next invocation will be roughly correct.
                double timeToNextSecond = countdownTimeRemaining.TotalMilliseconds % 1000;

                countdownUpdateDelegate = Scheduler.AddDelayed(onCountdownTick, timeToNextSecond);
            }
            else
            {
                countdownUpdateDelegate?.Cancel();
                countdownUpdateDelegate = null;
            }

            void onCountdownTick()
            {
                updateButtonText();

                int secondsRemaining = (int)countdownTimeRemaining.TotalSeconds;

                playTickSound(secondsRemaining);

                if (secondsRemaining > 0)
                    scheduleNextCountdownUpdate();
            }
        }

        private void playTickSound(int secondsRemaining)
        {
            if (secondsRemaining < 10) countdownTickSample?.Play();

            if (secondsRemaining <= 3)
            {
                if (secondsRemaining > 0)
                    countdownWarnSample?.Play();
                else
                    countdownWarnFinalSample?.Play();
            }
        }

        private void updateButtonText()
        {
            if (room == null)
            {
                Text = "Ready";
                return;
            }

            var localUser = multiplayerClient.LocalUser;

            int countReady = room.Users.Count(u => u.State == MultiplayerUserState.Ready);
            int countTotal = room.Users.Count(u => u.State != MultiplayerUserState.Spectating);
            string countText = $"({countReady} / {countTotal} ready)";

            if (countdown != null)
            {
                string countdownText = $"Starting in {countdownTimeRemaining:mm\\:ss}";

                switch (localUser?.State)
                {
                    default:
                        Text = $"Ready ({countdownText.ToLowerInvariant()})";
                        break;

                    case MultiplayerUserState.Spectating:
                    case MultiplayerUserState.Ready:
                        Text = $"{countdownText} {countText}";
                        break;
                }
            }
            else
            {
                switch (localUser?.State)
                {
                    case MultiplayerUserState.Spectating:
                    case MultiplayerUserState.Ready:
                        Text = multiplayerClient.IsHost
                            ? $"Start match {countText}"
                            : $"Waiting for host... {countText}";
                        break;

                    default:
                        // Show the abort button for the host as long as gameplay is in progress.
                        if (multiplayerClient.IsHost && room.State != MultiplayerRoomState.Open)
                            Text = "Abort the match";
                        else
                            Text = "Ready";
                        break;
                }
            }
        }

        private TimeSpan countdownTimeRemaining
        {
            get
            {
                double timeElapsed = Time.Current - countdownChangeTime;
                TimeSpan remaining;

                if (timeElapsed > countdown.TimeRemaining.TotalMilliseconds)
                    remaining = TimeSpan.Zero;
                else
                    remaining = countdown.TimeRemaining - TimeSpan.FromMilliseconds(timeElapsed);

                return remaining;
            }
        }

        private void updateButtonColour()
        {
            if (room == null)
            {
                setGreen();
                return;
            }

            var localUser = multiplayerClient.LocalUser;

            switch (localUser?.State)
            {
                default:
                    // Show the abort button for the host as long as gameplay is in progress.
                    if (multiplayerClient.IsHost && room.State != MultiplayerRoomState.Open)
                        setRed();
                    else
                        setGreen();
                    break;

                case MultiplayerUserState.Spectating:
                case MultiplayerUserState.Ready:
                    if (multiplayerClient.IsHost && !room.ActiveCountdowns.Any(c => c is MatchStartCountdown))
                        setGreen();
                    else
                        setYellow();

                    break;
            }

            void setYellow() => BackgroundColour = colours.YellowDark;

            void setGreen() => BackgroundColour = colours.Green;

            void setRed() => BackgroundColour = colours.Red;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (multiplayerClient != null)
                multiplayerClient.RoomUpdated -= onRoomUpdated;
        }

        public override LocalisableString TooltipText
        {
            get
            {
                if (room?.ActiveCountdowns.Any(c => c is MatchStartCountdown) == true
                    && multiplayerClient.IsHost
                    && multiplayerClient.LocalUser?.State == MultiplayerUserState.Ready
                    && !room.Settings.AutoStartEnabled)
                {
                    return "Cancel countdown";
                }

                return base.TooltipText;
            }
        }
    }
}
