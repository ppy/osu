// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Database;
using osu.Game.Online;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Tests.Visual.Multiplayer;
using osu.Game.Tests.Visual.Online;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneMultiplayerGameplayLeaderboard : MultiplayerTestScene
    {
        private const int users = 16;

        [Cached(typeof(SpectatorStreamingClient))]
        private TestMultiplayerStreaming streamingClient = new TestMultiplayerStreaming(users);

        [Cached(typeof(UserLookupCache))]
        private UserLookupCache lookupCache = new TestSceneCurrentlyPlayingDisplay.TestUserLookupCache();

        private MultiplayerGameplayLeaderboard leaderboard;

        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        public TestSceneMultiplayerGameplayLeaderboard()
        {
            base.Content.Children = new Drawable[]
            {
                streamingClient,
                lookupCache,
                Content
            };
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            AddStep("create leaderboard", () =>
            {
                leaderboard?.Expire();

                OsuScoreProcessor scoreProcessor;
                Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);

                var playable = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value);

                streamingClient.Start(Beatmap.Value.BeatmapInfo.OnlineBeatmapID ?? 0);

                Client.PlayingUsers.Clear();
                Client.PlayingUsers.AddRange(streamingClient.PlayingUsers);

                Children = new Drawable[]
                {
                    scoreProcessor = new OsuScoreProcessor(),
                };

                scoreProcessor.ApplyBeatmap(playable);

                LoadComponentAsync(leaderboard = new MultiplayerGameplayLeaderboard(scoreProcessor, streamingClient.PlayingUsers.ToArray())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }, Add);
            });

            AddUntilStep("wait for load", () => leaderboard.IsLoaded);
        }

        [Test]
        public void TestScoreUpdates()
        {
            AddRepeatStep("update state", () => streamingClient.RandomlyUpdateState(), 100);
        }

        [Test]
        public void TestUserQuit()
        {
            AddRepeatStep("mark user quit", () => Client.PlayingUsers.RemoveAt(0), users);
        }

        public class TestMultiplayerStreaming : SpectatorStreamingClient
        {
            public new BindableList<int> PlayingUsers => (BindableList<int>)base.PlayingUsers;

            private readonly int totalUsers;

            public TestMultiplayerStreaming(int totalUsers)
                : base(new DevelopmentEndpointConfiguration())
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
                    if (RNG.NextBool())
                        continue;

                    if (!lastHeaders.TryGetValue(userId, out var header))
                    {
                        lastHeaders[userId] = header = new FrameHeader(new ScoreInfo
                        {
                            Statistics = new Dictionary<HitResult, int>
                            {
                                [HitResult.Miss] = 0,
                                [HitResult.Meh] = 0,
                                [HitResult.Great] = 0
                            }
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
