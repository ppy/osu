// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseContextMenu : TestCase
    {
        public override string Description => @"Menu visible on right click";

        private const int start_time = 0;
        private const int duration = 1000;

        private ContextMenuContainer container;

        public override void Reset()
        {
            base.Reset();

            Add(container = new ContextMenuContainer
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
                EndValue = new Vector2(0, -100),
                StartTime = start_time,
                EndTime = start_time + duration,
                Easing = EasingTypes.Out,
                LoopCount = -1,
                LoopDelay = duration
            });

            container.Transforms.Add(new TransformPosition
            {
                StartValue = new Vector2(0, -100),
                EndValue = Vector2.Zero,
                StartTime = start_time + duration,
                EndTime = start_time + duration * 2,
                Easing = EasingTypes.In,
                LoopCount = -1,
                LoopDelay = duration
            });
        }

        private class ContextMenuContainer : Container, IHasContextMenu
        {
            public ContextMenuItem[] ContextMenuItems => new []
            {
                new ContextMenuItem(@"Some option"),
                new ContextMenuItem(@"Linkable option", ContextMenuType.Linkable),
                new ContextMenuItem(@"Another option"),
                new ContextMenuItem(@"Choose me please"),
                new ContextMenuItem(@"And me too"),
                new ContextMenuItem(@"Trying to fill"),
                new ContextMenuItem(@"Dismiss option", ContextMenuType.Dismiss),
            };
        }

        private class AnotherContextMenuContainer : Container, IHasContextMenu
        {
            public ContextMenuItem[] ContextMenuItems => new []
            {
                new ContextMenuItem(@"Invite to"),
                new ContextMenuItem(@"Linkable option", ContextMenuType.Linkable),
                new ContextMenuItem(@"Dismiss option", ContextMenuType.Dismiss),
            };
        }
    }
}
