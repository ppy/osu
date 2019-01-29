// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Tests.Beatmaps.IO;

namespace osu.Game.Tests.Visual
{
    public class TestCaseUpdateableBeatmapBackgroundSprite : OsuTestCase
    {
        private UpdateableBeatmapBackgroundSprite backgroundSprite;

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osu, APIAccess api, RulesetStore rulesets)
        {
            Bindable<BeatmapInfo> beatmapBindable = new Bindable<BeatmapInfo>();

            var imported = ImportBeatmapTest.LoadOszIntoOsu(osu);

            Child = backgroundSprite = new UpdateableBeatmapBackgroundSprite { RelativeSizeAxes = Axes.Both };

            backgroundSprite.Beatmap.BindTo(beatmapBindable);

            var req = new GetBeatmapSetRequest(1);
            api.Queue(req);

            AddStep("null", () => beatmapBindable.Value = null);

            AddStep("imported", () => beatmapBindable.Value = imported.Beatmaps.First());

            if (api.IsLoggedIn)
            {
                AddUntilStep(() => req.Result != null, "wait for api response");

                AddStep("online", () => beatmapBindable.Value = new BeatmapInfo
                {
                    BeatmapSet = req.Result?.ToBeatmapSet(rulesets)
                });
            }
            else
            {
                AddStep("online (login first)", () => { });
            }
        }
    }
}
