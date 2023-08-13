// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net;
using NUnit.Framework;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Import;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneReplayMissingBeatmap : OsuGameTestScene
    {
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        [Test]
        public void TestSceneMissingBeatmapWithOnlineAvailable()
        {
            var beatmap = new APIBeatmap
            {
                OnlineBeatmapSetID = 173612,
                BeatmapSet = new APIBeatmapSet
                {
                    OnlineID = 173612
                }
            };

            setupBeatmapResponse(beatmap);

            AddStep("import score", () =>
            {
                using (var resourceStream = TestResources.OpenResource("Replays/mania-replay.osr"))
                {
                    var importTask = new ImportTask(resourceStream, "replay.osr");

                    Game.ScoreManager.Import(new[] { importTask });
                }
            });

            AddUntilStep("Replay missing screen show", () => Game.ScreenStack.CurrentScreen.GetType() == typeof(ReplayMissingBeatmapScreen));
        }

        [Test]
        public void TestSceneMissingBeatmapWithOnlineUnavailable()
        {
            setupFailedResponse();

            AddStep("import score", () =>
            {
                using (var resourceStream = TestResources.OpenResource("Replays/mania-replay.osr"))
                {
                    var importTask = new ImportTask(resourceStream, "replay.osr");

                    Game.ScoreManager.Import(new[] { importTask });
                }
            });

            AddUntilStep("Replay missing screen not show", () => Game.ScreenStack.CurrentScreen.GetType() != typeof(ReplayMissingBeatmapScreen));
        }

        private void setupBeatmapResponse(APIBeatmap b)
            => AddStep("setup response", () =>
            {
                dummyAPI.HandleRequest = request =>
                {
                    if (request is GetBeatmapRequest getBeatmapRequest)
                    {
                        getBeatmapRequest.TriggerSuccess(b);
                        return true;
                    }

                    return false;
                };
            });

        private void setupFailedResponse()
            => AddStep("setup failed response", () =>
            {
                dummyAPI.HandleRequest = request =>
                {
                    request.TriggerFailure(new WebException());
                    return true;
                };
            });
    }
}
