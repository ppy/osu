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

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            Dependencies.Cache(manager);
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            dummyAPI.HandleRequest = request =>
            {
                switch (request)
                {
                    case GetBeatmapSetRequest getBeatmapSetRequest:
                        List<BeatmapInfo> info = new List<BeatmapInfo>();
                        List<BeatmapSetInfo> setInfo = new List<BeatmapSetInfo>();

                        var normal = CreateBeatmap(Ruleset.Value).BeatmapInfo.BeatmapSet;

                        info.Add(normal.Beatmaps.First());
                        setInfo.Add(normal);

                        var set = new APIBeatmapSet
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

                        this.Delay(1500).Schedule(() =>
                        {
                            getBeatmapSetRequest.TriggerSuccess(set);
                        });

                        break;
                }
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
