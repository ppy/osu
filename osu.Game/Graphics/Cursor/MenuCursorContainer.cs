// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;

namespace osu.Game.Graphics.Cursor
{
    /// <summary>
    /// A container which provides a <see cref="MenuCursor"/> which can be overridden by hovered <see cref="Drawable"/>s.
    /// </summary>
    public class MenuCursorContainer : Container, IProvideCursor
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        /// <summary>
        /// Whether any cursors can be displayed.
        /// </summary>
        internal bool CanShowCursor = true;

        public CursorContainer Cursor { get; }
        public bool ProvidingUserCursor => true;

        public MenuCursorContainer()
        {
            AddRangeInternal(new Drawable[]
            {
                Cursor = new MenuCursor { State = { Value = Visibility.Hidden } },
                content = new Container { RelativeSizeAxes = Axes.Both }
            });
        }

        private InputManager inputManager;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            inputManager = GetContainingInputManager();
        }

        private IProvideCursor currentTarget;

        protected override void Update()
        {
            base.Update();

            if (!CanShowCursor)
            {
                currentTarget?.Cursor?.Hide();
                currentTarget = null;
                return;
            }

            var newTarget = inputManager.HoveredDrawables.OfType<IProvideCursor>().FirstOrDefault(t => t.ProvidingUserCursor) ?? this;

            if (currentTarget == newTarget)
                return;

            currentTarget?.Cursor?.Hide();
            newTarget.Cursor?.Show();

            currentTarget = newTarget;
        }
    }
}
