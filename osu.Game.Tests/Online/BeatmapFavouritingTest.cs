// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Online.API;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Online
{
    [TestFixture]
    public class BeatmapFavouritingTest
    {
        /// <summary>
        /// Tests whether <see cref="APIExtensions.GetFavouriteState"/> correctly handles beatmaps which were not favourited at first.
        /// </summary>
        [Test]
        public void TestNormalBeatmap()
        {
            var beatmapSet = OsuTestScene.CreateAPIBeatmapSet(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo);

            // beatmap not favourited initially
            beatmapSet.HasFavourited = false;
            beatmapSet.FavouriteCount = 1336;

            var api = new DummyAPIAccess();

            Assert.That(api.GetFavouriteState(beatmapSet), Is.EqualTo(new BeatmapSetFavouriteState(false, 1336)));

            api.AddToFavourites(beatmapSet);

            Assert.That(api.GetFavouriteState(beatmapSet), Is.EqualTo(new BeatmapSetFavouriteState(true, 1337)));

            api.RemoveFromFavourites(beatmapSet);

            Assert.That(api.GetFavouriteState(beatmapSet), Is.EqualTo(new BeatmapSetFavouriteState(false, 1336)));
        }

        /// <summary>
        /// Tests whether <see cref="APIExtensions.GetFavouriteState"/> correctly handles beatmaps which were favourited at first.
        /// </summary>
        [Test]
        public void TestFavouritedBeatmap()
        {
            var beatmapSet = OsuTestScene.CreateAPIBeatmapSet(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo);

            // beatmap favourited initially
            beatmapSet.HasFavourited = true;
            beatmapSet.FavouriteCount = 1337;

            var api = new DummyAPIAccess();
            api.AddToFavourites(beatmapSet);

            Assert.That(api.GetFavouriteState(beatmapSet), Is.EqualTo(new BeatmapSetFavouriteState(true, 1337)));

            api.RemoveFromFavourites(beatmapSet);

            Assert.That(api.GetFavouriteState(beatmapSet), Is.EqualTo(new BeatmapSetFavouriteState(false, 1336)));

            api.AddToFavourites(beatmapSet);

            Assert.That(api.GetFavouriteState(beatmapSet), Is.EqualTo(new BeatmapSetFavouriteState(true, 1337)));
        }
    }
}
