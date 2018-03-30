// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.UI
{
    public class HitObjectContainer : CompositeDrawable
    {
        public virtual IEnumerable<DrawableHitObject> Objects => InternalChildren.Cast<DrawableHitObject>();
        public virtual IEnumerable<DrawableHitObject> AliveObjects => AliveInternalChildren.Cast<DrawableHitObject>();

        public virtual void Add(DrawableHitObject hitObject) => AddInternal(hitObject);
        public virtual bool Remove(DrawableHitObject hitObject) => RemoveInternal(hitObject);

        protected override int Compare(Drawable x, Drawable y)
        {
            if (!(x is DrawableHitObject xObj) || !(y is DrawableHitObject yObj))
                return base.Compare(x, y);

            // Put earlier hitobjects towards the end of the list, so they handle input first
            int i = yObj.HitObject.StartTime.CompareTo(xObj.HitObject.StartTime);
            return i == 0 ? CompareReverseChildID(x, y) : i;
        }
    }
}
