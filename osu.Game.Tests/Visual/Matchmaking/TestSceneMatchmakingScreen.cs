// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneMatchmakingScreen : MultiplayerTestScene
    {
        private const int user_count = 8;
        private const int beatmap_count = 50;

        private MultiplayerRoomUser[] users = null!;
        private ScreenMatchmaking screen = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () =>
            {
                var room = CreateDefaultRoom(MatchType.Matchmaking);
                room.Playlist = Enumerable.Range(1, 50).Select(i => new PlaylistItem(new MultiplayerPlaylistItem
                {
                    ID = i,
                    BeatmapID = i,
                    StarRating = i / 10.0,
                })).ToArray();

                JoinRoom(room);
            });

            WaitForJoined();

            AddStep("join users", () =>
            {
                for (int i = 0; i < 7; i++)
                {
                    MultiplayerClient.AddUser(new MultiplayerRoomUser(i)
                    {
                        User = new APIUser
                        {
                            Username = $"User {i}"
                        }
                    });
                }
            });

            setupRequestHandler();

            AddStep("load match", () =>
            {
                users = Enumerable.Range(1, user_count).Select(i => new MultiplayerRoomUser(i)
                {
                    User = new APIUser
                    {
                        Username = $"Player {i}"
                    }
                }).ToArray();

                var beatmaps = Enumerable.Range(1, beatmap_count).Select(i => new MultiplayerPlaylistItem
                {
                    BeatmapID = i,
                    StarRating = i / 10.0
                }).ToArray();

                LoadScreen(screen = new ScreenMatchmaking(new MultiplayerRoom(0)
                {
                    Users = users,
                    Playlist = beatmaps
                }));
            });
            AddUntilStep("wait for load", () => screen.IsCurrentScreen());
        }

        [Test]
        public void TestGameplayFlow()
        {
            for (int round = 1; round <= 3; round++)
            {
                AddLabel($"Round {round}");

                int r = round;
                changeStage(MatchmakingStage.RoundWarmupTime, state => state.CurrentRound = r);
                changeStage(MatchmakingStage.UserBeatmapSelect);
                changeStage(MatchmakingStage.ServerBeatmapFinalised, state =>
                {
                    MultiplayerPlaylistItem[] beatmaps = Enumerable.Range(1, 8).Select(i => new MultiplayerPlaylistItem
                    {
                        ID = i,
                        BeatmapID = i,
                        StarRating = i / 10.0,
                    }).ToArray();

                    state.CandidateItems = beatmaps.Select(b => b.ID).ToArray();
                    state.CandidateItem = beatmaps[0].ID;
                }, waitTime: 35);

                changeStage(MatchmakingStage.WaitingForClientsBeatmapDownload);
                changeStage(MatchmakingStage.GameplayWarmupTime);
                changeStage(MatchmakingStage.Gameplay);
                changeStage(MatchmakingStage.ResultsDisplaying);
            }

            changeStage(MatchmakingStage.Ended, state =>
            {
                int i = 1;

                foreach (var user in MultiplayerClient.ServerRoom!.Users.OrderBy(_ => RNG.Next()))
                {
                    state.Users[user.UserID].Placement = i++;
                    state.Users[user.UserID].Points = (8 - i) * 7;
                    state.Users[user.UserID].Rounds[1].Placement = 1;
                    state.Users[user.UserID].Rounds[1].TotalScore = 1;
                    state.Users[user.UserID].Rounds[1].Statistics[HitResult.LargeBonus] = 1;
                }
            });
        }

        private void changeStage(MatchmakingStage stage, Action<MatchmakingRoomState>? prepare = null, int waitTime = 5)
        {
            AddStep($"stage: {stage}", () => MultiplayerClient.MatchmakingChangeStage(stage, prepare).WaitSafely());
            AddWaitStep("wait", waitTime);
        }

        private void setupRequestHandler()
        {
            AddStep("setup request handler", () =>
            {
                Func<APIRequest, bool>? defaultRequestHandler = null;

                ((DummyAPIAccess)API).HandleRequest = request =>
                {
                    switch (request)
                    {
                        case GetBeatmapsRequest getBeatmaps:
                            getBeatmaps.TriggerSuccess(new GetBeatmapsResponse
                            {
                                Beatmaps = getBeatmaps.BeatmapIds.Select(id => new APIBeatmap
                                {
                                    OnlineID = id,
                                    StarRating = id,
                                    DifficultyName = $"Beatmap {id}",
                                    BeatmapSet = new APIBeatmapSet
                                    {
                                        Title = $"Title {id}",
                                        Artist = $"Artist {id}",
                                        AuthorString = $"Author {id}"
                                    }
                                }).ToList()
                            });
                            return true;

                        case IndexPlaylistScoresRequest index:
                            var result = new IndexedMultiplayerScores();

                            for (int i = 0; i < 8; ++i)
                            {
                                result.Scores.Add(new MultiplayerScore
                                {
                                    ID = i,
                                    Accuracy = 1 - (float)i / 16,
                                    Position = i + 1,
                                    EndedAt = DateTimeOffset.Now,
                                    Passed = true,
                                    Rank = (ScoreRank)RNG.Next((int)ScoreRank.D, (int)ScoreRank.XH),
                                    MaxCombo = 1000 - i,
                                    TotalScore = (long)(1_000_000 * (1 - (float)i / 16)),
                                    User = new APIUser { Username = $"user {i}" },
                                    Statistics = new Dictionary<HitResult, int>()
                                });
                            }

                            index.TriggerSuccess(result);
                            return true;

                        default:
                            return defaultRequestHandler?.Invoke(request) ?? false;
                    }
                };
            });
        }
    }
}
