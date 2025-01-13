// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.DailyChallenge;
using osuTK;

namespace osu.Game.Tests.Visual.DailyChallenge
{
    public partial class TestSceneDailyChallengeLeaderboard : OsuTestScene
    {
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        [Test]
        public void TestBasicBehaviour()
        {
            DailyChallengeLeaderboard leaderboard = null!;

            AddStep("set up response without user best", () =>
            {
                dummyAPI.HandleRequest = req =>
                {
                    if (req is IndexPlaylistScoresRequest indexRequest)
                    {
                        indexRequest.TriggerSuccess(createResponse(50, false));
                        return true;
                    }

                    return false;
                };
            });
            AddStep("create leaderboard", () => Child = leaderboard = new DailyChallengeLeaderboard(new Room { RoomID = 1 }, new PlaylistItem(Beatmap.Value.BeatmapInfo))
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(0.8f),
            });

            AddStep("set up response with user best", () =>
            {
                dummyAPI.HandleRequest = req =>
                {
                    if (req is IndexPlaylistScoresRequest indexRequest)
                    {
                        indexRequest.TriggerSuccess(createResponse(50, true));
                        return true;
                    }

                    return false;
                };
            });
            AddStep("force refetch", () => leaderboard.RefetchScores());
        }

        [Test]
        public void TestLoadingBehaviour()
        {
            IndexPlaylistScoresRequest pendingRequest = null!;
            DailyChallengeLeaderboard leaderboard = null!;

            AddStep("set up requests handler", () =>
            {
                dummyAPI.HandleRequest = req =>
                {
                    if (req is IndexPlaylistScoresRequest indexRequest)
                    {
                        pendingRequest = indexRequest;
                        return true;
                    }

                    return false;
                };
            });
            AddStep("create leaderboard", () => Child = leaderboard = new DailyChallengeLeaderboard(new Room { RoomID = 1 }, new PlaylistItem(Beatmap.Value.BeatmapInfo))
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(0.8f),
            });
            AddStep("complete load", () => pendingRequest.TriggerSuccess(createResponse(3, true)));
            AddStep("force refetch", () => leaderboard.RefetchScores());
            AddStep("complete load", () => pendingRequest.TriggerSuccess(createResponse(4, true)));
        }

        private IndexedMultiplayerScores createResponse(int scoreCount, bool returnUserBest)
        {
            var result = new IndexedMultiplayerScores();

            for (int i = 0; i < scoreCount; ++i)
            {
                result.Scores.Add(new MultiplayerScore
                {
                    ID = i,
                    Accuracy = 1 - (float)i / (2 * scoreCount),
                    Position = i + 1,
                    EndedAt = DateTimeOffset.Now,
                    Passed = true,
                    Rank = (ScoreRank)RNG.Next((int)ScoreRank.D, (int)ScoreRank.XH),
                    MaxCombo = 1000 - i,
                    TotalScore = (long)(1_000_000 * (1 - (float)i / (2 * scoreCount))),
                    User = new APIUser { Username = $"user {i}" },
                    Statistics = new Dictionary<HitResult, int>()
                });
            }

            if (returnUserBest)
            {
                result.UserScore = new MultiplayerScore
                {
                    ID = 99999,
                    Accuracy = 0.91,
                    Position = 4,
                    EndedAt = DateTimeOffset.Now,
                    Passed = true,
                    Rank = ScoreRank.A,
                    MaxCombo = 100,
                    TotalScore = 800000,
                    User = dummyAPI.LocalUser.Value,
                    Statistics = new Dictionary<HitResult, int>()
                };
            }

            return result;
        }
    }
}
