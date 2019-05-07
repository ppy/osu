// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Tests.Beatmaps.IO;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestCaseUpdateableBeatmapBackgroundSprite : OsuTestCase
    {
        private BeatmapSetInfo testBeatmap;
        private IAPIProvider api;
        private RulesetStore rulesets;

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osu, IAPIProvider api, RulesetStore rulesets)
        {
            this.api = api;
            this.rulesets = rulesets;

            testBeatmap = ImportBeatmapTest.LoadOszIntoOsu(osu);
        }

        [Test]
        public void TestNullBeatmap()
        {
            TestUpdateableBeatmapBackgroundSprite background = null;

            AddStep("load null beatmap", () => Child = background = new TestUpdateableBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both });
            AddUntilStep("wait for load", () => background.ContentLoaded);
        }

        [Test]
        public void TestLocalBeatmap()
        {
            TestUpdateableBeatmapBackgroundSprite background = null;

            AddStep("load local beatmap", () =>
            {
                Child = background = new TestUpdateableBeatmapBackgroundSprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Beatmap = { Value = testBeatmap.Beatmaps.First() }
                };
            });

            AddUntilStep("wait for load", () => background.ContentLoaded);
        }

        [Test]
        public void TestOnlineBeatmap()
        {
            if (api.IsLoggedIn)
            {
                var req = new GetBeatmapSetRequest(1);
                api.Queue(req);

                AddUntilStep("wait for api response", () => req.Result != null);

                TestUpdateableBeatmapBackgroundSprite background = null;

                AddStep("load online beatmap", () =>
                {
                    Child = background = new TestUpdateableBeatmapBackgroundSprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Beatmap = { Value = new BeatmapInfo { BeatmapSet = req.Result?.ToBeatmapSet(rulesets) } }
                    };
                });

                AddUntilStep("wait for load", () => background.ContentLoaded);
            }
            else
                AddStep("online (login first)", () => { });
        }

        private class TestUpdateableBeatmapBackgroundSprite : UpdateableBeatmapBackgroundSprite
        {
            public bool ContentLoaded => ((DelayedLoadUnloadWrapper)InternalChildren.LastOrDefault())?.Content?.IsLoaded ?? false;
        }
    }
}
