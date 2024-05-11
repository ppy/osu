// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Overlays.Music;
using osu.Game.Rulesets;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestScenePlaylistOverlay : OsuManualInputManagerTestScene
    {
        protected override bool UseFreshStoragePerRun => true;

        private PlaylistOverlay playlistOverlay = null!;

        private BeatmapManager beatmapManager = null!;

        private const int item_count = 20;

        private List<BeatmapSetInfo> beatmapSets => beatmapManager.GetAllUsableBeatmapSets();

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, Realm, null, Audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(300, 500),
                Child = playlistOverlay = new PlaylistOverlay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    State = { Value = Visibility.Visible }
                }
            };

            for (int i = 0; i < item_count; i++)
            {
                beatmapManager.Import(TestResources.CreateTestBeatmapSetInfo());
            }

            beatmapSets.First().ToLive(Realm);

            // Ensure all the initial imports are present before running any tests.
            Realm.Run(r => r.Refresh());
        });

        [Test]
        public void TestRearrangeItems()
        {
            AddUntilStep("wait for load complete", () =>
            {
                return this
                       .ChildrenOfType<PlaylistItem>()
                       .Count(i => i.ChildrenOfType<DelayedLoadWrapper>().First().DelayedLoadCompleted) > 6;
            });

            AddUntilStep("wait for animations to complete", () => !playlistOverlay.Transforms.Any());

            PlaylistItem firstItem = null!;

            AddStep("hold 1st item handle", () =>
            {
                firstItem = this.ChildrenOfType<PlaylistItem>().First();
                var handle = firstItem.ChildrenOfType<PlaylistItem.PlaylistItemHandle>().First();

                InputManager.MoveMouseTo(handle.ScreenSpaceDrawQuad.Centre);
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("drag to 5th", () =>
            {
                var item = this.ChildrenOfType<PlaylistItem>().ElementAt(4);
                InputManager.MoveMouseTo(item.ScreenSpaceDrawQuad.BottomLeft);
            });

            AddAssert("first is moved", () => playlistOverlay.ChildrenOfType<Playlist>().Single().Items.ElementAt(4).Value.Equals(firstItem.Model.Value));

            AddStep("release handle", () => InputManager.ReleaseButton(MouseButton.Left));
        }

        [Test]
        public void TestFiltering()
        {
            AddStep("set filter to \"10\"", () =>
            {
                var filterControl = playlistOverlay.ChildrenOfType<FilterControl>().Single();
                filterControl.Search.Current.Value = "10";
            });

            AddAssert("results filtered correctly",
                () => playlistOverlay.ChildrenOfType<PlaylistItem>()
                                     .Where(item => item.MatchingFilter)
                                     .All(item => item.FilterTerms.Any(term => term.ToString().Contains("10"))));

            AddStep("Import new non-matching beatmap", () =>
            {
                var testBeatmapSetInfo = TestResources.CreateTestBeatmapSetInfo(1);
                testBeatmapSetInfo.Beatmaps.Single().Metadata.Title = "no guid";
                beatmapManager.Import(testBeatmapSetInfo);
            });

            AddStep("Force realm refresh", () => Realm.Run(r => r.Refresh()));

            AddAssert("results filtered correctly",
                () => playlistOverlay.ChildrenOfType<PlaylistItem>()
                                     .Where(item => item.MatchingFilter)
                                     .All(item => item.FilterTerms.Any(term => term.ToString().Contains("10"))));
        }

        [Test]
        public void TestCollectionFiltering()
        {
            NowPlayingCollectionDropdown collectionDropdown() => playlistOverlay.ChildrenOfType<NowPlayingCollectionDropdown>().Single();

            AddStep("Add collection", () =>
            {
                Realm.Write(r =>
                {
                    r.RemoveAll<BeatmapCollection>();
                    r.Add(new BeatmapCollection("wang"));
                });
            });

            AddUntilStep("wait for dropdown to have new collection", () => collectionDropdown().Items.Count() == 2);

            AddStep("Filter to collection", () =>
            {
                collectionDropdown().Current.Value = collectionDropdown().Items.Last();
            });

            AddUntilStep("No items present", () => !playlistOverlay.ChildrenOfType<PlaylistItem>().Any(i => i.MatchingFilter));

            AddStep("Import new non-matching beatmap", () =>
            {
                beatmapManager.Import(TestResources.CreateTestBeatmapSetInfo(1));
            });

            AddStep("Force realm refresh", () => Realm.Run(r => r.Refresh()));

            AddUntilStep("No items matching", () => !playlistOverlay.ChildrenOfType<PlaylistItem>().Any(i => i.MatchingFilter));

            BeatmapSetInfo collectionAddedBeatmapSet = null!;

            AddStep("Import new matching beatmap", () =>
            {
                collectionAddedBeatmapSet = TestResources.CreateTestBeatmapSetInfo(1);

                beatmapManager.Import(collectionAddedBeatmapSet);
                Realm.Write(r => r.All<BeatmapCollection>().First().BeatmapMD5Hashes.Add(collectionAddedBeatmapSet.Beatmaps.First().MD5Hash));
            });

            AddStep("Force realm refresh", () => Realm.Run(r => r.Refresh()));

            AddUntilStep("Only matching item",
                () => playlistOverlay.ChildrenOfType<PlaylistItem>().Where(i => i.MatchingFilter).Select(i => i.Model.ID), () => Is.EquivalentTo(new[] { collectionAddedBeatmapSet.ID }));
        }
    }
}
