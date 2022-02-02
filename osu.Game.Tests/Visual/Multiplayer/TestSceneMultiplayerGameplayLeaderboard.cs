// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Tests.Visual.Spectator;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerGameplayLeaderboard : MultiplayerTestScene
    {
        private static IEnumerable<int> users => Enumerable.Range(0, 16);

        public new TestMultiplayerSpectatorClient SpectatorClient => (TestMultiplayerSpectatorClient)OnlinePlayDependencies?.SpectatorClient;

        private MultiplayerGameplayLeaderboard leaderboard;
        private OsuConfigManager config;

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(config = new OsuConfigManager(LocalStorage));
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("set local user", () => ((DummyAPIAccess)API).LocalUser.Value = LookupCache.GetUserAsync(1).GetResultSafely());

            AddStep("create leaderboard", () =>
            {
                leaderboard?.Expire();

                OsuScoreProcessor scoreProcessor;
                Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);

                var playableBeatmap = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value);
                var multiplayerUsers = new List<MultiplayerRoomUser>();

                foreach (int user in users)
                {
                    SpectatorClient.StartPlay(user, Beatmap.Value.BeatmapInfo.OnlineID);
                    multiplayerUsers.Add(OnlinePlayDependencies.Client.AddUser(new APIUser { Id = user }, true));
                }

                Children = new Drawable[]
                {
                    scoreProcessor = new OsuScoreProcessor(),
                };

                scoreProcessor.ApplyBeatmap(playableBeatmap);

                LoadComponentAsync(leaderboard = new MultiplayerGameplayLeaderboard(scoreProcessor, multiplayerUsers.ToArray())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }, Add);
            });

            AddUntilStep("wait for load", () => leaderboard.IsLoaded);
            AddUntilStep("wait for user population", () => Client.CurrentMatchPlayingUserIds.Count > 0);
        }

        [Test]
        public void TestScoreUpdates()
        {
            AddRepeatStep("update state", () => SpectatorClient.RandomlyUpdateState(), 100);
            AddToggleStep("switch compact mode", expanded => leaderboard.Expanded.Value = expanded);
        }

        [Test]
        public void TestUserQuit()
        {
            foreach (int user in users)
                AddStep($"mark user {user} quit", () => Client.RemoveUser(LookupCache.GetUserAsync(user).GetResultSafely().AsNonNull()));
        }

        [Test]
        public void TestChangeScoringMode()
        {
            AddRepeatStep("update state", () => SpectatorClient.RandomlyUpdateState(), 5);
            AddStep("change to classic", () => config.SetValue(OsuSetting.ScoreDisplayMode, ScoringMode.Classic));
            AddStep("change to standardised", () => config.SetValue(OsuSetting.ScoreDisplayMode, ScoringMode.Standardised));
        }

        protected override OnlinePlayTestSceneDependencies CreateOnlinePlayDependencies() => new TestDependencies();

        protected class TestDependencies : MultiplayerTestSceneDependencies
        {
            protected override TestSpectatorClient CreateSpectatorClient() => new TestMultiplayerSpectatorClient();
        }

        public class TestMultiplayerSpectatorClient : TestSpectatorClient
        {
            private readonly Dictionary<int, FrameHeader> lastHeaders = new Dictionary<int, FrameHeader>();

            public void RandomlyUpdateState()
            {
                foreach (int userId in PlayingUsers)
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

                    ((ISpectatorClient)this).UserSentFrames(userId, new FrameDataBundle(header, new[] { new LegacyReplayFrame(Time.Current, 0, 0, ReplayButtonState.None) }));
                }
            }
        }
    }
}
