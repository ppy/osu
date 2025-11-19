// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.RoundResults;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneRoundResultsScreen : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.Matchmaking)));
            WaitForJoined();

            setupRequestHandler();

            AddStep("load screen", () =>
            {
                Child = new ScreenStack(new SubScreenRoundResults())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.8f)
                };
            });
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
