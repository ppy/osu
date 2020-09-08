// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Collections;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Collections
{
    public class TestSceneManageCollectionsDialog : OsuManualInputManagerTestScene
    {
        protected override Container<Drawable> Content => content;

        private readonly Container content;
        private readonly DialogOverlay dialogOverlay;
        private readonly BeatmapCollectionManager manager;

        private ManageCollectionsDialog dialog;

        public TestSceneManageCollectionsDialog()
        {
            base.Content.AddRange(new Drawable[]
            {
                manager = new BeatmapCollectionManager(LocalStorage),
                content = new Container { RelativeSizeAxes = Axes.Both },
                dialogOverlay = new DialogOverlay()
            });
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.Cache(manager);
            dependencies.Cache(dialogOverlay);
            return dependencies;
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
        public void TestAddCollectionViaButton()
        {
            AddStep("press new collection button", () =>
            {
                InputManager.MoveMouseTo(dialog.ChildrenOfType<OsuButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            assertCollectionCount(1);

            AddStep("press again", () =>
            {
                InputManager.MoveMouseTo(dialog.ChildrenOfType<OsuButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            assertCollectionCount(2);
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
        public void TestRemoveCollectionViaButton()
        {
            AddStep("add two collections", () => manager.Collections.AddRange(new[]
            {
                new BeatmapCollection { Name = { Value = "1" } },
                new BeatmapCollection { Name = { Value = "2" } },
            }));

            assertCollectionCount(2);

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

            assertCollectionCount(1);
            assertCollectionName(0, "2");
        }

        [Test]
        public void TestCollectionNotRemovedWhenDialogCancelled()
        {
            AddStep("add two collections", () => manager.Collections.AddRange(new[]
            {
                new BeatmapCollection { Name = { Value = "1" } },
                new BeatmapCollection { Name = { Value = "2" } },
            }));

            assertCollectionCount(2);

            AddStep("click first delete button", () =>
            {
                InputManager.MoveMouseTo(dialog.ChildrenOfType<DrawableCollectionListItem.DeleteButton>().First(), new Vector2(5, 0));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("dialog displayed", () => dialogOverlay.CurrentDialog is DeleteCollectionDialog);
            AddStep("click confirmation", () =>
            {
                InputManager.MoveMouseTo(dialogOverlay.CurrentDialog.ChildrenOfType<PopupDialogButton>().Last());
                InputManager.Click(MouseButton.Left);
            });

            assertCollectionCount(2);
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
            => AddUntilStep($"{count} collections shown", () => dialog.ChildrenOfType<DrawableCollectionListItem>().Count() == count);

        private void assertCollectionName(int index, string name)
            => AddUntilStep($"item {index + 1} has correct name", () => dialog.ChildrenOfType<DrawableCollectionListItem>().ElementAt(index).ChildrenOfType<TextBox>().First().Text == name);
    }
}
