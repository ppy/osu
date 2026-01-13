// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Screens.SelectV2;
using osuTK.Input;
using Realms;
using CollectionDropdown = osu.Game.Screens.SelectV2.CollectionDropdown;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapFilterControl : SongSelectComponentsTestScene
    {
        private FilterControl filterControl = null!;

        protected override Anchor ComponentAnchor => Anchor.TopRight;
        protected override float InitialRelativeWidth => 0.7f;

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            Dependencies.Cache(Realm);
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = filterControl = new FilterControl
                {
                    State = { Value = Visibility.Visible },
                    RelativeSizeAxes = Axes.X,
                },
            };
        });

        [Test]
        public void TestSearch()
        {
            AddStep("search for text", () => filterControl.Search("test search"));
        }

        [Test]
        public void TestUpdateToIncludeBeatmapIfNotInCollection()
        {
            var beatmap = new BeatmapInfo();

            AddStep("remove all collections", () => writeAndRefresh(r => r.RemoveAll<BeatmapCollection>()));

            AddStep("add collections", () =>
            {
                writeAndRefresh(r => r.Add(new BeatmapCollection(name: "1")));
                writeAndRefresh(r => r.Add(new BeatmapCollection(name: "2", [beatmap.MD5Hash])));
            });

            void testForCollection(string name)
            {
                AddStep("expand header", () =>
                {
                    InputManager.MoveMouseTo(filterControl.ChildrenOfType<CollectionDropdown.ShearedDropdownHeader>().Single());
                    InputManager.Click(MouseButton.Left);
                });

                AddStep($"select collection {name}", () =>
                {
                    InputManager.MoveMouseTo(getCollectionDropdownItemFor(name));
                    InputManager.Click(MouseButton.Left);
                });

                AddAssert($"collection {name} selected", () => filterControl.ChildrenOfType<CollectionDropdown>().Single().Current.Value.CollectionName == name);

                AddStep("update filter to include beatmap", () =>
                {
                    filterControl.UpdateToInclude(beatmap);
                });
            }

            testForCollection("1");
            AddAssert("all beatmaps selected", () => filterControl.ChildrenOfType<CollectionDropdown>().Single().Current.Value is AllBeatmapsCollectionFilterMenuItem);

            testForCollection("2");
            AddAssert("collection 2 selected", () => filterControl.ChildrenOfType<CollectionDropdown>().Single().Current.Value.CollectionName == "2");
        }

        [Test]
        public void TestUpdateToIncludeBeatmapIfNotInDifficultyRange()
        {
            var beatmap = new BeatmapInfo
            {
                StarRating = 3.141592
            };

            void testForDifficultyRange(double min, double max)
            {
                AddStep($"set difficulty range min to {min}", () =>
                {
                    filterControl.ChildrenOfType<FilterControl.DifficultyRangeSlider>().Single().LowerBound.Value = min;
                });

                AddStep($"set difficulty range max to {max}", () =>
                {
                    filterControl.ChildrenOfType<FilterControl.DifficultyRangeSlider>().Single().UpperBound.Value = max;
                });

                AddStep("update filter to include beatmap", () =>
                {
                    filterControl.UpdateToInclude(beatmap);
                });
            }

            testForDifficultyRange(5, 10.1);
            AddAssert("beatmap star rating in range", () =>
            {
                var slider = filterControl.ChildrenOfType<FilterControl.DifficultyRangeSlider>().Single();
                return slider.LowerBound.Value <= beatmap.StarRating && slider.UpperBound.Value >= beatmap.StarRating;
            });

            testForDifficultyRange(0, 2);
            AddAssert("beatmap star rating in range", () =>
            {
                var slider = filterControl.ChildrenOfType<FilterControl.DifficultyRangeSlider>().Single();
                return slider.LowerBound.Value <= beatmap.StarRating && slider.UpperBound.Value >= beatmap.StarRating;
            });

            testForDifficultyRange(1, 8);
            AddAssert("difficulty range didn't update", () =>
            {
                var slider = filterControl.ChildrenOfType<FilterControl.DifficultyRangeSlider>().Single();
                return slider.LowerBound.Value == 1 && slider.UpperBound.Value == 8;
            });

            testForDifficultyRange(0, 10.1);
            AddAssert("difficulty range didn't update", () =>
            {
                var slider = filterControl.ChildrenOfType<FilterControl.DifficultyRangeSlider>().Single();
                return slider.LowerBound.Value == 0 && slider.UpperBound.Value == 10.1;
            });
        }

        private void writeAndRefresh(Action<Realm> action) => Realm.Write(r =>
        {
            action(r);
            r.Refresh();
        });

        private Menu.DrawableMenuItem getCollectionDropdownItemFor(string name)
        {
            return filterControl.ChildrenOfType<Menu.DrawableMenuItem>().Single(i => i.Item.Text.Value == name);
        }
    }
}
