// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSpectatorDrivenLeaderboard : OsuTestScene
    {
        [Cached(typeof(SpectatorStreamingClient))]
        private TestMultiplayerStreaming streamingClient = new TestMultiplayerStreaming(16);

        // used just to show beatmap card for the time being.
        protected override bool UseOnlineAPI => true;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            OsuScoreProcessor scoreProcessor;

            streamingClient.Start(Beatmap.Value.BeatmapInfo.OnlineBeatmapID ?? 0);

            Children = new Drawable[]
            {
                scoreProcessor = new OsuScoreProcessor(),
                new MultiplayerGameplayLeaderboard(scoreProcessor)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };

            Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);

            var playable = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value);

            scoreProcessor.ApplyBeatmap(playable);
        });

        [Test]
        public void TestScoreUpdates()
        {
            AddRepeatStep("update state", () => streamingClient.RandomlyUpdateState(), 100);
        }

        public class TestMultiplayerStreaming : SpectatorStreamingClient
        {
            public new BindableList<int> PlayingUsers => (BindableList<int>)base.PlayingUsers;

            private readonly int totalUsers;

            public TestMultiplayerStreaming(int totalUsers)
            {
                this.totalUsers = totalUsers;
            }

            public void Start(int beatmapId)
            {
                for (int i = 0; i < totalUsers; i++)
                {
                    ((ISpectatorClient)this).UserBeganPlaying(i, new SpectatorState
                    {
                        BeatmapID = beatmapId,
                        RulesetID = 0,
                    });
                }
            }

            private readonly Dictionary<int, FrameHeader> lastHeaders = new Dictionary<int, FrameHeader>();

            public void RandomlyUpdateState()
            {
                foreach (var userId in PlayingUsers)
                {
                    if (RNG.Next(0, 1) == 1)
                        continue;

                    if (!lastHeaders.TryGetValue(userId, out var header))
                    {
                        lastHeaders[userId] = header = new FrameHeader(new ScoreInfo
                        {
                            Statistics = new Dictionary<HitResult, int>(new[]
                            {
                                new KeyValuePair<HitResult, int>(HitResult.Miss, 0),
                                new KeyValuePair<HitResult, int>(HitResult.Meh, 0),
                                new KeyValuePair<HitResult, int>(HitResult.Great, 0)
                            })
                        });
                    }

                    switch (RNG.Next(0, 3))
                    {
                        case 0:
                            header.Combo = 0;
                            header.Statistics[HitResult.Miss]++;
                            break;

                        case 1:
                            header.Combo++;
                            header.MaxCombo = Math.Max(header.MaxCombo, header.Combo);
                            header.Statistics[HitResult.Meh]++;
                            break;

                        default:
                            header.Combo++;
                            header.MaxCombo = Math.Max(header.MaxCombo, header.Combo);
                            header.Statistics[HitResult.Great]++;
                            break;
                    }

                    ((ISpectatorClient)this).UserSentFrames(userId, new FrameDataBundle(header, Array.Empty<LegacyReplayFrame>()));
                }
            }

            protected override Task Connect() => Task.CompletedTask;
        }
    }
}
