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
        protected readonly Container Fill;

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
                    Fill.Width = 0;
            }
        }

        public DragBar()
        {
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                Fill = new Container
                {
                    Name = "FillContainer",
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                }
            };
        }

        public void UpdatePosition(float position)
        {
            if (IsSeeking || !IsEnabled) return;

            updatePosition(position, false);
        }

        private void seek(InputState state)
        {
            float seekLocation = state.Mouse.Position.X / DrawWidth;

            if (!IsEnabled) return;

            SeekRequested?.Invoke(seekLocation);
            updatePosition(seekLocation);
        }

        private void updatePosition(float position, bool easing = true)
        {
            position = MathHelper.Clamp(position, 0, 1);
            Fill.TransformTo(() => Fill.Width, position, easing ? 200 : 0, EasingTypes.OutQuint, new TransformSeek());
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
