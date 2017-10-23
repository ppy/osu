﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics.Cursor;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseContextMenu : OsuTestCase
    {
        public override string Description => @"Menu visible on right click";

        private const int start_time = 0;
        private const int duration = 1000;

        private readonly Container container;

        public TestCaseContextMenu()
        {
            Add(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    container = new MyContextMenuContainer
                    {
                        Size = new Vector2(200),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Green,
                        }
                    },
                    new AnotherContextMenuContainer
                    {
                        Size = new Vector2(200),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Red,
                        }
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Move box along a square trajectory
            container.Loop(c => c
                .MoveTo(new Vector2(0, 100), duration).Then()
                .MoveTo(new Vector2(100, 100), duration).Then()
                .MoveTo(new Vector2(100, 0), duration).Then()
                .MoveTo(Vector2.Zero, duration)
            );
        }

        private class MyContextMenuContainer : Container, IHasContextMenu
        {
            public MenuItem[] ContextMenuItems => new MenuItem[]
            {
                new OsuMenuItem(@"Some option"),
                new OsuMenuItem(@"Highlighted option", MenuItemType.Highlighted),
                new OsuMenuItem(@"Another option"),
                new OsuMenuItem(@"Choose me please"),
                new OsuMenuItem(@"And me too"),
                new OsuMenuItem(@"Trying to fill"),
                new OsuMenuItem(@"Destructive option", MenuItemType.Destructive),
            };
        }

        private class AnotherContextMenuContainer : Container, IHasContextMenu
        {
            public MenuItem[] ContextMenuItems => new MenuItem[]
            {
                new OsuMenuItem(@"Simple option"),
                new OsuMenuItem(@"Simple very very long option"),
                new OsuMenuItem(@"Change width", MenuItemType.Highlighted, () => this.ResizeWidthTo(Width * 2, 100, Easing.OutQuint)),
                new OsuMenuItem(@"Change height", MenuItemType.Highlighted, () => this.ResizeHeightTo(Height * 2, 100, Easing.OutQuint)),
                new OsuMenuItem(@"Change width back", MenuItemType.Destructive, () => this.ResizeWidthTo(Width / 2, 100, Easing.OutQuint)),
                new OsuMenuItem(@"Change height back", MenuItemType.Destructive, () => this.ResizeHeightTo(Height / 2, 100, Easing.OutQuint)),
            };
        }
    }
}
