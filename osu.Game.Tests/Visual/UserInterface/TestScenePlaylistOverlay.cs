// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Music;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestScenePlaylistOverlay : OsuManualInputManagerTestScene
    {
        private readonly BindableList<BeatmapSetInfo> beatmapSets = new BindableList<BeatmapSetInfo>();

        private PlaylistOverlay playlistOverlay;

        private BeatmapSetInfo first;

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

            beatmapSets.Clear();

            for (int i = 0; i < 100; i++)
            {
                beatmapSets.Add(TestResources.CreateTestBeatmapSetInfo());
            }

            first = beatmapSets.First();

            playlistOverlay.BeatmapSets.BindTo(beatmapSets);
        });

        [Test]
        public void TestRearrangeItems()
        {
            AddUntilStep("wait for animations to complete", () => !playlistOverlay.Transforms.Any());

            AddStep("hold 1st item handle", () =>
            {
                var handle = this.ChildrenOfType<OsuRearrangeableListItem<BeatmapSetInfo>.PlaylistItemHandle>().First();
                InputManager.MoveMouseTo(handle.ScreenSpaceDrawQuad.Centre);
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("drag to 5th", () =>
            {
                var item = this.ChildrenOfType<PlaylistItem>().ElementAt(4);
                InputManager.MoveMouseTo(item.ScreenSpaceDrawQuad.Centre);
            });

            AddAssert("song 1 is 5th", () => beatmapSets[4].Equals(first));

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
                                     .All(item => item.FilterTerms.Any(term => term.Contains("10"))));
        }
    }
}
