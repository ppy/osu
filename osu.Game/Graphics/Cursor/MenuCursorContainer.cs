// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Framework.Input.StateChanges;
using osu.Game.Configuration;

namespace osu.Game.Graphics.Cursor
{
    /// <summary>
    /// A container which provides a <see cref="MenuCursor"/>.
    /// It also handles cases where a more localised cursor is provided by another component (via <see cref="IProvideCursor"/>).
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

        private Bindable<bool> showDuringTouch;

        private InputManager inputManager;

        [Resolved]
        private OsuConfigManager config { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            inputManager = GetContainingInputManager();

            showDuringTouch = config.GetBindable<bool>(OsuSetting.GameplayCursorDuringTouch);
        }

        private IProvideCursor currentTarget;

        protected override void Update()
        {
            base.Update();

            var lastMouseSource = inputManager.CurrentState.Mouse.LastSource;
            bool hasValidInput = lastMouseSource != null && (showDuringTouch.Value || lastMouseSource is not ISourcedFromTouch);

            if (!hasValidInput || !CanShowCursor)
            {
                currentTarget?.Cursor?.Hide();
                currentTarget = null;
                return;
            }

            IProvideCursor newTarget = this;

            foreach (var d in inputManager.HoveredDrawables)
            {
                if (d is IProvideCursor p && p.ProvidingUserCursor)
                {
                    newTarget = p;
                    break;
                }
            }

            if (currentTarget == newTarget)
                return;

            currentTarget?.Cursor?.Hide();
            newTarget.Cursor?.Show();

            currentTarget = newTarget;
        }
    }
}
