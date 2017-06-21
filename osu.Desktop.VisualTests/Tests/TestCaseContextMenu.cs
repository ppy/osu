// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseContextMenu : TestCase
    {
        public override string Description => @"Menu visible on right click";

        private const int start_time = 0;
        private const int duration = 1000;

        private MyContextMenuContainer container;

        public override void Reset()
        {
            base.Reset();

            Add(container = new MyContextMenuContainer
            {
                Size = new Vector2(200),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Green,
                    }
                }
            });

            Add(new AnotherContextMenuContainer
            {
                Size = new Vector2(200),
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Red,
                    }
                }
            });

            container.Transforms.Add(new TransformPosition
            {
                StartValue = Vector2.Zero,
                EndValue = new Vector2(0, 100),
                StartTime = start_time,
                EndTime = start_time + duration,
                LoopCount = -1,
                LoopDelay = duration * 3
            });
            container.Transforms.Add(new TransformPosition
            {
                StartValue = new Vector2(0, 100),
                EndValue = new Vector2(100, 100),
                StartTime = start_time + duration,
                EndTime = start_time + duration * 2,
                LoopCount = -1,
                LoopDelay = duration * 3
            });
            container.Transforms.Add(new TransformPosition
            {
                StartValue = new Vector2(100, 100),
                EndValue = new Vector2(100, 0),
                StartTime = start_time + duration * 2,
                EndTime = start_time + duration * 3,
                LoopCount = -1,
                LoopDelay = duration * 3
            });
            container.Transforms.Add(new TransformPosition
            {
                StartValue = new Vector2(100, 0),
                EndValue = Vector2.Zero,
                StartTime = start_time + duration * 3,
                EndTime = start_time + duration * 4,
                LoopCount = -1,
                LoopDelay = duration * 3
            });
        }

        private class MyContextMenuContainer : Container, IHasContextMenu
        {
            public ContextMenuItem[] ContextMenuItems => new ContextMenuItem[]
            {
                new OsuContextMenuItem(@"Some option"),
                new OsuContextMenuItem(@"Highlighted option", MenuItemType.Highlighted),
                new OsuContextMenuItem(@"Another option"),
                new OsuContextMenuItem(@"Choose me please"),
                new OsuContextMenuItem(@"And me too"),
                new OsuContextMenuItem(@"Trying to fill"),
                new OsuContextMenuItem(@"Destructive option", MenuItemType.Destructive),
            };
        }

        private class AnotherContextMenuContainer : Container, IHasContextMenu
        {
            public ContextMenuItem[] ContextMenuItems => new ContextMenuItem[]
            {
                new OsuContextMenuItem(@"Simple option"),
                new OsuContextMenuItem(@"Simple very very long option"),
                new OsuContextMenuItem(@"Change width", MenuItemType.Highlighted) { Action = () => ResizeWidthTo(Width * 2, 100, EasingTypes.OutQuint) },
                new OsuContextMenuItem(@"Change height", MenuItemType.Highlighted) { Action = () => ResizeHeightTo(Height * 2, 100, EasingTypes.OutQuint) },
                new OsuContextMenuItem(@"Change width back", MenuItemType.Destructive) { Action = () => ResizeWidthTo(Width / 2, 100, EasingTypes.OutQuint) },
                new OsuContextMenuItem(@"Change height back", MenuItemType.Destructive) { Action = () => ResizeHeightTo(Height / 2, 100, EasingTypes.OutQuint) },
            };
        }
    }
}
