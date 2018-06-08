// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public abstract class DrawableManiaHitObject : DrawableHitObject<ManiaHitObject>
    {
        protected DrawableManiaHitObject(ManiaHitObject hitObject)
            : base(hitObject)
        {
        }

        /// <summary>
        /// Sets the scrolling direction.
        /// </summary>
        public virtual ScrollingDirection Direction
        {
            set
            {
                Anchor = value == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
                Origin = Anchor;

                if (!HasNestedHitObjects)
                    return;

                foreach (var obj in NestedHitObjects.OfType<DrawableManiaHitObject>())
                    obj.Direction = value;
            }
        }
    }

    public abstract class DrawableManiaHitObject<TObject> : DrawableManiaHitObject
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

        protected override void UpdateState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Miss:
                    this.FadeOut(150, Easing.In).Expire();
                    break;
                case ArmedState.Hit:
                    this.FadeOut(150, Easing.OutQuint).Expire();
                    break;
            }
        }
    }
}
