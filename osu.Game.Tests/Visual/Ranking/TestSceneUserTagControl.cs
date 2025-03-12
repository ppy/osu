// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Ranking;

namespace osu.Game.Tests.Visual.Ranking
{
    public partial class TestSceneUserTagControl : OsuTestScene
    {
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("set up working beatmap", () =>
            {
                Beatmap.Value.BeatmapInfo.OnlineID = 42;
            });
            AddStep("set up network requests", () =>
            {
                dummyAPI.HandleRequest = request =>
                {
                    switch (request)
                    {
                        case ListTagsRequest listTagsRequest:
                        {
                            Scheduler.AddDelayed(() => listTagsRequest.TriggerSuccess(new APITagCollection
                            {
                                Tags =
                                [
                                    new APITag { Id = 1, Name = "tech", Description = "Tests uncommon skills.", },
                                    new APITag { Id = 2, Name = "alt", Description = "Colloquial term for maps which use rhythms that encourage the player to alternate notes. Typically distinct from burst or stream maps.", },
                                    new APITag { Id = 3, Name = "aim", Description = "Category for difficulty relating to cursor movement.", },
                                    new APITag { Id = 4, Name = "tap", Description = "Category for difficulty relating to tapping input.", },
                                ]
                            }), 500);
                            return true;
                        }

                        case GetBeatmapSetRequest getBeatmapSetRequest:
                        {
                            var beatmapSet = CreateAPIBeatmapSet(Beatmap.Value.BeatmapInfo);
                            beatmapSet.Beatmaps.Single().TopTags =
                            [
                                new APIBeatmapTag { TagId = 3, VoteCount = 9 },
                            ];
                            Scheduler.AddDelayed(() => getBeatmapSetRequest.TriggerSuccess(beatmapSet), 500);
                            return true;
                        }

                        case AddBeatmapTagRequest:
                        case RemoveBeatmapTagRequest:
                        {
                            Scheduler.AddDelayed(request.TriggerSuccess, 500);
                            return true;
                        }
                    }

                    return false;
                };
            });
            AddStep("create control", () =>
            {
                Child = new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new UserTagControl
                    {
                        Width = 500,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };
            });
        }
    }
}
