// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterface;
using System.Linq;

namespace osu.Game.Graphics.Cursor
{
    public class ContextMenuContainer : Container
    {
        private readonly CursorContainer cursor;
        private readonly ContextMenu menu;

        private ScheduledDelegate findMenuTask;
        private UserInputManager inputManager;

        private const int default_appear_delay = 220;

        private IHasContextMenu currentlyDisplayed;

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

        protected override bool OnHover(InputState state)
        {
            if (menu.IsPresent)
            {
                if (currentlyDisplayed != null)
                {
                    menu.Items = currentlyDisplayed.Items;
                }
            }
            return base.OnHover(state);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            updateMenuState(state);
            return base.OnMouseUp(state, args);
        }

        protected override bool OnMouseMove(InputState state)
        {
            updateMenuState(state);
            return base.OnMouseMove(state);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            switch (args.Button)
            {
                case MouseButton.Right:
                    menu.Position = ToLocalSpace(cursor.ActiveCursor.ScreenSpaceDrawQuad.TopLeft);
                    menu.Show();
                    return true;
            }
            menu.Hide();

            return base.OnMouseDown(state, args);
        }

        private void updateMenuState(InputState state)
        {
            if (currentlyDisplayed?.Hovering != true)
            {
                if (currentlyDisplayed != null && !state.Mouse.HasMainButtonPressed)
                {
                    menu.Delay(150);
                    menu.FadeOut(500, EasingTypes.OutQuint);
                    currentlyDisplayed = null;
                }

                findMenuTask?.Cancel();
                findMenuTask = Scheduler.AddDelayed(delegate
                {
                    var menuTarget = inputManager.HoveredDrawables.OfType<IHasContextMenu>().FirstOrDefault();

                    if (menuTarget == null) return;

                    menu.Items = menuTarget.Items;
                    menu.FadeIn(500, EasingTypes.OutQuint);

                    currentlyDisplayed = menuTarget;
                }, (1 - menu.Alpha) * default_appear_delay);
            }
        }

        public class ContextMenu : FillFlowContainer
        {
            private ContextMenuItem[] items;
            public ContextMenuItem[] Items
            {
                set
                {
                    if(items != null)
                    {
                        foreach (var item in items)
                            Remove(item);
                    }

                    items = value;

                    foreach (var item in value)
                        Add(item);
                }
            }

            public ContextMenu()
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Vertical;

                CornerRadius = 5;
                Masking = true;
                EdgeEffect = new EdgeEffect
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(40),
                    Radius = 5,
                };
            }
        }
    }
}
