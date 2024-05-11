// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerTeamResults : ScreenTestScene
    {
        [Test]
        public void TestScaling()
        {
            // scheduling is needed as scaling the content immediately causes the entire scene to shake badly, for some odd reason.
            AddSliderStep("scale", 0.5f, 1.6f, 1f, v => Schedule(() =>
            {
                Stack.Scale = new Vector2(v);
                Stack.Size = new Vector2(1f / v);
            }));
        }

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

                SortedDictionary<int, BindableLong> teamScores = new SortedDictionary<int, BindableLong>
                {
                    { 0, new BindableLong(team1Score) },
                    { 1, new BindableLong(team2Score) }
                };

                Stack.Push(screen = new MultiplayerTeamResultsScreen(score, 1, new PlaylistItem(beatmapInfo), teamScores));
            });

            AddUntilStep("wait for loaded", () => screen.IsLoaded);
        }
    }
}
