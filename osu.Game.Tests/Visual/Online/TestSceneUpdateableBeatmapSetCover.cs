// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneUpdateableBeatmapSetCover : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(UpdateableBeatmapSetCover),
            typeof(BeatmapSetCover),
        };

        private readonly UpdateableBeatmapSetCover cover, card, list;

        public TestSceneUpdateableBeatmapSetCover()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10),
                Children = new[]
                {
                    cover = new UpdateableBeatmapSetCover(BeatmapSetCoverType.Cover) { Size = new Vector2(400), Masking = true, },
                    card = new UpdateableBeatmapSetCover(BeatmapSetCoverType.Card) { Size = new Vector2(400, 200), Masking = true, },
                    list = new UpdateableBeatmapSetCover(BeatmapSetCoverType.List) { Size = new Vector2(600, 150), Masking = true, },
                }
            };
        }

        [Test]
        public void TestLoading()
        {
            AddStep("loading", () => setBeatmap(null));
        }

        [Test]
        public void TestLocal()
        {
            AddStep("loading", () =>
            {
                setBeatmap(new BeatmapSetInfo
                {
                    OnlineInfo = new BeatmapSetOnlineInfo
                    {
                        Covers = new BeatmapSetOnlineCovers
                        {
                            Cover = "https://assets.ppy.sh/beatmaps/241526/covers/cover@2x.jpg?1521086997",
                            Card = "https://assets.ppy.sh/beatmaps/241526/covers/card@2x.jpg?1521086997",
                            List = "https://assets.ppy.sh/beatmaps/241526/covers/list@2x.jpg?1521086997",
                        }
                    }
                });
            });
        }

        private void setBeatmap(BeatmapSetInfo setInfo)
        {
            cover.BeatmapSet = setInfo;
            card.BeatmapSet = setInfo;
            list.BeatmapSet = setInfo;
        }
    }
}
