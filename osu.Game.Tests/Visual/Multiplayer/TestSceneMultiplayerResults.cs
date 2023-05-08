// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerResults : ScreenTestScene
    {
        [Test]
        public void TestDisplayResults()
        {
            MultiplayerResultsScreen screen = null;

            AddStep("show results screen", () =>
            {
                var rulesetInfo = new OsuRuleset().RulesetInfo;
                var beatmapInfo = CreateBeatmap(rulesetInfo).BeatmapInfo;
                var score = TestResources.CreateTestScoreInfo(beatmapInfo);

                Stack.Push(screen = new MultiplayerResultsScreen(score, 1, new PlaylistItem(beatmapInfo)));
            });

            AddUntilStep("wait for loaded", () => screen.IsLoaded);
        }
    }
}
