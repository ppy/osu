// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseContextMenu : TestCase
    {
        public override string Description => @"Menu visible on right click";

        public override void Reset()
        {
            base.Reset();

            Add(new ContextMenuContainerOne
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

            Add(new ContextMenuContainerTwo
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
        }

        private class ContextMenuContainerOne : Container, IHasContextMenu
        {
            public ContextMenuItem[] Items => new ContextMenuItem[]
            {
                new ContextMenuItem(@"Some option"),
                new LinkableContextMenuItem(@"Linkable option"),
                new ContextMenuItem(@"Another option"),
                new ContextMenuItem(@"Choose me please"),
                new ContextMenuItem(@"And me too"),
                new ContextMenuItem(@"Trying to fill"),
                new DismissContextMenuItem(@"Dismiss option"),
            };
        }

        private class ContextMenuContainerTwo : Container, IHasContextMenu
        {
            public ContextMenuItem[] Items => new ContextMenuItem[]
            {
                new ContextMenuItem(@"Invite to"),
                new LinkableContextMenuItem(@"Linkable option"),
                new DismissContextMenuItem(@"Dismiss option"),
            };
        }
    }
}
