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

            Add(new ContextMenuItem(@"test")
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            });
        }

        private class ContextMenuContainerOne : Container, IHasContextMenu
        {
            public ContextMenuItem[] Items => new ContextMenuItem[]
            {
                new ContextMenuItem(@"test1"),
                new ContextMenuItem(@"test2"),
                new ContextMenuItem(@"test3"),
            };
        }

        private class ContextMenuContainerTwo : Container, IHasContextMenu
        {
            public ContextMenuItem[] Items => new ContextMenuItem[]
            {
                new ContextMenuItem(@"test4"),
                new ContextMenuItem(@"test5"),
                new ContextMenuItem(@"test6"),
            };
        }
    }
}
