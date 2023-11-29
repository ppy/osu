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
    // A modified implementation of Bar which supports multiple accent bars
    public partial class PolyBar : Container
    {
        // Box to represent the background of the bars
        private readonly Box background;
        // Each accent coloured bar within the PolyBar
        private readonly Box[] bars;
        // number of bars, could use bars.Length instead but moved to readonly field for readability
        private readonly int barCount;
        private const int resize_duration = 250;
        private const Easing easing = Easing.InOutCubic;
        // Each of these maps to a box in bars 1:1, lengths[i] represents the length of bars[i]
        private float[] lengths;
        // getters and setters for the lengths are moved to functions, because C# doesn't support indexed getters
        public void SetLength(int index, float value)
        {
            lengths[index] = Math.Clamp(value, 0, 1);
            updateBarLength(index);
        }
        public float GetLength(int index) => lengths[index];
        // update colour of background through this class
        public Color4 BackgroundColour
        {
            get => background.Colour;
            set => background.Colour = value;
        }
        // getters and setters for colours, maps same as lengths and bars, GetColour(i) returns the colour of bars[i]
        public void SetColour(int index, Color4 value)
        {
            bars[index].Colour = value;
        }
        public Color4 GetColour(int index) => bars[index].Colour;
        // Only supports all bars going in the same direction currently
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
        // updates the length of a single bar of the given index, but will update all lengths if an argument of -1 or no argument is given
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
        // update the length of every bar
        private void updateBarLengths()
        {
            for (int i = 0; i < barCount; i++)
                updateBarLength(i);
        }
    }
}