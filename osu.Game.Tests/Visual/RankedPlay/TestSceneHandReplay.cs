// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneHandReplay : MultiplayerTestScene
    {
        private PlayerHandOfCards playerHand = null!;
        private OpponentHandOfCards opponentHand = null!;
        private TestHandReplayRecorder recorder = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.RankedPlay)));
            WaitForJoined();

            AddStep("setup", () =>
            {
                var cards = Enumerable.Range(0, 5)
                                      .Select(_ => new RankedPlayCardWithPlaylistItem(new RankedPlayCardItem()))
                                      .ToArray();

                Children =
                [
                    playerHand = new PlayerHandOfCards
                    {
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.5f),
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        SelectionMode = HandSelectionMode.Multiple
                    },
                    opponentHand = new OpponentHandOfCards
                    {
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.5f),
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    },
                    new HandReplayPlayer(API.LocalUser.Value.OnlineID, opponentHand),
                    recorder = new TestHandReplayRecorder(playerHand)
                    {
                        FlushInterval = flushInterval,
                        RecordInterval = recordInterval,
                    }
                ];

                foreach (var card in cards)
                {
                    playerHand.AddCard(card);
                    opponentHand.AddCard(card);
                }
            });
        }

        private double flushInterval = 1000;
        private double recordInterval = 25;
        private double fixedLatency;
        private double maxLatency;

        [Test]
        public void TestCardHandReplay()
        {
            AddSliderStep("record interval", 0.0, 1000.0, 25.0, value =>
            {
                recordInterval = value;
                recreateRecorder();
            });
            AddSliderStep("flush interval", 0.0, 5000.0, 1000.0, value =>
            {
                flushInterval = value;
                recreateRecorder();
            });
            AddSliderStep("latency", 0.0, 5000.0, 0.0, value =>
            {
                fixedLatency = value;
                recreateRecorder();
            });
            AddSliderStep("randomize latency", 0.0, 5000.0, 0.0, value =>
            {
                maxLatency = value;
                recreateRecorder();
            });
        }

        private void recreateRecorder()
        {
            if (recorder.IsNotNull())
            {
                Remove(recorder, true);
                Add(recorder = new TestHandReplayRecorder(playerHand)
                {
                    FlushInterval = flushInterval,
                    RecordInterval = recordInterval,
                    FixedLatency = fixedLatency,
                    RandomLatency = maxLatency,
                });
            }
        }

        private partial class TestHandReplayRecorder(PlayerHandOfCards handOfCards) : HandReplayRecorder(handOfCards)
        {
            private double lastSendTime;

            public double FixedLatency;

            public double RandomLatency;

            protected override void Flush(RankedPlayCardHandReplayFrame[] frames)
            {
                double sendTime = Math.Max(lastSendTime, Time.Current + FixedLatency + RNG.NextDouble(RandomLatency));

                lastSendTime = sendTime;

                Scheduler.AddDelayed(() => base.Flush(frames), sendTime - Time.Current);
            }
        }
    }
}
