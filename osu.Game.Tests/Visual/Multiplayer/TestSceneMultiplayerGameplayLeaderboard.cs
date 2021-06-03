// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Tests.Visual.Online;
using osu.Game.Tests.Visual.Spectator;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerGameplayLeaderboard : MultiplayerTestScene
    {
        private const int users = 16;

        [Cached(typeof(SpectatorClient))]
        private TestMultiplayerSpectatorClient spectatorClient = new TestMultiplayerSpectatorClient();

        [Cached(typeof(UserLookupCache))]
        private UserLookupCache lookupCache = new TestSceneCurrentlyPlayingDisplay.TestUserLookupCache();

        private MultiplayerGameplayLeaderboard leaderboard;

        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        private OsuConfigManager config;

        public TestSceneMultiplayerGameplayLeaderboard()
        {
            base.Content.Children = new Drawable[]
            {
                spectatorClient,
                lookupCache,
                Content
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(config = new OsuConfigManager(LocalStorage));
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            AddStep("set local user", () => ((DummyAPIAccess)API).LocalUser.Value = lookupCache.GetUserAsync(1).Result);

            AddStep("create leaderboard", () =>
            {
                leaderboard?.Expire();

                OsuScoreProcessor scoreProcessor;
                Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);

                var playable = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value);

                for (int i = 0; i < users; i++)
                    spectatorClient.StartPlay(i, Beatmap.Value.BeatmapInfo.OnlineBeatmapID ?? 0);

                spectatorClient.Schedule(() =>
                {
                    Client.CurrentMatchPlayingUserIds.Clear();
                    Client.CurrentMatchPlayingUserIds.AddRange(spectatorClient.PlayingUsers);
                });

                Children = new Drawable[]
                {
                    scoreProcessor = new OsuScoreProcessor(),
                };

                scoreProcessor.ApplyBeatmap(playable);

                LoadComponentAsync(leaderboard = new MultiplayerGameplayLeaderboard(scoreProcessor, spectatorClient.PlayingUsers.ToArray())
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
            AddRepeatStep("update state", () => spectatorClient.RandomlyUpdateState(), 100);
            AddToggleStep("switch compact mode", expanded => leaderboard.Expanded.Value = expanded);
        }

        [Test]
        public void TestUserQuit()
        {
            AddRepeatStep("mark user quit", () => Client.CurrentMatchPlayingUserIds.RemoveAt(0), users);
        }

        [Test]
        public void TestChangeScoringMode()
        {
            AddRepeatStep("update state", () => spectatorClient.RandomlyUpdateState(), 5);
            AddStep("change to classic", () => config.SetValue(OsuSetting.ScoreDisplayMode, ScoringMode.Classic));
            AddStep("change to standardised", () => config.SetValue(OsuSetting.ScoreDisplayMode, ScoringMode.Standardised));
        }

        public class TestMultiplayerSpectatorClient : TestSpectatorClient
        {
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

                    ((ISpectatorClient)this).UserSentFrames(userId, new FrameDataBundle(header, new[] { new LegacyReplayFrame(Time.Current, 0, 0, ReplayButtonState.None) }));
                }
            }
        }
    }
}
