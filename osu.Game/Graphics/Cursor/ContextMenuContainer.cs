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
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics.UserInterface;
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
            if (menu.IsPresent && menuTarget != null)
                menu.Position = new Vector2(-ToSpaceOfOtherDrawable(-relativeCursorPosition, menuTarget).X, -ToSpaceOfOtherDrawable(-relativeCursorPosition, menuTarget).Y);
            base.Update();
        }

        private class ContextMenu : Menu
        {
            private const int margin_vertical = ContextMenuItem.MARGIN_VERTICAL;
            private const int fade_duration = 250;

            public ContextMenu()
            {
                CornerRadius = 5;
                ItemsContainer.Padding = new MarginPadding { Vertical = margin_vertical };
                Masking = true;
                EdgeEffect = new EdgeEffect
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.25f),
                    Radius = 4,
                };

                Background.Colour = OsuColour.FromHex(@"223034");
            }

            protected override void AnimateOpen() => FadeIn(fade_duration, EasingTypes.OutQuint);

            protected override void AnimateClose() => FadeOut(fade_duration, EasingTypes.OutQuint);

            protected override void UpdateContentHeight()
            {
                var actualHeight = (RelativeSizeAxes & Axes.Y) > 0 ? 1 : ContentHeight;
                ResizeTo(new Vector2(1, State == MenuState.Opened ? actualHeight : 0), fade_duration, EasingTypes.OutQuint);
            }
        }
    }
}
