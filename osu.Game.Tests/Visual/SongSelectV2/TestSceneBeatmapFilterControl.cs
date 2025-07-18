// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapFilterControl : SongSelectComponentsTestScene
    {
        private FilterControl filterControl = null!;

        protected override Anchor ComponentAnchor => Anchor.TopRight;
        protected override float InitialRelativeWidth => 0.7f;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            BeatmapCollectionStore collectionStore;

            Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(BeatmapCollectionStore), collectionStore = new BeatmapCollectionStore()),
                },
                Children = new Drawable[]
                {
                    collectionStore,
                    filterControl = new FilterControl
                    {
                        State = { Value = Visibility.Visible },
                        RelativeSizeAxes = Axes.X,
                    }
                },
            };
        });

        [Test]
        public void TestSearch()
        {
            AddStep("search for text", () => filterControl.Search("test search"));
        }
    }
}
