// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// An overlay layer on top of the player HUD which fades to red when the current player health falls a certain threshold defined by <see cref="LowHealthThreshold"/>.
    /// </summary>
    public class FaillingLayer : HealthDisplay
    {
        private const float max_alpha = 0.4f;

        private readonly Box box;

        /// <summary>
        /// The threshold under which the current player life should be considered low and the layer should start fading in.
        /// </summary>
        protected virtual double LowHealthThreshold => 0.20f;

        public FaillingLayer()
        {
            RelativeSizeAxes = Axes.Both;
            Child = box = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour color)
        {
            box.Colour = color.Red;
        }

        protected override void Update()
        {
            box.Alpha = (float)Math.Clamp(max_alpha * (1 - Current.Value / LowHealthThreshold), 0, max_alpha);
            base.Update();
        }
    }
}
