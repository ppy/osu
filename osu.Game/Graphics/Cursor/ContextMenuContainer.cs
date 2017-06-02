// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using System.Collections;
using System.Collections.Generic;

namespace osu.Game.Graphics.Cursor
{
    public class ContextMenuContainer : Container
    {
        private ContextMenu contextMenu;

        public IEnumerable<ContextMenuItem> Items
        {
            set
            {
                contextMenu.ItemsContainer.InternalChildren = value;

                foreach (var item in contextMenu.ItemsContainer.InternalChildren)
                    item.Action += () => Hide();
            }
        }

        public ContextMenuContainer()
        {
            Anchor = Anchor.TopLeft;
            Origin = Anchor.TopLeft;
            AlwaysPresent = true;
            AlwaysReceiveInput = true;
            AutoSizeAxes = Axes.Y;
            Width = 300;
            Children = new Drawable[]
            {
                contextMenu = new ContextMenu
                {
                    Alpha = 0
                }
            };
        }

        public new void Show() => contextMenu.State = MenuState.Opened;
        public new void Hide() => contextMenu.State = MenuState.Closed;
        public MenuState State => contextMenu.State;

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
