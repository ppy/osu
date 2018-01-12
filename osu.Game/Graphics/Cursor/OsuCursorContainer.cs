// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;

namespace osu.Game.Graphics.Cursor
{
    public class OsuCursorContainer : Container, IProvideCursor
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        /// <summary>
        /// Whether any cursors can be displayed.
        /// </summary>
        public bool CanShowCursor;

        public CursorContainer Cursor { get; }
        public bool ProvidesUserCursor => true;

        public OsuCursorContainer()
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

            var newTarget = inputManager.HoveredDrawables.OfType<IProvideCursor>().FirstOrDefault(t => t.ProvidesUserCursor) ?? this;

            if (currentTarget == newTarget)
                return;

            currentTarget?.Cursor?.Hide();
            newTarget.Cursor?.Show();

            currentTarget = newTarget;
        }
    }
}
