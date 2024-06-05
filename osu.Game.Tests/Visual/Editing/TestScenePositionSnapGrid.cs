// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestScenePositionSnapGrid : OsuManualInputManagerTestScene
    {
        private Container content;
        protected override Container<Drawable> Content => content;

        [BackgroundDependencyLoader]
        private void load()
        {
            base.Content.AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Gray
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(10),
                }
            });
        }

        private static readonly object[][] test_cases =
        {
            new object[] { new Vector2(0, 0), new Vector2(10, 10), 0f },
            new object[] { new Vector2(240, 180), new Vector2(10, 15), 10f },
            new object[] { new Vector2(160, 120), new Vector2(30, 20), -10f },
            new object[] { new Vector2(480, 360), new Vector2(100, 100), 0f },
        };

        [TestCaseSource(nameof(test_cases))]
        public void TestRectangularGrid(Vector2 position, Vector2 spacing, float rotation)
        {
            RectangularPositionSnapGrid grid = null;

            AddStep("create grid", () =>
            {
                Child = grid = new RectangularPositionSnapGrid
                {
                    RelativeSizeAxes = Axes.Both,
                };
                grid.StartPosition.Value = position;
                grid.Spacing.Value = spacing;
                grid.GridLineRotation.Value = rotation;
            });

            AddStep("add snapping cursor", () => Add(new SnappingCursorContainer
            {
                RelativeSizeAxes = Axes.Both,
                GetSnapPosition = pos => grid.GetSnappedPosition(grid.ToLocalSpace(pos))
            }));
        }

        private partial class SnappingCursorContainer : CompositeDrawable
        {
            public Func<Vector2, Vector2> GetSnapPosition;

            private readonly Drawable cursor;

            public SnappingCursorContainer()
            {
                RelativeSizeAxes = Axes.Both;

                InternalChild = cursor = new Circle
                {
                    Origin = Anchor.Centre,
                    Size = new Vector2(50),
                    Colour = Color4.Red
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updatePosition(GetContainingInputManager().CurrentState.Mouse.Position);
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                base.OnMouseMove(e);

                updatePosition(e.ScreenSpaceMousePosition);
                return true;
            }

            private void updatePosition(Vector2 screenSpacePosition)
            {
                cursor.Position = GetSnapPosition.Invoke(screenSpacePosition);
            }
        }
    }
}
