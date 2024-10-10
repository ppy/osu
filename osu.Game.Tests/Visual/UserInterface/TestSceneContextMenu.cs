// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneContextMenu : OsuTestScene
    {
        private const int start_time = 0;
        private const int duration = 1000;

        private readonly Container container;

        public TestSceneContextMenu()
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

        private static MenuItem[] makeMenu()
        {
            return new MenuItem[]
            {
                new OsuMenuItem(@"Some option"),
                new OsuMenuItem(@"Highlighted option", MenuItemType.Highlighted),
                new OsuMenuItem(@"Another option"),
                new OsuMenuItem(@"Nested option")
                {
                    Items = new MenuItem[]
                    {
                        new OsuMenuItem(@"Sub-One"),
                        new OsuMenuItem(@"Sub-Two"),
                        new OsuMenuItem(@"Sub-Three"),
                        new OsuMenuItem(@"Sub-Nested option")
                        {
                            Items = new MenuItem[]
                            {
                                new OsuMenuItem(@"Double Sub-One"),
                                new OsuMenuItem(@"Double Sub-Two"),
                                new OsuMenuItem(@"Double Sub-Three"),
                                new OsuMenuItem(@"Sub-Sub-Nested option")
                                {
                                    Items = new MenuItem[]
                                    {
                                        new OsuMenuItem(@"Too Deep One"),
                                        new OsuMenuItem(@"Too Deep Two"),
                                        new OsuMenuItem(@"Too Deep Three"),
                                    }
                                }
                            }
                        }
                    }
                },
                new OsuMenuItem(@"Choose me please"),
                new OsuMenuItem(@"And me too"),
                new OsuMenuItem(@"Trying to fill"),
                new OsuMenuItem(@"Destructive option", MenuItemType.Destructive),
            };
        }

        private partial class MyContextMenuContainer : Container, IHasContextMenu
        {
            public MenuItem[] ContextMenuItems => makeMenu();
        }

        private partial class AnotherContextMenuContainer : Container, IHasContextMenu
        {
            public MenuItem[] ContextMenuItems
            {
                get
                {
                    List<MenuItem> items = makeMenu().ToList();
                    items.AddRange(new MenuItem[]
                    {
                        new OsuMenuItem(@"Change width", MenuItemType.Highlighted, () => this.ResizeWidthTo(Width * 2, 100, Easing.OutQuint)),
                        new OsuMenuItem(@"Change height", MenuItemType.Highlighted, () => this.ResizeHeightTo(Height * 2, 100, Easing.OutQuint)),
                        new OsuMenuItem(@"Change width back", MenuItemType.Destructive, () => this.ResizeWidthTo(Width / 2, 100, Easing.OutQuint)),
                        new OsuMenuItem(@"Change height back", MenuItemType.Destructive, () => this.ResizeHeightTo(Height / 2, 100, Easing.OutQuint)),
                    });

                    return items.ToArray();
                }
            }
        }
    }
}
