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
    public partial class TestSceneManageCollectionsDialog : OsuManualInputManagerTestScene
    {
        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        private DialogOverlay dialogOverlay = null!;
        private BeatmapManager beatmapManager = null!;
        private ManageCollectionsDialog dialog = null!;

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, Realm, null, Audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);

            beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();

            base.Content.AddRange(new Drawable[]
            {
                Content,
                dialogOverlay = new DialogOverlay(),
            });

            Dependencies.CacheAs<IDialogOverlay>(dialogOverlay);
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Realm.Write(r => r.RemoveAll<BeatmapCollection>());
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
            AddAssert("last item is placeholder", () => !dialog.ChildrenOfType<DrawableCollectionListItem>().Last().Model.IsManaged);
        }

        [Test]
        public void TestAddCollectionExternal()
        {
            AddStep("add collection", () => Realm.Write(r => r.Add(new BeatmapCollection(name: "First collection"))));
            assertCollectionCount(1);
            assertCollectionName(0, "First collection");

            AddStep("add another collection", () => Realm.Write(r => r.Add(new BeatmapCollection(name: "Second collection"))));
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
            DrawableCollectionListItem placeholderItem = null!;

            AddStep("focus placeholder", () =>
            {
                InputManager.MoveMouseTo(placeholderItem = dialog.ChildrenOfType<DrawableCollectionListItem>().Last());
                InputManager.Click(MouseButton.Left);
            });

            assertCollectionCount(0);

            AddStep("change collection name", () =>
            {
                placeholderItem.ChildrenOfType<TextBox>().First().Text = "test text";
                InputManager.Key(Key.Enter);
            });

            assertCollectionCount(1);

            AddAssert("last item is placeholder", () => !dialog.ChildrenOfType<DrawableCollectionListItem>().Last().Model.IsManaged);
        }

        [Test]
        public void TestRemoveCollectionExternal()
        {
            BeatmapCollection first = null!;

            AddStep("add two collections", () =>
            {
                Realm.Write(r =>
                {
                    r.Add(new[]
                    {
                        first = new BeatmapCollection(name: "1"),
                        new BeatmapCollection(name: "2"),
                    });
                });
            });

            AddStep("remove first collection", () => Realm.Write(r => r.Remove(first)));
            assertCollectionCount(1);
            assertCollectionName(0, "2");
        }

        [Test]
        public void TestCollectionNameCollisions()
        {
            AddStep("add dropdown", () =>
            {
                Add(new CollectionDropdown
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.4f,
                });
            });
            AddStep("add two collections with same name", () => Realm.Write(r => r.Add(new[]
            {
                new BeatmapCollection(name: "1"),
                new BeatmapCollection(name: "1")
                {
                    BeatmapMD5Hashes = { beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps[0].MD5Hash }
                },
            })));
        }

        [Test]
        public void TestCollectionNameCollisionsWithBuiltInItems()
        {
            AddStep("add dropdown", () =>
            {
                Add(new CollectionDropdown
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.4f,
                });
            });
            AddStep("add two collections which collide with default items", () => Realm.Write(r => r.Add(new[]
            {
                new BeatmapCollection(name: "All beatmaps"),
                new BeatmapCollection(name: "Manage collections...")
                {
                    BeatmapMD5Hashes = { beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps[0].MD5Hash }
                },
            })));
        }

        [Test]
        public void TestRemoveCollectionViaButton()
        {
            AddStep("add two collections", () => Realm.Write(r => r.Add(new[]
            {
                new BeatmapCollection(name: "1"),
                new BeatmapCollection(name: "2")
                {
                    BeatmapMD5Hashes = { beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps[0].MD5Hash }
                },
            })));

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
                InputManager.PressButton(MouseButton.Left);
            });

            assertCollectionCount(0);

            AddStep("release mouse button", () => InputManager.ReleaseButton(MouseButton.Left));
        }

        [Test]
        public void TestCollectionNotRemovedWhenDialogCancelled()
        {
            AddStep("add collection", () => Realm.Write(r => r.Add(new[]
            {
                new BeatmapCollection(name: "1")
                {
                    BeatmapMD5Hashes = { beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps[0].MD5Hash }
                },
            })));

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
            BeatmapCollection first = null!;

            AddStep("add two collections", () =>
            {
                Realm.Write(r =>
                {
                    r.Add(new[]
                    {
                        first = new BeatmapCollection(name: "1"),
                        new BeatmapCollection(name: "2"),
                    });
                });
            });

            assertCollectionName(0, "1");
            assertCollectionName(1, "2");

            AddStep("change first collection name", () => Realm.Write(_ => first.Name = "First"));

            // Item will have moved due to alphabetical sorting.
            assertCollectionName(0, "2");
            assertCollectionName(1, "First");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestCollectionRenamedOnTextChange(bool commitWithEnter)
        {
            BeatmapCollection first = null!;
            DrawableCollectionListItem firstItem = null!;

            AddStep("add two collections", () =>
            {
                Realm.Write(r =>
                {
                    r.Add(new[]
                    {
                        first = new BeatmapCollection(name: "1"),
                        new BeatmapCollection(name: "2"),
                    });
                });
            });

            assertCollectionCount(2);

            AddStep("focus first collection", () =>
            {
                InputManager.MoveMouseTo(firstItem = dialog.ChildrenOfType<DrawableCollectionListItem>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddStep("change first collection name", () =>
            {
                firstItem.ChildrenOfType<TextBox>().First().Text = "First";
            });

            if (commitWithEnter)
                AddStep("commit via enter", () => InputManager.Key(Key.Enter));
            else
            {
                AddStep("commit via click away", () =>
                {
                    InputManager.MoveMouseTo(firstItem.ScreenSpaceDrawQuad.TopLeft - new Vector2(10));
                    InputManager.Click(MouseButton.Left);
                });
            }

            AddUntilStep("collection has new name", () => first.Name == "First");
        }

        private void assertCollectionCount(int count)
            => AddUntilStep($"{count} collections shown", () => dialog.ChildrenOfType<DrawableCollectionListItem>().Count() == count + 1); // +1 for placeholder

        private void assertCollectionName(int index, string name)
            => AddUntilStep($"item {index + 1} has correct name", () => dialog.ChildrenOfType<DrawableCollectionListItem>().ElementAt(index).ChildrenOfType<TextBox>().First().Text == name);
    }
}
