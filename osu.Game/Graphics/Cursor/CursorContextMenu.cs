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
using osu.Game.Graphics.UserInterface;
using System.Linq;

namespace osu.Game.Graphics.Cursor
{
    public class CursorContextMenu : Container
    {
        private readonly CursorContainer cursor;
        private readonly ContextMenuContainer menu;

        private UserInputManager inputManager;
        private IHasContextMenu menuTarget;
        private Vector2 relativeCursorPosition;

        public CursorContextMenu(CursorContainer cursor)
        {
            this.cursor = cursor;
            RelativeSizeAxes = Axes.Both;

            Add(menu = new ContextMenuContainer());
        }

        [BackgroundDependencyLoader]
        private void load(UserInputManager input)
        {
            inputManager = input;
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            switch (args.Button)
            {
                case MouseButton.Right:
                    menuTarget = inputManager.HoveredDrawables.OfType<IHasContextMenu>().FirstOrDefault();

                    if (menuTarget == null)
                        return false;

                    menu.Items = menuTarget.ContextMenuItems;

                    menu.Position = ToLocalSpace(cursor.ActiveCursor.ScreenSpaceDrawQuad.TopLeft);
                    relativeCursorPosition = ToSpaceOfOtherDrawable(menu.Position, menuTarget);
                    menu.Open();
                    break;
                default:
                    menu.Close();
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
