// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
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
    }
}
