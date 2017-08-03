// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// Display an icon that is constrained to a physical region on screen.
    /// </summary>
    public class ConstrainedIconContainer : CompositeDrawable
    {
        public Drawable Icon
        {
            get
            {
                return InternalChild;
            }

            set
            {
                InternalChild = value;
            }
        }

        /// <summary>
        /// Determines an edge effect of this <see cref="Container"/>.
        /// Edge effects are e.g. glow or a shadow.
        /// Only has an effect when <see cref="CompositeDrawable.Masking"/> is true.
        /// </summary>
        public new EdgeEffectParameters EdgeEffect
        {
            get { return base.EdgeEffect; }
            set { base.EdgeEffect = value; }
        }

        protected override void Update()
        {
            base.Update();
            if (InternalChildren.Count > 0 && InternalChild.DrawSize.X > 0)
            {
                var fitScale = Math.Min(DrawSize.X / InternalChild.DrawSize.X, DrawSize.Y / InternalChild.DrawSize.Y);
                InternalChild.Scale = new Vector2(fitScale);
                InternalChild.Anchor = Anchor.Centre;
                InternalChild.Origin = Anchor.Centre;
            }
        }

        public ConstrainedIconContainer()
        {
            Masking = true;
        }
    }
}
