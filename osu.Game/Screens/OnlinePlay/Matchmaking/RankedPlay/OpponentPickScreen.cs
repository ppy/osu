// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class OpponentPickScreen : RankedPlaySubScreen
    {
        public CardFlow CenterRow { get; private set; } = null!;

        protected override LocalisableString StageHeading => "Pick Phase";
        protected override LocalisableString StageCaption => "Waiting for your opponent...";

        protected override RankedPlayColourScheme ColourScheme => RankedPlayColourScheme.Red;

        private PlayerHandOfCards playerHand = null!;
        private OpponentHandOfCards opponentHand = null!;

        [Resolved]
        private RankedPlayMatchInfo matchInfo { get; set; } = null!;

        private Sample? cardAddSample;

        private const int card_play_samples = 2;
        private Sample?[]? cardPlaySamples;

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
                },
                playerHand = new PlayerHandOfCards
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                },
                new HandReplayRecorder(playerHand),
                new HandReplayPlayer(matchInfo.OpponentId, opponentHand),
            ];

            cardAddSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/card-add-1");

            cardPlaySamples = new Sample?[card_play_samples];
            for (int i = 0; i < card_play_samples; i++)
                cardPlaySamples[i] = audio.Samples.Get($@"Multiplayer/Matchmaking/Ranked/card-play-{1 + i}");
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

            foreach (var card in matchInfo.OpponentCards)
            {
                double currentDelay = delay;

                opponentHand.AddCard(card, c =>
                {
                    c.Position = opponentHand.BottomCardInsertPosition;
                    c.DelayMovementOnEntering(currentDelay);
                });

                delay += 50;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            matchInfo.CardPlayed += cardPlayed;
        }

        private void cardPlayed(RankedPlayCardWithPlaylistItem item) => Task.Run(async () =>
        {
            if (opponentHand.Cards.FirstOrDefault(it => it.Card.Item.Equals(item)) is { } c)
                await c.Card.CardRevealed.ConfigureAwait(false);

            Schedule(() =>
            {
                RankedPlayCard? card;

                if (opponentHand.RemoveCard(item, out card, out var drawQuad))
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

                SamplePlaybackHelper.PlayWithRandomPitch(cardPlaySamples);

                card
                    .MoveTo(new Vector2(0), 600, Easing.OutExpo)
                    .ScaleTo(CENTERED_CARD_SCALE, 600, Easing.OutExpo)
                    .RotateTo(0, 400, Easing.OutExpo);

                opponentHand.Contract();
                playerHand.Contract();
            });
        });

        protected override void Dispose(bool isDisposing)
        {
            matchInfo.CardPlayed -= cardPlayed;

            base.Dispose(isDisposing);
        }
    }
}
