// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    internal partial class StageBubble : CompositeDrawable
    {
        private readonly Color4 backgroundColour = Color4.Salmon;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private readonly MatchmakingStage stage;
        private readonly LocalisableString displayText;
        private Drawable progressBar = null!;

        private DateTimeOffset countdownStartTime;
        private DateTimeOffset countdownEndTime;

        private Sample? stageProgressSample;
        private double? lastSamplePlayback;

        public StageBubble(MatchmakingStage stage, LocalisableString displayText)
        {
            this.stage = stage;
            this.displayText = displayText;

            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            InternalChild = new CircularContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = backgroundColour.Darken(0.2f)
                    },
                    progressBar = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = backgroundColour
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = displayText,
                        Padding = new MarginPadding(10)
                    }
                }
            };

            stageProgressSample = audio.Samples.Get(@"Multiplayer/countdown-tick");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.MatchRoomStateChanged += onMatchRoomStateChanged;
            client.CountdownStarted += onCountdownStarted;
            client.CountdownStopped += onCountdownStopped;

            if (client.Room != null)
            {
                onMatchRoomStateChanged(client.Room.MatchState);
                foreach (var countdown in client.Room.ActiveCountdowns)
                    onCountdownStarted(countdown);
            }
        }

        protected override void Update()
        {
            base.Update();

            TimeSpan duration = countdownEndTime - countdownStartTime;

            if (duration.TotalMilliseconds == 0)
                progressBar.Width = 0;
            else
            {
                TimeSpan elapsed = DateTimeOffset.Now - countdownStartTime;
                progressBar.Width = (float)(elapsed.TotalMilliseconds / duration.TotalMilliseconds);

                bool enoughTimeElapsed = lastSamplePlayback == null || Time.Current - lastSamplePlayback >= 1000f;
                if (elapsed.TotalMilliseconds < 1000f || !enoughTimeElapsed || elapsed.TotalMilliseconds >= duration.TotalMilliseconds)
                    return;

                stageProgressSample?.Play();
                lastSamplePlayback = Time.Current;
            }
        }

        private void onMatchRoomStateChanged(MatchRoomState? state) => Scheduler.Add(() =>
        {
            if (state is not MatchmakingRoomState matchmakingState)
                return;

            if (matchmakingState.Stage == MatchmakingStage.RoundWarmupTime)
            {
                countdownStartTime = countdownEndTime = DateTimeOffset.Now;
                activate();
            }
        });

        private void onCountdownStarted(MultiplayerCountdown countdown) => Scheduler.Add(() =>
        {
            if (countdown is not MatchmakingStageCountdown matchmakingStatusCountdown || matchmakingStatusCountdown.Stage != stage)
                return;

            countdownStartTime = DateTimeOffset.Now;
            countdownEndTime = countdownStartTime + countdown.TimeRemaining;
            activate();
        });

        private void onCountdownStopped(MultiplayerCountdown countdown) => Scheduler.Add(() =>
        {
            if (countdown is not MatchmakingStageCountdown matchmakingStatusCountdown || matchmakingStatusCountdown.Stage != stage)
                return;

            countdownEndTime = DateTimeOffset.Now;
            deactivate();
        });

        private void activate()
        {
            this.FadeTo(1, 200);
        }

        private void deactivate()
        {
            this.FadeTo(0.5f, 200);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.MatchRoomStateChanged -= onMatchRoomStateChanged;
                client.CountdownStarted -= onCountdownStarted;
                client.CountdownStopped -= onCountdownStopped;
            }
        }
    }
}
