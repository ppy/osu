// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Skinning;
using Realms;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public partial class TestSceneSkinDropdown : OsuManualInputManagerTestScene
    {
        private OverlayColourProvider colourProvider { get; set; } = new OverlayColourProvider(OverlayColourScheme.Purple);

        private SkinDropdown skinDropdown;

        private IDisposable realmSubscription;

        [Resolved]
        private SkinManager skins { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; }

        protected override bool UseFreshStoragePerRun => true;

        private readonly List<Live<SkinInfo>> dropdownItems = new List<Live<SkinInfo>>();

        private static readonly SkinInfo protected_skin_1 = new SkinInfo { Protected = true, ID = SkinInfo.ARGON_SKIN, Name = "Protected skin 1", };
        private static readonly SkinInfo protected_skin_2 = new SkinInfo { Protected = true, ID = SkinInfo.ARGON_PRO_SKIN, Name = "Protected skin 2", };

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset storage", () =>
            {
                realmSubscription?.Dispose();

                realm.Write(r =>
                {
                    r.RemoveAll<SkinInfo>();

                    var imported = new[]
                    {
                        new SkinInfo { ID = protected_skin_1.ID, Name = protected_skin_1.Name, Protected = true },
                        new SkinInfo { ID = protected_skin_2.ID, Name = protected_skin_2.Name, Protected = true },
                        new SkinInfo { ID = Guid.NewGuid(), Name = "Imported Skin 1" },
                        new SkinInfo { ID = Guid.NewGuid(), Name = "Imported Skin 2" },
                        new SkinInfo { ID = Guid.NewGuid(), Name = "Imported Skin 3" },
                        new SkinInfo { ID = Guid.NewGuid(), Name = "Imported Skin 4" }
                    };

                    foreach (var skin in imported)
                        r.Add(skin);
                });
            });

            AddStep("create dropdown", () =>
            {
                skinDropdown?.Expire();

                Add(new DependencyProvidingContainer
                    {
                        CachedDependencies =
                        [
                            (typeof(OverlayColourProvider), colourProvider),
                        ],
                        RelativeSizeAxes = Axes.X,
                        Padding = new MarginPadding { Left = 300, Right = 300 },
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children =
                        [
                            new SettingsItemV2(skinDropdown = new SkinDropdown
                            {
                                AlwaysShowSearchBar = true,
                                AllowNonContiguousMatching = true,
                                Caption = SkinSettingsStrings.CurrentSkin,
                                Current = skins.CurrentSkinInfo,
                            })
                        ]
                    }
                );
            });

            AddStep("load skins", loadSkins);
        }

        [Test]
        public void TestToggleFavourite()
        {
            toggleFavourite();
            AddWaitStep("wait", 5);
            toggleFavourite();
        }

        [Test]
        public void TestRightClickToggleFavourite()
        {
            rightClickToggleFavourite();
            AddWaitStep("wait", 5);
            rightClickToggleFavourite();
        }

        [Test]
        public void TestTouchToggleFavourite()
        {
            touchToggleFavourite();
            AddWaitStep("wait", 5);
            touchToggleFavourite();
        }

        [Test]
        public void TestTouchHoldToggleFavourite()
        {
            touchHoldToggleFavourite();
            AddWaitStep("wait", 5);
            touchHoldToggleFavourite();
        }

        private void toggleFavourite()
        {
            IHasText label = null;
            bool? isFavourite = null;

            AddStep("open dropdown", () =>
            {
                var menu = skinDropdown.ChildrenOfType<Menu>().FirstOrDefault();

                if (menu?.State != MenuState.Closed) return;

                InputManager.MoveMouseTo(skinDropdown);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("dropdown opened", () =>
            {
                var menu = skinDropdown.ChildrenOfType<Menu>().FirstOrDefault();
                return menu != null && menu.State == MenuState.Open;
            });
            AddStep("move over imported skin 4", () =>
            {
                isFavourite = getStarIcon();
                label = skinDropdown.ChildrenOfType<IHasText>()
                                    .FirstOrDefault(d => d.Text.ToString() == "Imported Skin 4");
                Assert.NotNull(label);
                Assert.NotNull(label.Parent);
                InputManager.MoveMouseTo(label.Parent);
            });
            AddStep("hover over star", () =>
            {
                Assert.NotNull(label.Parent);
                var menuItem = label.Parent.Parent;

                var starIcon = menuItem.ChildrenOfType<SpriteIcon>()
                                       .First(icon => icon.Icon.Equals(FontAwesome.Solid.Star) || icon.Icon.Equals(FontAwesome.Regular.Star));
                InputManager.MoveMouseTo(starIcon, new Vector2(10, 4));
            });
            AddStep("toggle favourite status", () =>
            {
                isFavourite = getStarIcon();
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("check favourite status", () => getStarIcon() != isFavourite);
        }

        private void rightClickToggleFavourite()
        {
            bool? isFavourite = null;

            AddStep("open dropdown", () =>
            {
                var menu = skinDropdown.ChildrenOfType<Menu>().FirstOrDefault();

                if (menu?.State == MenuState.Closed)
                {
                    InputManager.MoveMouseTo(skinDropdown);
                    InputManager.Click(MouseButton.Left);
                }
            });
            AddAssert("dropdown opened", () =>
            {
                var menu = skinDropdown.ChildrenOfType<Menu>().FirstOrDefault();
                return menu != null && menu.State == MenuState.Open;
            });
            AddStep("move over imported skin 4", () =>
            {
                var label = skinDropdown.ChildrenOfType<IHasText>().First(d => d.Text.ToString() == "Imported Skin 4");
                Assert.NotNull(label.Parent);
                InputManager.MoveMouseTo(label.Parent);
            });
            AddStep("toggle favourite status", () =>
            {
                isFavourite = getStarIcon();
                InputManager.Click(MouseButton.Right);
            });
            AddAssert("check favourite status", () => getStarIcon() != isFavourite);
        }

        private void touchToggleFavourite()
        {
            IHasText label = null;
            bool? isFavourite = null;
            int touchDragPosition = 100;

            AddStep("open dropdown", () =>
            {
                var menu = skinDropdown.ChildrenOfType<Menu>().FirstOrDefault();

                if (menu?.State == MenuState.Closed)
                {
                    var position = skinDropdown.ToScreenSpace(new Vector2(200, 50));

                    InputManager.BeginTouch(new Touch(TouchSource.Touch1, position));
                    InputManager.EndTouch(new Touch(TouchSource.Touch1, position));
                }
            });
            AddAssert("dropdown opened", () =>
            {
                var menu = skinDropdown.ChildrenOfType<Menu>().FirstOrDefault();
                return menu != null && menu.State == MenuState.Open;
            });
            AddStep("begin favourite drag", () =>
            {
                isFavourite = getStarIcon();
                label = skinDropdown.ChildrenOfType<TruncatingSpriteText>()
                                    .FirstOrDefault(d => d.Text.ToString() == "Imported Skin 4");
                Assert.NotNull(label);
                Assert.NotNull(label.Parent);
                var startPos = label.Parent.ToScreenSpace(new Vector2(touchDragPosition, 10));
                InputManager.BeginTouch(new Touch(TouchSource.Touch1, startPos));
            });

            AddUntilStep("drag right", () =>
            {
                touchDragPosition += 10;

                label = skinDropdown.ChildrenOfType<TruncatingSpriteText>()
                                    .FirstOrDefault(d => d.Text.ToString() == "Imported Skin 4");
                Assert.NotNull(label);
                Assert.NotNull(label.Parent);
                var position = label.Parent.ToScreenSpace(new Vector2(touchDragPosition, 10));

                InputManager.MoveTouchTo(new Touch(TouchSource.Touch1, position));
                return getStarIcon() != isFavourite;
            });
            AddStep("end drag", () =>
            {
                isFavourite = getStarIcon();
                InputManager.EndTouch(new Touch(TouchSource.Touch1, Vector2.Zero));
            });
            AddAssert("check favourite status", () => getStarIcon() == isFavourite);
        }

        private void touchHoldToggleFavourite()
        {
            IHasText label;
            bool? isFavourite = null;

            AddStep("open dropdown", () =>
            {
                var menu = skinDropdown.ChildrenOfType<Menu>().FirstOrDefault();

                if (menu?.State == MenuState.Closed)
                {
                    var position = skinDropdown.ToScreenSpace(new Vector2(200, 50));

                    InputManager.BeginTouch(new Touch(TouchSource.Touch1, position));
                    InputManager.EndTouch(new Touch(TouchSource.Touch1, position));
                }
            });
            AddAssert("dropdown opened", () =>
            {
                var menu = skinDropdown.ChildrenOfType<Menu>().FirstOrDefault();
                return menu != null && menu.State == MenuState.Open;
            });
            AddStep("begin favourite hold", () =>
            {
                isFavourite = getStarIcon();
                label = skinDropdown.ChildrenOfType<IHasText>()
                                    .FirstOrDefault(d => d.Text.ToString() == "Imported Skin 4");
                Assert.NotNull(label);
                Assert.NotNull(label.Parent);
                var startPos = label.Parent.ToScreenSpace(new Vector2(100, 10));
                InputManager.BeginTouch(new Touch(TouchSource.Touch1, startPos));
            });
            AddUntilStep("check favourite status", () => getStarIcon() != isFavourite);
            AddStep("end hold", () => InputManager.EndTouch(new Touch(TouchSource.Touch1, Vector2.Zero)));
        }

        private void loadSkins()
        {
            realmSubscription = realm.RegisterForNotifications(_ => realm.Realm.All<SkinInfo>()
                                                                         .Where(s => !s.DeletePending)
                                                                         .OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase), skinsChanged);
        }

        private void skinsChanged(IRealmCollection<SkinInfo> sender, ChangeSet changes)
        {
            // This can only mean that realm is recycling, else we would see the protected skins.
            // Because we are using `Live<>` in this class, we don't need to worry about this scenario too much.
            if (!sender.Any())
                return;

            // For simplicity repopulate the full list.
            // In the future we should change this to properly handle ChangeSet events.
            dropdownItems.Clear();
            dropdownItems.AddRange(GetAllUsableSkins());

            Schedule(() => skinDropdown.Items = dropdownItems);
        }

        public IList<Live<SkinInfo>> GetAllUsableSkins()
        {
            var usableSkins = new List<Live<SkinInfo>>();

            realm.Run(r =>
            {
                usableSkins.Add(r.Find<SkinInfo>(protected_skin_1.ID).ToLive(realm));
                usableSkins.Add(r.Find<SkinInfo>(protected_skin_2.ID).ToLive(realm));

                var userSkins = r.All<SkinInfo>()
                                 .Where(s => !s.DeletePending && !s.Protected)
                                 .OrderByDescending(s => s.Favourite)
                                 .ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
                                 .AsEnumerable()
                                 .Select(s => s.ToLive(realm));

                foreach (var s in userSkins.Where(s => !s.Value.Protected))
                    usableSkins.Add(s);
            });

            return usableSkins;
        }

        private bool getStarIcon()
        {
            var label = skinDropdown.ChildrenOfType<IHasText>().FirstOrDefault(d => d.Text.ToString() == "Imported Skin 4");
            Assert.NotNull(label);
            Assert.NotNull(label.Parent);
            var menuItem = label.Parent.Parent;
            var starIcon = menuItem.ChildrenOfType<SpriteIcon>()
                                   .First(icon => icon.Icon.Equals(FontAwesome.Solid.Star) || icon.Icon.Equals(FontAwesome.Regular.Star));
            return starIcon.Icon.Equals(FontAwesome.Solid.Star);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            realmSubscription?.Dispose();
        }
    }
}
