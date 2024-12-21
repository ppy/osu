// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Seasonal;

namespace osu.Game.Tests.Visual.Menus
{
    public partial class TestSceneMainMenuSeasonalLighting : OsuTestScene
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("prepare beatmap", () =>
            {
                var setInfo = beatmaps.QueryBeatmapSet(b => b.Protected && b.Hash == IntroChristmas.CHRISTMAS_BEATMAP_SET_HASH);

                if (setInfo != null)
                    Beatmap.Value = beatmaps.GetWorkingBeatmap(setInfo.Value.Beatmaps.First());
            });

            AddStep("create lighting", () => Child = new MainMenuSeasonalLighting());

            AddStep("restart beatmap", () =>
            {
                Beatmap.Value.Track.Start();
                Beatmap.Value.Track.Seek(4000);
            });
        }

        [Test]
        public void TestBasic()
        {
        }
    }
}
