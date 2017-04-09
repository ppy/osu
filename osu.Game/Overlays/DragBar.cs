// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using OpenTK;

namespace osu.Game.Overlays
{
    public class DragBar : Container
    {
        private readonly Box fill;

        public Action<float> SeekRequested;

        public bool IsSeeking { get; private set; }

        private bool enabled = true;
        public bool IsEnabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                if (!enabled)
                    fill.Width = 0;
            }
        }

        public DragBar()
        {
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                fill = new Box
                {
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0
                }
            };
        }

        public void UpdatePosition(float position)
        {
            if (IsSeeking || !IsEnabled) return;

            updatePosition(position);
        }

        private void seek(InputState state)
        {
            if (!IsEnabled) return;
            float seekLocation = state.Mouse.Position.X / DrawWidth;
            SeekRequested?.Invoke(seekLocation);
            updatePosition(seekLocation);
        }

        private void updatePosition(float position)
        {
            position = MathHelper.Clamp(position, 0, 1);
            fill.TransformTo(fill.Width, position, 200, EasingTypes.OutQuint, new TransformSeek());
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

        protected override bool OnDragStart(InputState state) => IsSeeking = true;

        protected override bool OnDragEnd(InputState state)
        {
            IsSeeking = false;
            return true;
        }

        private class TransformSeek : TransformFloat
        {
            public override void Apply(Drawable d)
            {
                base.Apply(d);
                d.Width = CurrentValue;
            }
        }
    }
}
