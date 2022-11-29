// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    /// A container which provides the main <see cref="MenuCursorContainer"/>.
    /// Also handles cases where a more localised cursor is provided by another component (via <see cref="IProvideCursor"/>).
    /// </summary>
    public partial class GlobalCursorDisplay : Container, IProvideCursor
    {
        /// <summary>
        /// Control whether any cursor should be displayed.
        /// </summary>
        internal bool ShowCursor = true;

        CursorContainer IProvideCursor.Cursor => MenuCursor;

        public MenuCursorContainer MenuCursor { get; }

        public bool ProvidingUserCursor => true;

        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        private Bindable<bool> showDuringTouch = null!;

        private InputManager inputManager = null!;

        private IProvideCursor? currentOverrideProvider;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        public GlobalCursorDisplay()
        {
            AddRangeInternal(new Drawable[]
            {
                Content = new Container { RelativeSizeAxes = Axes.Both },
                MenuCursor = new MenuCursorContainer { State = { Value = Visibility.Hidden } }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
            showDuringTouch = config.GetBindable<bool>(OsuSetting.GameplayCursorDuringTouch);
        }

        protected override void Update()
        {
            base.Update();

            var lastMouseSource = inputManager.CurrentState.Mouse.LastSource;
            bool hasValidInput = lastMouseSource != null && (showDuringTouch.Value || lastMouseSource is not ISourcedFromTouch);

            if (!hasValidInput || !ShowCursor)
            {
                currentOverrideProvider?.Cursor?.Hide();
                currentOverrideProvider = null;
                return;
            }

            IProvideCursor newOverrideProvider = this;

            foreach (var d in inputManager.HoveredDrawables)
            {
                if (d is IProvideCursor p && p.ProvidingUserCursor)
                {
                    newOverrideProvider = p;
                    break;
                }
            }

            if (currentOverrideProvider == newOverrideProvider)
                return;

            currentOverrideProvider?.Cursor?.Hide();
            newOverrideProvider.Cursor?.Show();

            currentOverrideProvider = newOverrideProvider;
        }
    }
}
