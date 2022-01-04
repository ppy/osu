// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerTeamResults : ScreenTestScene
    {
        [TestCase(7483253, 1048576)]
        [TestCase(1048576, 7483253)]
        [TestCase(1048576, 1048576)]
        public void TestDisplayTeamResults(int team1Score, int team2Score)
        {
            MultiplayerResultsScreen screen = null;

            AddStep("show results screen", () =>
            {
                var rulesetInfo = new OsuRuleset().RulesetInfo;
                var beatmapInfo = CreateBeatmap(rulesetInfo).BeatmapInfo;
                var score = TestResources.CreateTestScoreInfo(beatmapInfo);

                PlaylistItem playlistItem = new PlaylistItem
                {
                    BeatmapID = beatmapInfo.OnlineID ?? -1,
                };

                SortedDictionary<int, BindableInt> teamScores = new SortedDictionary<int, BindableInt>
                {
                    { 0, new BindableInt(team1Score) },
                    { 1, new BindableInt(team2Score) }
                };

                Stack.Push(screen = new MultiplayerTeamResultsScreen(score, 1, playlistItem, teamScores));
            });

            AddUntilStep("wait for loaded", () => screen.IsLoaded);
        }
    }
}
