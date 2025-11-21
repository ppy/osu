// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneStarRatingRangeDisplay : OsuTestScene
    {
        private readonly Room room = new Room();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10),
                Children = new Drawable[]
                {
                    new StarRatingRangeDisplay(room)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(5),
                    },
                    new StarRatingRangeDisplay(room)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(2),
                    },
                    new StarRatingRangeDisplay(room)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(1),
                    },
                    new StarRatingRangeDisplay(room)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0.2f,
                        Scale = new Vector2(5),
                    },
                    new StarRatingRangeDisplay(room)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0.2f,
                        Scale = new Vector2(2),
                    },
                    new StarRatingRangeDisplay(room)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0.2f,
                        Scale = new Vector2(1),
                    },
                }
            };
        }

        [Test]
        public void TestRange([Values(0, 2, 3, 4, 6, 7)] double min, [Values(0, 2, 3, 4, 6, 7)] double max)
        {
            AddStep("set playlist", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(new BeatmapInfo { StarRating = min }) { ID = TestResources.GetNextTestID() },
                    new PlaylistItem(new BeatmapInfo { StarRating = max }) { ID = TestResources.GetNextTestID() },
                ];
            });
        }

        [Test]
        public void TestRangeUsesNonExpiredItemsIfThereAreAny()
        {
            AddStep("set up room", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(new BeatmapInfo { StarRating = 1 }) { ID = TestResources.GetNextTestID(), Expired = true },
                    new PlaylistItem(new BeatmapInfo { StarRating = 2 }) { ID = TestResources.GetNextTestID(), Expired = false },
                    new PlaylistItem(new BeatmapInfo { StarRating = 3 }) { ID = TestResources.GetNextTestID(), Expired = true },
                    new PlaylistItem(new BeatmapInfo { StarRating = 4 }) { ID = TestResources.GetNextTestID(), Expired = false },
                    new PlaylistItem(new BeatmapInfo { StarRating = 5 }) { ID = TestResources.GetNextTestID(), Expired = false },
                    new PlaylistItem(new BeatmapInfo { StarRating = 6 }) { ID = TestResources.GetNextTestID(), Expired = true },
                    new PlaylistItem(new BeatmapInfo { StarRating = 7 }) { ID = TestResources.GetNextTestID(), Expired = true },
                ];
            });
            AddAssert("minimum is 2.00*", () => this.ChildrenOfType<StarRatingDisplay>().ElementAt(0).Current.Value.Stars, () => Is.EqualTo(2));
            AddAssert("maximum is 5.00*", () => this.ChildrenOfType<StarRatingDisplay>().ElementAt(1).Current.Value.Stars, () => Is.EqualTo(5));
        }

        [Test]
        public void TestRangeUsesAllItemsIfAllAreExpired()
        {
            AddStep("set up room", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(new BeatmapInfo { StarRating = 1 }) { ID = TestResources.GetNextTestID(), Expired = true },
                    new PlaylistItem(new BeatmapInfo { StarRating = 2 }) { ID = TestResources.GetNextTestID(), Expired = true },
                    new PlaylistItem(new BeatmapInfo { StarRating = 3 }) { ID = TestResources.GetNextTestID(), Expired = true },
                    new PlaylistItem(new BeatmapInfo { StarRating = 4 }) { ID = TestResources.GetNextTestID(), Expired = true },
                    new PlaylistItem(new BeatmapInfo { StarRating = 5 }) { ID = TestResources.GetNextTestID(), Expired = true },
                    new PlaylistItem(new BeatmapInfo { StarRating = 6 }) { ID = TestResources.GetNextTestID(), Expired = true },
                    new PlaylistItem(new BeatmapInfo { StarRating = 7 }) { ID = TestResources.GetNextTestID(), Expired = true },
                ];
            });
            AddAssert("minimum is 1.00*", () => this.ChildrenOfType<StarRatingDisplay>().ElementAt(0).Current.Value.Stars, () => Is.EqualTo(1));
            AddAssert("maximum is 7.00*", () => this.ChildrenOfType<StarRatingDisplay>().ElementAt(1).Current.Value.Stars, () => Is.EqualTo(7));
        }
    }
}
