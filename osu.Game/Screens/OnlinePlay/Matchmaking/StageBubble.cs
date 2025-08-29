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
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    internal partial class StageBubble : CompositeDrawable
    {
        private readonly Color4 backgroundColour = Color4.Salmon;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private readonly int? round;

        private readonly MatchmakingStage stage;

        private readonly LocalisableString displayText;
        private Drawable progressBar = null!;

        private DateTimeOffset countdownStartTime;
        private DateTimeOffset countdownEndTime;
        private SpriteIcon arrow = null!;

        private Sample? stageProgressSample;
        private double? lastSamplePlayback;

        public bool Active { get; private set; }

        public StageBubble(int? round, MatchmakingStage stage, LocalisableString displayText)
        {
            this.round = round;
            this.stage = stage;
            this.displayText = displayText;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    arrow = new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Alpha = 0.5f,
                        Size = new Vector2(16),
                        Icon = FontAwesome.Solid.ArrowRight,
                        Margin = new MarginPadding { Horizontal = 10 }
                    },
                    new CircularContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
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
                    }
                }
            };

            stageProgressSample = audio.Samples.Get(@"Multiplayer/countdown-tick");
            Alpha = 0.5f;
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
            Active = false;

            if (state is not MatchmakingRoomState roomState)
                return;

            if (round != null && roomState.CurrentRound != round)
                return;

            Active = stage == roomState.Stage;

            bool isPreparing =
                (stage == MatchmakingStage.RoundWarmupTime && roomState.Stage == MatchmakingStage.WaitingForClientsJoin) ||
                (stage == MatchmakingStage.GameplayWarmupTime && roomState.Stage == MatchmakingStage.WaitingForClientsBeatmapDownload) ||
                (stage == MatchmakingStage.ResultsDisplaying && roomState.Stage == MatchmakingStage.Gameplay);

            if (isPreparing)
            {
                arrow.FadeTo(1, 500)
                     .Then()
                     .FadeTo(0.5f, 500)
                     .Loop();
            }
        });

        private void onCountdownStarted(MultiplayerCountdown countdown) => Scheduler.Add(() =>
        {
            if (!Active)
                return;

            if (countdown is not MatchmakingStageCountdown matchmakingState)
                return;

            countdownStartTime = DateTimeOffset.Now;
            countdownEndTime = countdownStartTime + countdown.TimeRemaining;
            arrow.FadeIn(500, Easing.OutQuint);
            this.FadeTo(1, 200);
        });

        private void onCountdownStopped(MultiplayerCountdown countdown) => Scheduler.Add(() =>
        {
            if (!Active)
                return;

            if (countdown is not MatchmakingStageCountdown)
                return;

            countdownEndTime = DateTimeOffset.Now;
        });

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
