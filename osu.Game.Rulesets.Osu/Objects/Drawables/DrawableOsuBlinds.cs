// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using OpenTK.Graphics;
using osu.Framework.Allocation;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    /// <summary>
    /// Element for the Blinds mod drawing 2 black boxes covering the whole screen which resize inside a restricted area with some leniency.
    /// </summary>
    public class DrawableOsuBlinds : Container
    {
        private Box box1, box2;
        private float target = 1;
        private readonly float easing = 1;
        private readonly Container restrictTo;

        /// <summary>
        /// <para>
        /// Percentage of playfield to extend blinds over. Basically moves the origin points where the blinds start.
        /// </para>
        /// <para>
        /// -1 would mean the blinds always cover the whole screen no matter health.
        /// 0 would mean the blinds will only ever be on the edge of the playfield on 0% health.
        /// 1 would mean the blinds are fully outside the playfield on 50% health.
        /// Infinity would mean the blinds are always outside the playfield except on 100% health.
        /// </para>
        /// </summary>
        private const float leniency = 0.2f;

        public DrawableOsuBlinds(Container restrictTo)
        {
            this.restrictTo = restrictTo;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            Width = 1;
            Height = 1;

            Add(box1 = new Box
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Colour = Color4.Black,
                RelativeSizeAxes = Axes.Y,
                Width = 0,
                Height = 1
            });
            Add(box2 = new Box
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Colour = Color4.Black,
                RelativeSizeAxes = Axes.Y,
                Width = 0,
                Height = 1
            });
        }

        protected override void Update()
        {
            float start = Parent.ToLocalSpace(restrictTo.ScreenSpaceDrawQuad.TopLeft).X;
            float end = Parent.ToLocalSpace(restrictTo.ScreenSpaceDrawQuad.TopRight).X;
            float rawWidth = end - start;
            start -= rawWidth * leniency * 0.5f;
            end += rawWidth * leniency * 0.5f;
            float width = (end - start) * 0.5f * easing;
            // different values in case the playfield ever moves from center to somewhere else.
            box1.Width = start + width;
            box2.Width = DrawWidth - end + width;
        }

        /// <summary>
        /// Health between 0 and 1 for the blinds to base the width on. Will get animated for 200ms using out-quintic easing.
        /// </summary>
        public float Value
        {
            set
            {
                target = value;
                this.TransformTo(nameof(easing), target, 200, Easing.OutQuint);
            }
            get
            {
                return target;
            }
        }
    }
}
