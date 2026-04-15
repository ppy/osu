// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class DiscardScreen : RankedPlaySubScreen
    {
        // When the 'time running out' warning sample starts to play (in remaining seconds)
        private const int warning_time_threshold = 10;

        public CardFlow CenterRow { get; private set; } = null!;

        public override bool ShowStageOverlay => true;
        public override LocalisableString StageHeading => "Discard Phase";

        private PlayerHandOfCards playerHand = null!;
        private ShearedButton discardButton = null!;
        private OsuTextFlowContainer explainer = null!;

        [Resolved]
        private RankedPlayMatchInfo matchInfo { get; set; } = null!;

        private Sample? cardAddSample;
        private Sample? cardDiscardSample;

        private const int card_play_samples = 2;
        private Sample?[]? cardPlaySamples;

        /// <summary>
        /// Whether the local user has discarded cards.
        /// </summary>
        private bool hasDiscardedCards;

        private Sample? timeRunningOutSample;
        private SampleChannel? timeRunningOutSampleChannel;

        private DateTimeOffset stageEndTime;
        private TimeSpan stageDuration;

        private ScheduledDelegate? waitingOpponentTextUpdate;

        public DiscardScreen()
        {
            StageCaption = "Replace cards from your hand";
        }

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
                discardButton = new ShearedButton
                {
                    Name = "Discard Button",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 150,
                    Action = onDiscardButtonClicked,
                    Enabled = { Value = true },
                },
                playerHand = new PlayerHandOfCards
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    SelectionMode = HandSelectionMode.Multiple,
                },
                explainer = new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(size: 24))
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    TextAnchor = Anchor.TopCentre,
                    Y = 250,
                    ParagraphSpacing = 1,
                    Alpha = 0,
                }.With(d =>
                {
                    d.AddParagraph("These are your cards for this match!");
                    d.AddParagraph("When it’s your turn, you can play a card to go head-to-head against your opponent!");
                })
            ];

            cardAddSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/card-add-1");
            cardDiscardSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/card-discard-1");

            cardPlaySamples = new Sample?[card_play_samples];
            for (int i = 0; i < card_play_samples; i++)
                cardPlaySamples[i] = audio.Samples.Get($@"Multiplayer/Matchmaking/Ranked/card-play-{1 + i}");

            timeRunningOutSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/time-running-out");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            matchInfo.PlayerCardAdded += cardAdded;
            matchInfo.PlayerCardRemoved += cardRemoved;

            playerHand.SelectionChanged += onSelectionChanged;

            Client.CountdownStarted += onCountdownStarted;
            Client.CountdownStopped += onCountdownStopped;

            if (Client.Room != null)
            {
                foreach (var countdown in Client.Room.ActiveCountdowns)
                    onCountdownStarted(countdown);
            }

            onSelectionChanged();
        }

        private bool shouldPlayWarningSample
            => matchInfo.Stage.Value == RankedPlayStage.CardDiscard
               && stageDuration > TimeSpan.FromSeconds(warning_time_threshold)
               && stageEndTime - DateTimeOffset.Now < TimeSpan.FromSeconds(warning_time_threshold)
               && !hasDiscardedCards;

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

            double delay = 0;
            const double stagger = 50;

            foreach (var card in matchInfo.PlayerCards)
            {
                double currentDelay = delay;

                playerHand.AddCard(card, c =>
                {
                    c.Position = playerHand.BottomCardInsertPosition;
                    c.DelayMovementOnEntering(currentDelay);
                });

                Scheduler.AddDelayed(() => SamplePlaybackHelper.PlayWithRandomPitch(cardAddSample), delay);

                delay += stagger;
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
            if (countdown is not RankedPlayStageCountdown)
                return;

            stageEndTime = DateTimeOffset.Now;
            stageDuration = TimeSpan.Zero;
        });

        private void onSelectionChanged()
        {
            if (playerHand.Selection.Any())
                discardButton.Text = $"Replace {"card".ToQuantity(playerHand.Selection.Count())}";
            else
                discardButton.Text = "Keep cards";
        }

        private void onDiscardButtonClicked()
        {
            discardButton.Hide();

            Client.DiscardCards(playerHand.Selection.Select(it => it.Card).ToArray()).FireAndForget();
            playerHand.SelectionMode = HandSelectionMode.Disabled;

            hasDiscardedCards = true;

            StageCaption = string.Empty;

            // A bit awkward, but we're delaying this until we're mostly sure the opponent is still discarding.
            // See the countdown reset logic for DiscardStage which gives 3 seconds for animation.
            waitingOpponentTextUpdate = Scheduler.AddDelayed(() => StageCaption = "Waiting for your opponent...", 3200);
        }

        private readonly List<RankedPlayCardWithPlaylistItem> discardedCards = new List<RankedPlayCardWithPlaylistItem>();

        private void cardRemoved(RankedPlayCardWithPlaylistItem item) => discardedCards.Add(item);

        private void playDiscardAnimation()
        {
            const double stagger = 100;
            double delay = 0;

            foreach (var item in discardedCards)
            {
                if (!playerHand.RemoveCard(item, out var card, out Quad drawQuad))
                    return;

                card.Anchor = Anchor.Centre;
                card.Origin = Anchor.Centre;

                card.SongPreviewEnabled.Value = false;

                card.MatchScreenSpaceDrawQuad(drawQuad, CenterRow);

                CenterRow.Add(card);

                using (BeginDelayedSequence(1000 + delay))
                {
                    card.PopOutAndExpire();
                }

                Scheduler.AddDelayed(() =>
                {
                    SamplePlaybackHelper.PlayWithRandomPitch(cardPlaySamples);
                }, delay);

                delay += stagger;
            }

            Scheduler.AddDelayed(() =>
            {
                cardDiscardSample?.Play();
            }, 1000);

            discardedCards.Clear();
            CenterRow.LayoutCards(stagger: stagger);
        }

        private double nextCardDrawTime;
        private double earliestPresentationTime;

        private void cardAdded(RankedPlayCardWithPlaylistItem card)
        {
            if (discardedCards.Count > 0)
            {
                playDiscardAnimation();
                nextCardDrawTime = Math.Max(nextCardDrawTime, Time.Current + 2000);
            }

            double delay = Math.Max(0, nextCardDrawTime - Time.Current);
            nextCardDrawTime = Time.Current + delay + 100;

            earliestPresentationTime = Time.Current + 3500;

            Scheduler.AddDelayed(() =>
            {
                playerHand.AddCard(card, d =>
                {
                    // card should enter from centre-right of screen
                    var cardEnterPosition = ToSpaceOfOtherDrawable(new Vector2(DrawWidth, DrawHeight * 0.5f), playerHand);
                    d.SetupMovementForDrawnCard(cardEnterPosition);
                });

                SamplePlaybackHelper.PlayWithRandomPitch(cardAddSample);
            }, delay);
        }

        public void PresentRemainingCards()
        {
            discardButton.Hide();

            double presentationTime = Math.Max(earliestPresentationTime, Time.Current);
            Scheduler.AddDelayed(presentRemainingCards, presentationTime - Time.Current);

            waitingOpponentTextUpdate?.Cancel();
            StageCaption = string.Empty;
        }

        private void presentRemainingCards()
        {
            int delay = 0;

            foreach (var item in matchInfo.PlayerCards)
            {
                if (playerHand.RemoveCard(item, out var card, out Quad drawQuad))
                {
                    card.MatchScreenSpaceDrawQuad(drawQuad, CenterRow);

                    CenterRow.Add(card);

                    Scheduler.AddDelayed(() =>
                    {
                        SamplePlaybackHelper.PlayWithRandomPitch(cardPlaySamples);
                    }, delay);

                    delay += 50;
                }
                else
                {
                    CenterRow.Add(new RankedPlayCard(item)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    });
                }
            }

            CenterRow.LayoutCards(stagger: 50, duration: 600);

            explainer
                .Delay(100)
                .MoveToOffset(new Vector2(0, 50))
                .MoveToOffset(new Vector2(0, -50), 600, Easing.OutExpo)
                .FadeIn(250);
        }

        protected override void Dispose(bool isDisposing)
        {
            timeRunningOutSampleChannel?.Stop();
            timeRunningOutSampleChannel?.Dispose();

            matchInfo.PlayerCardAdded -= cardAdded;
            matchInfo.PlayerCardRemoved -= cardRemoved;

            base.Dispose(isDisposing);
        }
    }
}
