// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Collections
{
    public class TestSceneManageCollectionsDialog : OsuManualInputManagerTestScene
    {
        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        private DialogOverlay dialogOverlay;
        private CollectionManager manager;

        private RulesetStore rulesets;
        private BeatmapManager beatmapManager;

        private ManageCollectionsDialog dialog;

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, Audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(ContextFactory);

            beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();

            base.Content.AddRange(new Drawable[]
            {
                manager = new CollectionManager(LocalStorage),
                Content,
                dialogOverlay = new DialogOverlay(),
            });

            Dependencies.Cache(manager);
            Dependencies.Cache(dialogOverlay);
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            manager.Collections.Clear();
            Child = dialog = new ManageCollectionsDialog();
        });

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("show dialog", () => dialog.Show());
        }

        [Test]
        public void TestHideDialog()
        {
            AddWaitStep("wait for animation", 3);
            AddStep("hide dialog", () => dialog.Hide());
        }

        [Test]
        public void TestLastItemIsPlaceholder()
        {
            AddAssert("last item is placeholder", () => !manager.Collections.Contains(dialog.ChildrenOfType<DrawableCollectionListItem>().Last().Model));
        }

        [Test]
        public void TestAddCollectionExternal()
        {
            AddStep("add collection", () => manager.Collections.Add(new BeatmapCollection { Name = { Value = "First collection" } }));
            assertCollectionCount(1);
            assertCollectionName(0, "First collection");

            AddStep("add another collection", () => manager.Collections.Add(new BeatmapCollection { Name = { Value = "Second collection" } }));
            assertCollectionCount(2);
            assertCollectionName(1, "Second collection");
        }

        [Test]
        public void TestFocusPlaceholderDoesNotCreateCollection()
        {
            AddStep("focus placeholder", () =>
            {
                InputManager.MoveMouseTo(dialog.ChildrenOfType<DrawableCollectionListItem>().Last());
                InputManager.Click(MouseButton.Left);
            });

            assertCollectionCount(0);
        }

        [Test]
        public void TestAddCollectionViaPlaceholder()
        {
            DrawableCollectionListItem placeholderItem = null;

            AddStep("focus placeholder", () =>
            {
                InputManager.MoveMouseTo(placeholderItem = dialog.ChildrenOfType<DrawableCollectionListItem>().Last());
                InputManager.Click(MouseButton.Left);
            });

            // Done directly via the collection since InputManager methods cannot add text to textbox...
            AddStep("change collection name", () => placeholderItem.Model.Name.Value = "a");
            assertCollectionCount(1);
            AddAssert("collection now exists", () => manager.Collections.Contains(placeholderItem.Model));

            AddAssert("last item is placeholder", () => !manager.Collections.Contains(dialog.ChildrenOfType<DrawableCollectionListItem>().Last().Model));
        }

        [Test]
        public void TestRemoveCollectionExternal()
        {
            AddStep("add two collections", () => manager.Collections.AddRange(new[]
            {
                new BeatmapCollection { Name = { Value = "1" } },
                new BeatmapCollection { Name = { Value = "2" } },
            }));

            AddStep("remove first collection", () => manager.Collections.RemoveAt(0));
            assertCollectionCount(1);
            assertCollectionName(0, "2");
        }

        [Test]
        public void TestCollectionNameCollisions()
        {
            AddStep("add dropdown", () =>
            {
                Add(new CollectionFilterDropdown
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        RelativeSizeAxes = Axes.X,
                        Width = 0.4f,
                    }
                );
            });
            AddStep("add two collections with same name", () => manager.Collections.AddRange(new[]
            {
                new BeatmapCollection { Name = { Value = "1" } },
                new BeatmapCollection { Name = { Value = "1" }, Beatmaps = { beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps[0] } },
            }));
        }

        [Test]
        public void TestRemoveCollectionViaButton()
        {
            AddStep("add two collections", () => manager.Collections.AddRange(new[]
            {
                new BeatmapCollection { Name = { Value = "1" } },
                new BeatmapCollection { Name = { Value = "2" }, Beatmaps = { beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps[0] } },
            }));

            assertCollectionCount(2);

            AddStep("click first delete button", () =>
            {
                InputManager.MoveMouseTo(dialog.ChildrenOfType<DrawableCollectionListItem.DeleteButton>().First(), new Vector2(5, 0));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("dialog not displayed", () => dialogOverlay.CurrentDialog == null);
            assertCollectionCount(1);
            assertCollectionName(0, "2");

            AddStep("click first delete button", () =>
            {
                InputManager.MoveMouseTo(dialog.ChildrenOfType<DrawableCollectionListItem.DeleteButton>().First(), new Vector2(5, 0));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("dialog displayed", () => dialogOverlay.CurrentDialog is DeleteCollectionDialog);
            AddStep("click confirmation", () =>
            {
                InputManager.MoveMouseTo(dialogOverlay.CurrentDialog.ChildrenOfType<PopupDialogButton>().First());
                InputManager.Click(MouseButton.Left);
            });

            assertCollectionCount(0);
        }

        [Test]
        public void TestCollectionNotRemovedWhenDialogCancelled()
        {
            AddStep("add two collections", () => manager.Collections.AddRange(new[]
            {
                new BeatmapCollection { Name = { Value = "1" }, Beatmaps = { beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps[0] } },
            }));

            assertCollectionCount(1);

            AddStep("click first delete button", () =>
            {
                InputManager.MoveMouseTo(dialog.ChildrenOfType<DrawableCollectionListItem.DeleteButton>().First(), new Vector2(5, 0));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("dialog displayed", () => dialogOverlay.CurrentDialog is DeleteCollectionDialog);
            AddStep("click cancellation", () =>
            {
                InputManager.MoveMouseTo(dialogOverlay.CurrentDialog.ChildrenOfType<PopupDialogButton>().Last());
                InputManager.Click(MouseButton.Left);
            });

            assertCollectionCount(1);
        }

        [Test]
        public void TestCollectionRenamedExternal()
        {
            AddStep("add two collections", () => manager.Collections.AddRange(new[]
            {
                new BeatmapCollection { Name = { Value = "1" } },
                new BeatmapCollection { Name = { Value = "2" } },
            }));

            AddStep("change first collection name", () => manager.Collections[0].Name.Value = "First");

            assertCollectionName(0, "First");
        }

        [Test]
        public void TestCollectionRenamedOnTextChange()
        {
            AddStep("add two collections", () => manager.Collections.AddRange(new[]
            {
                new BeatmapCollection { Name = { Value = "1" } },
                new BeatmapCollection { Name = { Value = "2" } },
            }));

            assertCollectionCount(2);

            AddStep("change first collection name", () => dialog.ChildrenOfType<TextBox>().First().Text = "First");
            AddAssert("collection has new name", () => manager.Collections[0].Name.Value == "First");
        }

        private void assertCollectionCount(int count)
            => AddUntilStep($"{count} collections shown", () => dialog.ChildrenOfType<DrawableCollectionListItem>().Count(i => i.IsCreated.Value) == count);

        private void assertCollectionName(int index, string name)
            => AddUntilStep($"item {index + 1} has correct name", () => dialog.ChildrenOfType<DrawableCollectionListItem>().ElementAt(index).ChildrenOfType<TextBox>().First().Text == name);
    }
}
