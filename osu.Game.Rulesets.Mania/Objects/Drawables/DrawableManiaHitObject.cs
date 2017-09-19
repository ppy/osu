// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using OpenTK.Graphics;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public abstract class DrawableManiaHitObject<TObject> : DrawableScrollingHitObject<ManiaHitObject>
        where TObject : ManiaHitObject
    {
        /// <summary>
        /// The key that will trigger input for this hit object.
        /// </summary>
        protected ManiaAction Action { get; }

        public new TObject HitObject;

        protected DrawableManiaHitObject(TObject hitObject, ManiaAction? action = null)
            : base(hitObject)
        {
            RelativePositionAxes = Axes.Y;
            HitObject = hitObject;

            if (action != null)
                Action = action.Value;
        }

        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
            set
            {
                if (base.AccentColour == value)
                    return;
                base.AccentColour = value;
            }
        }
    }
}
