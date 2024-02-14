// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osu.Game.Tests.Resources;
using osuTK.Input;
using Realms;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneFilterControl : OsuManualInputManagerTestScene
    {
        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        private BeatmapManager beatmapManager = null!;
        private FilterControl control = null!;

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, Realm, null, Audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);

            beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();

            base.Content.AddRange(new Drawable[]
            {
                Content
            });
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            writeAndRefresh(r => r.RemoveAll<BeatmapCollection>());

            Child = control = new FilterControl
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Height = FilterControl.HEIGHT,
            };
        });

        [Test]
        public void TestEmptyCollectionFilterContainsAllBeatmaps()
        {
            assertCollectionDropdownContains("All beatmaps");
            assertCollectionHeaderDisplays("All beatmaps");
        }

        [Test]
        public void TestCollectionAddedToDropdown()
        {
            AddStep("add collection", () => writeAndRefresh(r => r.Add(new BeatmapCollection(name: "1"))));
            AddStep("add collection", () => writeAndRefresh(r => r.Add(new BeatmapCollection(name: "2"))));
            assertCollectionDropdownContains("1");
            assertCollectionDropdownContains("2");
        }

        [Test]
        public void TestCollectionsCleared()
        {
            AddStep("add collection", () => writeAndRefresh(r => r.Add(new BeatmapCollection(name: "1"))));
            AddStep("add collection", () => writeAndRefresh(r => r.Add(new BeatmapCollection(name: "2"))));
            AddStep("add collection", () => writeAndRefresh(r => r.Add(new BeatmapCollection(name: "3"))));

            AddAssert("check count 5", () => control.ChildrenOfType<CollectionDropdown>().Single().ChildrenOfType<Menu.DrawableMenuItem>().Count(), () => Is.EqualTo(5));

            AddStep("delete all collections", () => writeAndRefresh(r => r.RemoveAll<BeatmapCollection>()));

            AddAssert("check count 2", () => control.ChildrenOfType<CollectionDropdown>().Single().ChildrenOfType<Menu.DrawableMenuItem>().Count(), () => Is.EqualTo(2));
        }

        [Test]
        public void TestCollectionRemovedFromDropdown()
        {
            BeatmapCollection first = null!;

            AddStep("add collection", () => writeAndRefresh(r => r.Add(first = new BeatmapCollection(name: "1"))));
            AddStep("add collection", () => writeAndRefresh(r => r.Add(new BeatmapCollection(name: "2"))));
            AddStep("remove collection", () => writeAndRefresh(r => r.Remove(first)));

            assertCollectionDropdownContains("1", false);
            assertCollectionDropdownContains("2");
        }

        [Test]
        public void TestCollectionRenamed()
        {
            AddStep("add collection", () => writeAndRefresh(r => r.Add(new BeatmapCollection(name: "1"))));
            assertCollectionDropdownContains("1");
            AddStep("select collection", () =>
            {
                var dropdown = control.ChildrenOfType<CollectionDropdown>().Single();
                dropdown.Current.Value = dropdown.ItemSource.ElementAt(1);
            });

            addExpandHeaderStep();

            AddStep("change name", () => writeAndRefresh(_ => getFirstCollection().Name = "First"));

            assertCollectionDropdownContains("First");
            assertCollectionHeaderDisplays("First");
        }

        [Test]
        public void TestAllBeatmapFilterDoesNotHaveAddButton()
        {
            addExpandHeaderStep();
            AddStep("hover all beatmaps", () => InputManager.MoveMouseTo(getAddOrRemoveButton(0)));
            AddAssert("'All beatmaps' filter does not have add button", () => !getAddOrRemoveButton(0).IsPresent);
        }

        [Test]
        public void TestCollectionFilterHasAddButton()
        {
            addExpandHeaderStep();
            AddStep("add collection", () => writeAndRefresh(r => r.Add(new BeatmapCollection(name: "1"))));
            assertCollectionDropdownContains("1");
            AddStep("hover collection", () => InputManager.MoveMouseTo(getAddOrRemoveButton(1)));
            AddAssert("collection has add button", () => getAddOrRemoveButton(1).IsPresent);
        }

        [Test]
        public void TestButtonDisabledAndEnabledWithBeatmapChanges()
        {
            addExpandHeaderStep();

            AddStep("add collection", () => writeAndRefresh(r => r.Add(new BeatmapCollection(name: "1"))));
            assertCollectionDropdownContains("1");

            AddStep("select available beatmap", () => Beatmap.Value = beatmapManager.GetWorkingBeatmap(beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps[0]));
            AddAssert("button enabled", () => getAddOrRemoveButton(1).Enabled.Value);

            AddStep("set dummy beatmap", () => Beatmap.SetDefault());
            AddAssert("button disabled", () => !getAddOrRemoveButton(1).Enabled.Value);
        }

        [Test]
        public void TestButtonChangesWhenAddedAndRemovedFromCollection()
        {
            addExpandHeaderStep();

            AddStep("select available beatmap", () => Beatmap.Value = beatmapManager.GetWorkingBeatmap(beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps[0]));

            AddStep("add collection", () => writeAndRefresh(r => r.Add(new BeatmapCollection(name: "1"))));
            assertCollectionDropdownContains("1");

            assertFirstButtonIs(FontAwesome.Solid.PlusSquare);

            AddStep("add beatmap to collection", () => writeAndRefresh(r => getFirstCollection().BeatmapMD5Hashes.Add(Beatmap.Value.BeatmapInfo.MD5Hash)));
            assertFirstButtonIs(FontAwesome.Solid.MinusSquare);

            AddStep("remove beatmap from collection", () => writeAndRefresh(r => getFirstCollection().BeatmapMD5Hashes.Clear()));
            assertFirstButtonIs(FontAwesome.Solid.PlusSquare);
        }

        [Test]
        public void TestButtonAddsAndRemovesBeatmap()
        {
            addExpandHeaderStep();

            AddStep("select available beatmap", () => Beatmap.Value = beatmapManager.GetWorkingBeatmap(beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps[0]));

            AddStep("add collection", () => writeAndRefresh(r => r.Add(new BeatmapCollection(name: "1"))));
            assertCollectionDropdownContains("1");
            assertFirstButtonIs(FontAwesome.Solid.PlusSquare);

            addClickAddOrRemoveButtonStep(1);
            AddAssert("collection contains beatmap", () => getFirstCollection().BeatmapMD5Hashes.Contains(Beatmap.Value.BeatmapInfo.MD5Hash));
            assertFirstButtonIs(FontAwesome.Solid.MinusSquare);

            addClickAddOrRemoveButtonStep(1);
            AddAssert("collection does not contain beatmap", () => !getFirstCollection().BeatmapMD5Hashes.Contains(Beatmap.Value.BeatmapInfo.MD5Hash));
            assertFirstButtonIs(FontAwesome.Solid.PlusSquare);
        }

        [Test]
        public void TestManageCollectionsFilterIsNotSelected()
        {
            bool received = false;

            addExpandHeaderStep();

            AddStep("add collection", () => writeAndRefresh(r => r.Add(new BeatmapCollection(name: "1", new List<string> { "abc" }))));
            assertCollectionDropdownContains("1");

            AddStep("select collection", () =>
            {
                InputManager.MoveMouseTo(getCollectionDropdownItemAt(1));
                InputManager.Click(MouseButton.Left);
            });

            addExpandHeaderStep();

            AddStep("watch for filter requests", () =>
            {
                received = false;
                control.ChildrenOfType<CollectionDropdown>().First().RequestFilter = () => received = true;
            });

            AddStep("click manage collections filter", () =>
            {
                int lastItemIndex = control.ChildrenOfType<CollectionDropdown>().Single().Items.Count() - 1;
                InputManager.MoveMouseTo(getCollectionDropdownItemAt(lastItemIndex));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("collection filter still selected", () => control.CreateCriteria().CollectionBeatmapMD5Hashes?.Any() == true);

            AddAssert("filter request not fired", () => !received);
        }

        private void writeAndRefresh(Action<Realm> action) => Realm.Write(r =>
        {
            action(r);
            r.Refresh();
        });

        private BeatmapCollection getFirstCollection() => Realm.Run(r => r.All<BeatmapCollection>().First());

        private void assertCollectionHeaderDisplays(string collectionName, bool shouldDisplay = true)
            => AddUntilStep($"collection dropdown header displays '{collectionName}'",
                () => shouldDisplay == (control.ChildrenOfType<CollectionDropdown.CollectionDropdownHeader>().Single().ChildrenOfType<SpriteText>().First().Text == collectionName));

        private void assertFirstButtonIs(IconUsage icon) => AddUntilStep($"button is {icon.Icon.ToString()}", () => getAddOrRemoveButton(1).Icon.Equals(icon));

        private void assertCollectionDropdownContains(string collectionName, bool shouldContain = true) =>
            AddUntilStep($"collection dropdown {(shouldContain ? "contains" : "does not contain")} '{collectionName}'",
                // A bit of a roundabout way of going about this, see: https://github.com/ppy/osu-framework/issues/3871 + https://github.com/ppy/osu-framework/issues/3872
                () => shouldContain == control.ChildrenOfType<Menu.DrawableMenuItem>().Any(i => i.ChildrenOfType<CompositeDrawable>().OfType<IHasText>().First().Text == collectionName));

        private IconButton getAddOrRemoveButton(int index)
            => getCollectionDropdownItemAt(index).ChildrenOfType<IconButton>().Single();

        private void addExpandHeaderStep() => AddStep("expand header", () =>
        {
            InputManager.MoveMouseTo(control.ChildrenOfType<CollectionDropdown.CollectionDropdownHeader>().Single());
            InputManager.Click(MouseButton.Left);
        });

        private void addClickAddOrRemoveButtonStep(int index) => AddStep("click add or remove button", () =>
        {
            InputManager.MoveMouseTo(getAddOrRemoveButton(index));
            InputManager.Click(MouseButton.Left);
        });

        private Menu.DrawableMenuItem getCollectionDropdownItemAt(int index)
        {
            // todo: we should be able to use Items, but apparently that's not guaranteed to be ordered... see: https://github.com/ppy/osu-framework/pull/6079
            CollectionFilterMenuItem item = control.ChildrenOfType<CollectionDropdown>().Single().ItemSource.ElementAt(index);
            return control.ChildrenOfType<Menu.DrawableMenuItem>().Single(i => i.Item.Text.Value == item.CollectionName);
        }
    }
}
