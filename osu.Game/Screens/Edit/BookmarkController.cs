// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Components.Menus;

namespace osu.Game.Screens.Edit
{
    public partial class BookmarkController : Component, IKeyBindingHandler<GlobalAction>
    {
        /// <summary>
        /// Bookmarks menu item (with submenu containing options). Should be added to the <see cref="Editor"/>'s global menu.
        /// </summary>
        public EditorMenuItem Menu { get; private set; }

        [Resolved]
        private EditorClock clock { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        private readonly BindableList<int> bookmarks = new BindableList<int>();

        private readonly EditorMenuItem removeBookmarkMenuItem;
        private readonly EditorMenuItem seekToPreviousBookmarkMenuItem;
        private readonly EditorMenuItem seekToNextBookmarkMenuItem;
        private readonly EditorMenuItem resetBookmarkMenuItem;

        public BookmarkController()
        {
            Menu = new EditorMenuItem(EditorStrings.Bookmarks)
            {
                Items = new MenuItem[]
                {
                    new EditorMenuItem(EditorStrings.AddBookmark, MenuItemType.Standard, addBookmarkAtCurrentTime)
                    {
                        Hotkey = new Hotkey(GlobalAction.EditorAddBookmark),
                    },
                    removeBookmarkMenuItem = new EditorMenuItem(EditorStrings.RemoveClosestBookmark, MenuItemType.Destructive, removeClosestBookmark)
                    {
                        Hotkey = new Hotkey(GlobalAction.EditorRemoveClosestBookmark)
                    },
                    seekToPreviousBookmarkMenuItem = new EditorMenuItem(EditorStrings.SeekToPreviousBookmark, MenuItemType.Standard, () => seekBookmark(-1))
                    {
                        Hotkey = new Hotkey(GlobalAction.EditorSeekToPreviousBookmark)
                    },
                    seekToNextBookmarkMenuItem = new EditorMenuItem(EditorStrings.SeekToNextBookmark, MenuItemType.Standard, () => seekBookmark(1))
                    {
                        Hotkey = new Hotkey(GlobalAction.EditorSeekToNextBookmark)
                    },
                    resetBookmarkMenuItem = new EditorMenuItem(EditorStrings.ResetBookmarks, MenuItemType.Destructive, () => dialogOverlay?.Push(new BookmarkResetDialog(editorBeatmap)))
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            bookmarks.BindTo(editorBeatmap.Bookmarks);
        }

        protected override void Update()
        {
            base.Update();

            bool hasAnyBookmark = bookmarks.Count > 0;
            bool hasBookmarkCloseEnoughForDeletion = bookmarks.Any(b => Math.Abs(b - clock.CurrentTimeAccurate) < 2000);

            removeBookmarkMenuItem.Action.Disabled = !hasBookmarkCloseEnoughForDeletion;
            seekToPreviousBookmarkMenuItem.Action.Disabled = !hasAnyBookmark;
            seekToNextBookmarkMenuItem.Action.Disabled = !hasAnyBookmark;
            resetBookmarkMenuItem.Action.Disabled = !hasAnyBookmark;
        }

        private void addBookmarkAtCurrentTime()
        {
            int bookmark = (int)clock.CurrentTimeAccurate;
            int idx = bookmarks.BinarySearch(bookmark);
            if (idx < 0)
                bookmarks.Insert(~idx, bookmark);
        }

        private void removeClosestBookmark()
        {
            if (removeBookmarkMenuItem.Action.Disabled)
                return;

            int closestBookmark = bookmarks.MinBy(b => Math.Abs(b - clock.CurrentTimeAccurate));
            bookmarks.Remove(closestBookmark);
        }

        private void seekBookmark(int direction)
        {
            int? targetBookmark = direction < 1
                ? bookmarks.Cast<int?>().LastOrDefault(b => b < clock.CurrentTimeAccurate)
                : bookmarks.Cast<int?>().FirstOrDefault(b => b > clock.CurrentTimeAccurate);

            if (targetBookmark != null)
                clock.SeekSmoothlyTo(targetBookmark.Value);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.EditorSeekToPreviousBookmark:
                    seekBookmark(-1);
                    return true;

                case GlobalAction.EditorSeekToNextBookmark:
                    seekBookmark(1);
                    return true;
            }

            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.EditorAddBookmark:
                    addBookmarkAtCurrentTime();
                    return true;

                case GlobalAction.EditorRemoveClosestBookmark:
                    removeClosestBookmark();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
