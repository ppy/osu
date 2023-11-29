// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Graphics.UserInterface
{
    public partial class PolyBar : Container
    {
        private readonly Box background;
        private readonly Box[] bars;
        private readonly int barCount;
        private const int resize_duration = 250;
        private const Easing easing = Easing.InOutCubic;
        private float[] lengths;
        public void SetLength(int index, float value)
        {
            lengths[index] = Math.Clamp(value, 0, 1);
            updateBarLength(index);
        }
        public float GetLength(int index) => lengths[index];
        public Color4 BackgroundColour
        {
            get => background.Colour;
            set => background.Colour = value;
        }
        public void SetColour(int index, Color4 value)
        {
            bars[index].Colour = value;
        }
        public Color4 GetColour(int index) => bars[index].Colour;
        // This should be modified so bars can go different directions, this is all that's needed for current usecase though
        private BarDirection direction = BarDirection.LeftToRight;
        public BarDirection Direction
        {
            get => direction;
            set
            {
                direction = value;
                updateBarLength();
            }
        }
        public PolyBar(int barCount)
        {
            lengths = new float[barCount];
            this.barCount = barCount;
            List<Drawable> children = new List<Drawable>();
            background = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = new Color4(0, 0, 0, 0),
            };
            children.Add(background);
            bars = new Box[barCount];
            for (int i = 0; i < barCount; i++)
            {
                bars[i] = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0,
                };
                children.Add(bars[i]);
            }
            Children = children.ToArray();
        }
        private void updateBarLength(int index=-1)
        {
            if (index == -1)
            {
                updateBarLengths();
                return;
            }
            switch (direction)
            {
                case BarDirection.LeftToRight:
                case BarDirection.RightToLeft:
                    bars[index].ResizeTo(new Vector2(lengths[index], 1), resize_duration, easing);
                    break;

                case BarDirection.TopToBottom:
                case BarDirection.BottomToTop:
                    bars[index].ResizeTo(new Vector2(1, lengths[index]), resize_duration, easing);
                    break;
            }

            switch (direction)
            {
                case BarDirection.LeftToRight:
                case BarDirection.TopToBottom:
                    bars[index].Anchor = Anchor.TopLeft;
                    bars[index].Origin = Anchor.TopLeft;
                    break;

                case BarDirection.RightToLeft:
                case BarDirection.BottomToTop:
                    bars[index].Anchor = Anchor.BottomRight;
                    bars[index].Origin = Anchor.BottomRight;
                    break;
            }

        }
        private void updateBarLengths()
        {
            for (int i = 0; i < barCount; i++)
                updateBarLength(i);
        }
    }
}