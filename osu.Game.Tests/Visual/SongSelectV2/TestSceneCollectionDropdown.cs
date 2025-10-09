// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Tests.Resources;
using osuTK.Input;
using Realms;
using CollectionDropdown = osu.Game.Screens.SelectV2.CollectionDropdown;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneCollectionDropdown : OsuManualInputManagerTestScene
    {
        private RulesetStore rulesets = null!;
        private BeatmapManager beatmapManager = null!;
        private CollectionDropdown dropdown = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, Realm, null, Audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);

            beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            writeAndRefresh(r => r.RemoveAll<BeatmapCollection>());

            Child = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = dropdown = new CollectionDropdown
                {
                    Width = 300,
                    Y = 100,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                },
            };
        });

        [Test]
        public void TestEmptyCollectionFilterContainsAllBeatmaps()
        {
            assertCollectionDropdownContains(CollectionsStrings.AllBeatmaps);
            assertCollectionHeaderDisplays(CollectionsStrings.AllBeatmaps);
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

            AddUntilStep("check count 5", () => dropdown.ChildrenOfType<CollectionDropdown>().Single().ChildrenOfType<Menu.DrawableMenuItem>().Count(), () => Is.EqualTo(5));

            AddStep("delete all collections", () => writeAndRefresh(r => r.RemoveAll<BeatmapCollection>()));

            AddUntilStep("check count 2", () => dropdown.ChildrenOfType<CollectionDropdown>().Single().ChildrenOfType<Menu.DrawableMenuItem>().Count(), () => Is.EqualTo(2));
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
            AddStep("select collection", () => dropdown.Current.Value = dropdown.ItemSource.ElementAt(1));

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
            AddUntilStep("collection contains beatmap", () => getFirstCollection().BeatmapMD5Hashes.Contains(Beatmap.Value.BeatmapInfo.MD5Hash));
            assertFirstButtonIs(FontAwesome.Solid.MinusSquare);

            addClickAddOrRemoveButtonStep(1);
            AddUntilStep("collection does not contain beatmap", () => !getFirstCollection().BeatmapMD5Hashes.Contains(Beatmap.Value.BeatmapInfo.MD5Hash));
            assertFirstButtonIs(FontAwesome.Solid.PlusSquare);
        }

        [Test]
        public void TestManageCollectionsFilterIsNotSelected()
        {
            addExpandHeaderStep();

            AddStep("add collection", () => writeAndRefresh(r => r.Add(new BeatmapCollection(name: "1", new List<string> { "abc" }))));
            assertCollectionDropdownContains("1");

            AddStep("select collection", () =>
            {
                InputManager.MoveMouseTo(getCollectionDropdownItemAt(1));
                InputManager.Click(MouseButton.Left);
            });

            addExpandHeaderStep();

            AddStep("click manage collections filter", () =>
            {
                int lastItemIndex = dropdown.ChildrenOfType<CollectionDropdown>().Single().Items.Count() - 1;
                InputManager.MoveMouseTo(getCollectionDropdownItemAt(lastItemIndex));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("collection filter still selected", () => dropdown.Current.Value.CollectionName == "1");
        }

        private void writeAndRefresh(Action<Realm> action) => Realm.Write(r =>
        {
            action(r);
            r.Refresh();
        });

        private BeatmapCollection getFirstCollection() => Realm.Run(r => r.All<BeatmapCollection>().First());

        private void assertCollectionHeaderDisplays(LocalisableString collectionName, bool shouldDisplay = true)
            => AddUntilStep($"collection dropdown header displays '{collectionName}'",
                () => shouldDisplay == dropdown.ChildrenOfType<CollectionDropdown.ShearedDropdownHeader>().Any(h => h.ChildrenOfType<SpriteText>().Any(t => t.Text == collectionName)));

        private void assertFirstButtonIs(IconUsage icon) => AddUntilStep($"button is {icon.Icon.ToString()}", () => getAddOrRemoveButton(1).Icon.Equals(icon));

        private void assertCollectionDropdownContains(LocalisableString collectionName, bool shouldContain = true) =>
            AddUntilStep($"collection dropdown {(shouldContain ? "contains" : "does not contain")} '{collectionName}'",
                // A bit of a roundabout way of going about this, see: https://github.com/ppy/osu-framework/issues/3871 + https://github.com/ppy/osu-framework/issues/3872
                () => shouldContain == dropdown.ChildrenOfType<Menu.DrawableMenuItem>().Any(i => i.ChildrenOfType<CompositeDrawable>().OfType<IHasText>().First().Text == collectionName));

        private IconButton getAddOrRemoveButton(int index)
            => getCollectionDropdownItemAt(index).ChildrenOfType<IconButton>().Single();

        private void addExpandHeaderStep() => AddStep("expand header", () =>
        {
            InputManager.MoveMouseTo(dropdown.ChildrenOfType<CollectionDropdown.ShearedDropdownHeader>().Single());
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
            CollectionFilterMenuItem item = dropdown.ChildrenOfType<CollectionDropdown>().Single().ItemSource.ElementAt(index);
            return dropdown.ChildrenOfType<Menu.DrawableMenuItem>().Single(i => i.Item.Text.Value == item.CollectionName);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (rulesets.IsNotNull())
                rulesets.Dispose();
        }
    }
}
