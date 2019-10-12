// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly FillFlowContainer<TestUpdateableBeatmapSetCover> covers;

        public TestSceneUpdateableBeatmapSetCover()
        {
            Child = covers = new FillFlowContainer<TestUpdateableBeatmapSetCover>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10),
                Children = Enum.GetValues(typeof(BeatmapSetCoverType)).Cast<BeatmapSetCoverType>().Select(t => new TestUpdateableBeatmapSetCover(t)).ToList(),
            };
        }

        [SetUp]
        public void SetUp()
        {
            foreach (var cover in covers)
                cover.BeatmapSet = null;
        }

        [Test]
        public void TestLocal()
        {
            var setInfo = new BeatmapSetInfo
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
            };

            foreach (var cover in covers)
            {
                var coverType = cover.Type.ToString().ToLower();
                AddStep($"set beatmap for {coverType}", () => cover.BeatmapSet = setInfo);
                AddUntilStep($"{coverType} drawable is displayed", () => cover.DisplayedDrawable is BeatmapSetCover);
                AddAssert($"ensure {coverType} has correct props", () => (cover.DisplayedDrawable as BeatmapSetCover)?.BeatmapSet == setInfo && (cover.DisplayedDrawable as BeatmapSetCover)?.CoverType == cover.Type);
            }
        }

        private class TestUpdateableBeatmapSetCover : UpdateableBeatmapSetCover
        {
            public readonly BeatmapSetCoverType Type;

            public new Drawable DisplayedDrawable => base.DisplayedDrawable as BeatmapSetCover;

            public TestUpdateableBeatmapSetCover(BeatmapSetCoverType type)
                : base(type)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Masking = true;

                switch (Type = type)
                {
                    case BeatmapSetCoverType.Cover:
                        Size = new Vector2(600, 300);
                        break;

                    case BeatmapSetCoverType.Card:
                        Size = new Vector2(400, 200);
                        break;

                    case BeatmapSetCoverType.List:
                        Size = new Vector2(150, 75);
                        break;
                }
            }
        }
    }
}
