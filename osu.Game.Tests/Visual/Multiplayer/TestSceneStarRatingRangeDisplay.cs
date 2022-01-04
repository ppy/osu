// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneStarRatingRangeDisplay : OnlinePlayTestScene
    {
        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            SelectedRoom.Value = new Room();

            Child = new StarRatingRangeDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        });

        [Test]
        public void TestRange([Values(0, 2, 3, 4, 6, 7)] double min, [Values(0, 2, 3, 4, 6, 7)] double max)
        {
            AddStep("set playlist", () =>
            {
                SelectedRoom.Value.Playlist.AddRange(new[]
                {
                    new PlaylistItem { Beatmap = { Value = new BeatmapInfo { StarRating = min } } },
                    new PlaylistItem { Beatmap = { Value = new BeatmapInfo { StarRating = max } } },
                });
            });
        }
    }
}
