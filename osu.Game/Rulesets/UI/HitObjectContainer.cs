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

        private readonly Dictionary<DrawableHitObject, IBindable> startTimeMap = new Dictionary<DrawableHitObject, IBindable>();

        public HitObjectContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public virtual void Add(DrawableHitObject hitObject)
        {
            bindStartTime(hitObject);
            AddInternal(hitObject);
        }

        public virtual bool Remove(DrawableHitObject hitObject)
        {
            if (!RemoveInternal(hitObject))
                return false;

            unbindStartTime(hitObject);

            return true;
        }

        public virtual void Clear(bool disposeChildren = true)
        {
            ClearInternal(disposeChildren);
            unbindAllStartTimes();
        }

        public int IndexOf(DrawableHitObject hitObject) => IndexOfInternal(hitObject);

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

        #region Comparator + StartTime tracking

        private void bindStartTime(DrawableHitObject hitObject)
        {
            var bindable = hitObject.StartTimeBindable.GetBoundCopy();
            bindable.BindValueChanged(_ => onStartTimeChanged(hitObject));

            startTimeMap[hitObject] = bindable;
        }

        private void unbindStartTime(DrawableHitObject hitObject)
        {
            startTimeMap[hitObject].UnbindAll();
            startTimeMap.Remove(hitObject);
        }

        private void unbindAllStartTimes()
        {
            foreach (var kvp in startTimeMap)
                kvp.Value.UnbindAll();
            startTimeMap.Clear();
        }

        private void onStartTimeChanged(DrawableHitObject hitObject) => SortInternal();

        protected override int Compare(Drawable x, Drawable y)
        {
            if (!(x is DrawableHitObject xObj) || !(y is DrawableHitObject yObj))
                return base.Compare(x, y);

            // Put earlier hitobjects towards the end of the list, so they handle input first
            int i = yObj.HitObject.StartTime.CompareTo(xObj.HitObject.StartTime);
            return i == 0 ? CompareReverseChildID(x, y) : i;
        }

        #endregion

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            unbindAllStartTimes();
        }
    }
}
