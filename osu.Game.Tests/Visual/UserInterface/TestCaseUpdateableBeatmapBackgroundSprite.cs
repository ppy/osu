// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Tests.Beatmaps.IO;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestCaseUpdateableBeatmapBackgroundSprite : OsuTestCase
    {
        private TestUpdateableBeatmapBackgroundSprite backgroundSprite;

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osu, IAPIProvider api, RulesetStore rulesets)
        {
            Bindable<BeatmapInfo> beatmapBindable = new Bindable<BeatmapInfo>();

            var imported = ImportBeatmapTest.LoadOszIntoOsu(osu);

            Child = backgroundSprite = new TestUpdateableBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both };

            backgroundSprite.Beatmap.BindTo(beatmapBindable);

            var req = new GetBeatmapSetRequest(1);
            api.Queue(req);

            AddStep("load null beatmap", () => beatmapBindable.Value = null);
            AddUntilStep("wait for cleanup...", () => backgroundSprite.ChildCount == 1);
            AddStep("load imported beatmap", () => beatmapBindable.Value = imported.Beatmaps.First());
            AddUntilStep("wait for cleanup...", () => backgroundSprite.ChildCount == 1);

            if (api.IsLoggedIn)
            {
                AddUntilStep("wait for api response", () => req.Result != null);
                AddStep("load online beatmap", () => beatmapBindable.Value = new BeatmapInfo
                {
                    BeatmapSet = req.Result?.ToBeatmapSet(rulesets)
                });
                AddUntilStep("wait for cleanup...", () => backgroundSprite.ChildCount == 1);
            }
            else
            {
                AddStep("online (login first)", () => { });
            }
        }

        private class TestUpdateableBeatmapBackgroundSprite : UpdateableBeatmapBackgroundSprite
        {
            public int ChildCount => InternalChildren.Count;
        }
    }
}
