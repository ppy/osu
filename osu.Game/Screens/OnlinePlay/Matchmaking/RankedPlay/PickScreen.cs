// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class PickScreen : RankedPlaySubScreen
    {
        // When the 'time running out' warning sample starts to play (in remaining seconds)
        private const int warning_time_threshold = 10;

        public CardFlow CenterRow { get; private set; } = null!;

        protected override LocalisableString StageHeading => "Pick Phase";
        protected override LocalisableString StageCaption => "It's your turn to play a card!";

        private PlayerHandOfCards playerHand = null!;
        private OpponentHandOfCards opponentHand = null!;

        [Resolved]
        private RankedPlayMatchInfo matchInfo { get; set; } = null!;

        private Sample? cardAddSample;

        private const int card_play_samples = 2;
        private Sample?[]? cardPlaySamples;

        private Sample? timeRunningOutSample;
        private SampleChannel? timeRunningOutSampleChannel;
        private Sample? timeUpBuzzerSample;

        private DateTimeOffset stageEndTime;
        private TimeSpan stageDuration;

        /// <summary>
        /// Whether the local user has played a card themselves.
        /// </summary>
        private bool hasPlayedCard;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            var matchState = Client.Room?.MatchState as RankedPlayRoomState;

            Debug.Assert(matchState != null);

            Children =
            [
                CenterRow = new CardFlow
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            ];

            CenterColumn.Children =
            [
                opponentHand = new OpponentHandOfCards
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Y = -100,
                },
                playerHand = new PlayerHandOfCards
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    SelectionMode = HandSelectionMode.Single,
                    PlayCardAction = onPlayButtonClicked
                },
                new HandReplayRecorder(playerHand),
                new HandReplayPlayer(matchInfo.OpponentId, opponentHand),
            ];

            cardAddSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/card-add-1");

            cardPlaySamples = new Sample?[card_play_samples];
            for (int i = 0; i < card_play_samples; i++)
                cardPlaySamples[i] = audio.Samples.Get($@"Multiplayer/Matchmaking/Ranked/card-play-{1 + i}");

            timeRunningOutSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/time-running-out");
            timeUpBuzzerSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/time-up");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            matchInfo.CardPlayed += cardPlayed;

            Client.CountdownStarted += onCountdownStarted;
            Client.CountdownStopped += onCountdownStopped;

            if (Client.Room != null)
            {
                foreach (var countdown in Client.Room.ActiveCountdowns)
                    onCountdownStarted(countdown);
            }
        }

        private bool shouldPlayWarningSample
            => matchInfo.Stage.Value == RankedPlayStage.CardPlay
               && stageDuration > TimeSpan.FromSeconds(warning_time_threshold)
               && stageEndTime - DateTimeOffset.Now < TimeSpan.FromSeconds(warning_time_threshold)
               && !hasPlayedCard;

        protected override void Update()
        {
            base.Update();

            if (shouldPlayWarningSample)
            {
                timeRunningOutSampleChannel ??= timeRunningOutSample?.GetChannel();

                if (timeRunningOutSampleChannel == null || timeRunningOutSampleChannel.Playing)
                    return;

                timeRunningOutSampleChannel.ManualFree = true;
                timeRunningOutSampleChannel.Looping = true;
                timeRunningOutSampleChannel.Play();
            }
            else
                timeRunningOutSampleChannel?.Stop();
        }

        public override void OnEntering(RankedPlaySubScreen? previous)
        {
            base.OnEntering(previous);

            const double stagger = 50;
            double delay = 0;

            foreach (var item in matchInfo.PlayerCards)
            {
                double currentDelay = delay;

                if ((previous as DiscardScreen)?.CenterRow.RemoveCard(item, out var card, out var drawQuad) == true)
                {
                    playerHand.AddCard(card, c =>
                    {
                        c.MatchScreenSpaceDrawQuad(drawQuad, playerHand);
                        c.DelayMovementOnEntering(currentDelay);
                    });
                }
                else
                {
                    playerHand.AddCard(item, c =>
                    {
                        c.Position = playerHand.BottomCardInsertPosition;
                        c.DelayMovementOnEntering(currentDelay);
                    });
                    Scheduler.AddDelayed(() =>
                    {
                        SamplePlaybackHelper.PlayWithRandomPitch(cardAddSample);
                    }, delay);
                }

                delay += stagger;
            }

            delay = 0;

            foreach (var item in matchInfo.OpponentCards)
            {
                double currentDelay = delay;

                opponentHand.AddCard(item, c =>
                {
                    c.Position = ToSpaceOfOtherDrawable(new Vector2(DrawWidth / 2, 0), playerHand);
                    c.DelayMovementOnEntering(currentDelay);
                });

                delay += 50;
            }
        }

        private void onCountdownStarted(MultiplayerCountdown countdown) => Scheduler.Add(() =>
        {
            if (countdown is not RankedPlayStageCountdown)
                return;

            stageEndTime = DateTimeOffset.Now + countdown.TimeRemaining;
            stageDuration = countdown.TimeRemaining;
        });

        private void onCountdownStopped(MultiplayerCountdown countdown) => Scheduler.Add(() =>
        {
            if (countdown is not RankedPlayStageCountdown stageCountdown)
                return;

            stageEndTime = DateTimeOffset.Now;
            stageDuration = TimeSpan.Zero;

            if (stageCountdown.Stage == RankedPlayStage.CardPlay && !hasPlayedCard)
                timeUpBuzzerSample?.Play();
        });

        private void onPlayButtonClicked()
        {
            var selection = playerHand.Selection.SingleOrDefault();

            if (selection != null)
            {
                hasPlayedCard = true;
                playerHand.SelectionMode = HandSelectionMode.Disabled;

                Client.PlayCard(selection.Card).FireAndForget();
            }

            playerHand.PlayCardAction = null;
        }

        private void cardPlayed(RankedPlayCardWithPlaylistItem item)
        {
            RankedPlayCard? card;

            if (playerHand.RemoveCard(item, out card, out var drawQuad))
            {
                card.MatchScreenSpaceDrawQuad(drawQuad, CenterRow);
            }
            else
            {
                Logger.Log($"Played card {item.Card.ID} was not present in hand.", level: LogLevel.Error);

                card = new RankedPlayCard(item)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            }

            CenterRow.Add(card);

            card
                .MoveTo(new Vector2(0), 600, Easing.OutExpo)
                .ScaleTo(CENTERED_CARD_SCALE, 600, Easing.OutExpo)
                .RotateTo(0, 400, Easing.OutExpo);

            SamplePlaybackHelper.PlayWithRandomPitch(cardPlaySamples);

            opponentHand.Contract();
            playerHand.Contract();

            playerHand.SelectionMode = HandSelectionMode.Disabled;
        }

        protected override void Dispose(bool isDisposing)
        {
            timeRunningOutSampleChannel?.Stop();
            timeRunningOutSampleChannel?.Dispose();

            matchInfo.CardPlayed -= cardPlayed;

            base.Dispose(isDisposing);
        }
    }
}
