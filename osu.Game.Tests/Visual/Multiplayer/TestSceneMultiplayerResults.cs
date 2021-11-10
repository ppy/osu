// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Multiplayer;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerResults : ScreenTestScene
    {
        [Test]
        public void TestDisplayResults()
        {
            MultiplayerResultsScreen screen = null;

            AddStep("show results screen", () =>
            {
                var rulesetInfo = new OsuRuleset().RulesetInfo;
                var beatmapInfo = CreateBeatmap(rulesetInfo).BeatmapInfo;

                var score = new ScoreInfo
                {
                    Rank = ScoreRank.B,
                    TotalScore = 987654,
                    Accuracy = 0.8,
                    MaxCombo = 500,
                    Combo = 250,
                    BeatmapInfo = beatmapInfo,
                    User = new APIUser { Username = "Test user" },
                    Date = DateTimeOffset.Now,
                    OnlineScoreID = 12345,
                    Ruleset = rulesetInfo,
                };

                PlaylistItem playlistItem = new PlaylistItem
                {
                    BeatmapID = beatmapInfo.ID,
                };

                Stack.Push(screen = new MultiplayerResultsScreen(score, 1, playlistItem));
            });

            AddUntilStep("wait for loaded", () => screen.IsLoaded);
        }
    }
}
