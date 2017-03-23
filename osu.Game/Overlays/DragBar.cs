// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;

namespace osu.Game.Overlays
{
    public class DragBar : Container
    {
        protected readonly Container FillContainer;
        protected readonly Box Fill;

        public Action<float> SeekRequested;
        private bool isDragging;

        private bool enabled;
        public bool IsEnabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                if (!enabled)
                    FillContainer.Width = 0;
            }
        }

        public DragBar()
        {
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                FillContainer = new Container
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0,
                    Children = new Drawable[]
                    {
                        Fill = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                }
            };
        }

        public void UpdatePosition(float position)
        {
            if (isDragging || !IsEnabled) return;

            FillContainer.Width = position;
        }

        private void seek(InputState state)
        {
            if (!IsEnabled) return;
            float seekLocation = state.Mouse.Position.X / DrawWidth;
            SeekRequested?.Invoke(seekLocation);
            FillContainer.Width = seekLocation;
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            seek(state);
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            seek(state);
            return true;
        }

        protected override bool OnDragStart(InputState state) => isDragging = true;

        protected override bool OnDragEnd(InputState state)
        {
            isDragging = false;
            return true;
        }
    }
}
