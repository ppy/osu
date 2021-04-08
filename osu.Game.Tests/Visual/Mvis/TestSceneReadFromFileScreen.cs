using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Share;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Mvis
{
    public class TestSceneReadFromFileScreen : ScreenTestScene
    {
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        [Resolved]
        private BeatmapManager manager { get; set; }

        private APIBeatmapSet dummyBeatmapSet;

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            Dependencies.Cache(manager);

            List<BeatmapInfo> info = new List<BeatmapInfo>();
            List<BeatmapSetInfo> setInfo = new List<BeatmapSetInfo>();

            var normal = CreateBeatmap(Ruleset.Value).BeatmapInfo.BeatmapSet;

            info.Add(normal.Beatmaps.First());
            setInfo.Add(normal);

            dummyBeatmapSet = new APIBeatmapSet
            {
                OnlineBeatmapSetID = RNG.Next(0, 100),
                Author = new User
                {
                    Username = "Author",
                    Id = 1001,
                },
                Beatmaps = info,
                BeatmapSets = setInfo,
                Source = "Source",
                Artist = "Song Artist",
                Title = "Song Title"
            };
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            dummyAPI.HandleRequest = request =>
            {
                if (request is GetBeatmapSetRequest getBeatmapSetRequest)
                {
                    this.Delay(RNG.Next(0, 3001)).Schedule(() =>
                    {
                        if (RNG.Next(0, 11) == 0)
                            getBeatmapSetRequest.TriggerFailure(new TimeoutException());
                        else
                            getBeatmapSetRequest.TriggerSuccess(dummyBeatmapSet);
                    });
                }

                return true;
            };
        });

        [Test]
        public void CreateMvisScreen()
        {
            AddStep("Create screen", () =>
            {
                if (Stack.CurrentScreen != null)
                    Stack?.Exit();

                LoadScreen(new ReadFromFileScreen());
            });
        }
    }
}
