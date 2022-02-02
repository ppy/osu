// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Tests.Resources;

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
                var score = TestResources.CreateTestScoreInfo(beatmapInfo);

                PlaylistItem playlistItem = new PlaylistItem
                {
                    BeatmapID = beatmapInfo.OnlineID,
                };

                Stack.Push(screen = new MultiplayerResultsScreen(score, 1, playlistItem));
            });

            AddUntilStep("wait for loaded", () => screen.IsLoaded);
        }
    }
}
