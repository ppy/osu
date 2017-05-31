// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using System.Linq;

namespace osu.Game.Graphics.Cursor
{
    public class ContextMenuContainer : Container
    {
        private readonly CursorContainer cursor;
        private readonly ContextMenu menu;

        private UserInputManager inputManager;
        private IHasContextMenu menuTarget;
        private Vector2 relativeCursorPosition;

        public ContextMenuContainer(CursorContainer cursor)
        {
            this.cursor = cursor;
            AlwaysPresent = true;
            RelativeSizeAxes = Axes.Both;
            Add(menu = new ContextMenu { Alpha = 0 });
        }

        [BackgroundDependencyLoader]
        private void load(UserInputManager input)
        {
            inputManager = input;
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            menu.State = MenuState.Closed;

            switch (args.Button)
            {
                case MouseButton.Right:
                    menuTarget = inputManager.HoveredDrawables.OfType<IHasContextMenu>().FirstOrDefault();

                    if (menuTarget == null)
                        return false;

                    menu.ItemsContainer.InternalChildren = menuTarget.ContextMenuItems;
                    foreach (var item in menu.ItemsContainer.InternalChildren)
                        item.Action += () => menu.State = MenuState.Closed;

                    menu.Position = ToLocalSpace(cursor.ActiveCursor.ScreenSpaceDrawQuad.TopLeft);
                    relativeCursorPosition = ToSpaceOfOtherDrawable(menu.Position, menuTarget);
                    menu.Toggle();
                    break;
            }

            return true;
        }

        protected override void Update()
        {
            if (menu.State == MenuState.Opened && menuTarget != null)
                menu.Position = new Vector2(-ToSpaceOfOtherDrawable(-relativeCursorPosition, menuTarget).X, -ToSpaceOfOtherDrawable(-relativeCursorPosition, menuTarget).Y);
            base.Update();
        }
    }
}
