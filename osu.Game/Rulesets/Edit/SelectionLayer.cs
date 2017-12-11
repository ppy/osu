// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using System.Linq;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Rulesets.Edit
{
    public class SelectionLayer : CompositeDrawable
    {
        private readonly Playfield playfield;

        public SelectionLayer(Playfield playfield)
        {
            this.playfield = playfield;

            RelativeSizeAxes = Axes.Both;
        }

        private DragContainer dragBox;

        protected override bool OnDragStart(InputState state)
        {
            dragBox?.Hide();
            AddInternal(dragBox = new DragContainer(ToLocalSpace(state.Mouse.NativeState.Position))
            {
                CapturableObjects = playfield.HitObjects.Objects
            });

            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            dragBox.Track(ToLocalSpace(state.Mouse.NativeState.Position));
            dragBox.UpdateCapture();
            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            if (dragBox.CapturedHitObjects.Count == 0)
                dragBox.Hide();
            else
                dragBox.FinishCapture();
            return true;
        }

        protected override bool OnClick(InputState state)
        {
            dragBox?.Hide();
            return true;
        }
    }

    public class DragContainer : CompositeDrawable
    {
        public IEnumerable<DrawableHitObject> CapturableObjects;

        private readonly Container borderMask;
        private readonly Drawable background;
        private readonly MarkerContainer markers;

        private Color4 captureFinishedColour;

        private readonly Vector2 startPos;

        public DragContainer(Vector2 startPos)
        {
            this.startPos = startPos;

            InternalChildren = new Drawable[]
            {
                borderMask = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = 2,
                    MaskingSmoothness = 1,
                    Child = background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.1f,
                        AlwaysPresent = true
                    },
                },
                markers = new MarkerContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            captureFinishedColour = colours.Yellow;
        }

        public void Track(Vector2 position)
        {
            var trackingRectangle = RectangleF.FromLTRB(
                Math.Min(startPos.X, position.X),
                Math.Min(startPos.Y, position.Y),
                Math.Max(startPos.X, position.X),
                Math.Max(startPos.Y, position.Y));

            Position = trackingRectangle.Location;
            Size = trackingRectangle.Size;
        }

        private List<DrawableHitObject> capturedHitObjects = new List<DrawableHitObject>();
        public IReadOnlyList<DrawableHitObject> CapturedHitObjects => capturedHitObjects;

        public void UpdateCapture()
        {
            capturedHitObjects.Clear();

            foreach (var obj in CapturableObjects)
            {
                if (!obj.IsAlive || !obj.IsPresent)
                    continue;

                var objectPosition = obj.ToScreenSpace(obj.SelectionPoint);
                if (ScreenSpaceDrawQuad.Contains(objectPosition))
                    capturedHitObjects.Add(obj);
            }
        }

        public void FinishCapture()
        {
            // Move the rectangle to cover the hitobjects
            var topLeft = new Vector2(float.MaxValue, float.MaxValue);
            var bottomRight = new Vector2(float.MinValue, float.MinValue);

            foreach (var obj in capturedHitObjects)
            {
                topLeft = Vector2.ComponentMin(topLeft, Parent.ToLocalSpace(obj.SelectionQuad.TopLeft));
                bottomRight = Vector2.ComponentMax(bottomRight, Parent.ToLocalSpace(obj.SelectionQuad.BottomRight));
            }

            topLeft -= new Vector2(5);
            bottomRight += new Vector2(5);

            this.MoveTo(topLeft, 200, Easing.OutQuint)
                .ResizeTo(bottomRight - topLeft, 200, Easing.OutQuint)
                .FadeColour(captureFinishedColour, 200);

            borderMask.BorderThickness = 3;

            background.Delay(50).FadeOut(200);
            markers.FadeIn(200);
        }

        private bool isActive = true;
        public override bool HandleInput => isActive;

        public override void Hide()
        {
            isActive = false;
            this.FadeOut(400, Easing.OutQuint).Expire();
        }
    }

    public class MarkerContainer : CompositeDrawable
    {
        public Action<RectangleF> ResizeRequested;

        public MarkerContainer()
        {
            Padding = new MarginPadding(1);

            InternalChildren = new Drawable[]
            {
                new Marker
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.Centre
                },
                new Marker
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.Centre
                },
                new Marker
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.Centre
                },
                new Marker
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.Centre
                },
                new CentreMarker
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            };
        }
    }

    public class Marker : CompositeDrawable
    {
        private float marker_size = 10;

        public Marker()
        {
            Size = new Vector2(marker_size);

            InternalChild = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Child = new Box { RelativeSizeAxes = Axes.Both }
            };
        }
    }

    public class CentreMarker : CompositeDrawable
    {
        private float marker_size = 10;
        private float line_width = 2;

        public CentreMarker()
        {
            Size = new Vector2(marker_size);

            InternalChildren = new[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Height = line_width
                },
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Width = line_width
                },
            };
        }
    }
}
