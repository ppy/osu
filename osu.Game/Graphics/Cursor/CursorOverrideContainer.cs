// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
    public class CursorOverrideContainer : Container, IProvideCursor
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        /// <summary>
        /// Whether any cursors can be displayed.
        /// </summary>
        internal bool CanShowCursor = true;

        public CursorContainer Cursor { get; }
        public bool ProvidingUserCursor => true;

        public CursorOverrideContainer()
        {
            AddRangeInternal(new Drawable[]
            {
                Cursor = new MenuCursor { State = Visibility.Hidden },
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
