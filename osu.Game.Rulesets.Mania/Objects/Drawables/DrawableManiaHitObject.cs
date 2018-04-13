// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public abstract class DrawableManiaHitObject<TObject> : DrawableHitObject<ManiaHitObject>
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
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            HitObject = hitObject;

            if (action != null)
                Action = action.Value;
        }
    }
}
