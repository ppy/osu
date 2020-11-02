// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Performance;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.UI
{
    public class HitObjectContainer : LifetimeManagementContainer
    {
        public IEnumerable<DrawableHitObject> Objects => InternalChildren.Cast<DrawableHitObject>().OrderBy(h => h.HitObject.StartTime);
        public IEnumerable<DrawableHitObject> AliveObjects => AliveInternalChildren.Cast<DrawableHitObject>().OrderBy(h => h.HitObject.StartTime);

        private readonly Dictionary<DrawableHitObject, (IBindable<double> bindable, double timeAtAdd)> startTimeMap = new Dictionary<DrawableHitObject, (IBindable<double>, double)>();

        public HitObjectContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public virtual void Add(DrawableHitObject hitObject)
        {
            // Added first for the comparer to remain ordered during AddInternal
            startTimeMap[hitObject] = (hitObject.HitObject.StartTimeBindable.GetBoundCopy(), hitObject.HitObject.StartTime);
            startTimeMap[hitObject].bindable.BindValueChanged(_ => onStartTimeChanged(hitObject));

            AddInternal(hitObject);
        }

        public virtual bool Remove(DrawableHitObject hitObject)
        {
            if (!RemoveInternal(hitObject))
                return false;

            // Removed last for the comparer to remain ordered during RemoveInternal
            startTimeMap[hitObject].bindable.UnbindAll();
            startTimeMap.Remove(hitObject);

            return true;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            unbindStartTimeMap();
        }

        public virtual void Clear(bool disposeChildren = true)
        {
            ClearInternal(disposeChildren);
            unbindStartTimeMap();
        }

        private void unbindStartTimeMap()
        {
            foreach (var kvp in startTimeMap)
                kvp.Value.bindable.UnbindAll();
            startTimeMap.Clear();
        }

        public int IndexOf(DrawableHitObject hitObject) => IndexOfInternal(hitObject);

        private void onStartTimeChanged(DrawableHitObject hitObject)
        {
            if (!RemoveInternal(hitObject))
                return;

            // Update the stored time, preserving the existing bindable
            startTimeMap[hitObject] = (startTimeMap[hitObject].bindable, hitObject.HitObject.StartTime);
            AddInternal(hitObject);
        }

        protected override int Compare(Drawable x, Drawable y)
        {
            if (!(x is DrawableHitObject xObj) || !(y is DrawableHitObject yObj))
                return base.Compare(x, y);

            // Put earlier hitobjects towards the end of the list, so they handle input first
            int i = startTimeMap[yObj].timeAtAdd.CompareTo(startTimeMap[xObj].timeAtAdd);
            return i == 0 ? CompareReverseChildID(x, y) : i;
        }

        protected override void OnChildLifetimeBoundaryCrossed(LifetimeBoundaryCrossedEvent e)
        {
            if (!(e.Child is DrawableHitObject hitObject))
                return;

            if ((e.Kind == LifetimeBoundaryKind.End && e.Direction == LifetimeBoundaryCrossingDirection.Forward)
                || (e.Kind == LifetimeBoundaryKind.Start && e.Direction == LifetimeBoundaryCrossingDirection.Backward))
            {
                hitObject.OnKilled();
            }
        }
    }
}
