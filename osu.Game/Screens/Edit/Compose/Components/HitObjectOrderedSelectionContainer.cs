// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A container for <see cref="SelectionBlueprint"/> ordered by their <see cref="HitObject"/> start times.
    /// </summary>
    public sealed class HitObjectOrderedSelectionContainer : Container<SelectionBlueprint>
    {
        public override void Add(SelectionBlueprint drawable)
        {
            base.Add(drawable);
            bindStartTime(drawable);
        }

        public override bool Remove(SelectionBlueprint drawable)
        {
            if (!base.Remove(drawable))
                return false;

            unbindStartTime(drawable);
            return true;
        }

        public override void Clear(bool disposeChildren)
        {
            base.Clear(disposeChildren);
            unbindAllStartTimes();
        }

        private readonly Dictionary<SelectionBlueprint, IBindable> startTimeMap = new Dictionary<SelectionBlueprint, IBindable>();

        private void bindStartTime(SelectionBlueprint blueprint)
        {
            var bindable = blueprint.HitObject.StartTimeBindable.GetBoundCopy();

            bindable.BindValueChanged(_ =>
            {
                if (LoadState >= LoadState.Ready)
                    SortInternal();
            });

            startTimeMap[blueprint] = bindable;
        }

        private void unbindStartTime(SelectionBlueprint blueprint)
        {
            startTimeMap[blueprint].UnbindAll();
            startTimeMap.Remove(blueprint);
        }

        private void unbindAllStartTimes()
        {
            foreach (var kvp in startTimeMap)
                kvp.Value.UnbindAll();
            startTimeMap.Clear();
        }

        protected override int Compare(Drawable x, Drawable y)
        {
            var xObj = (SelectionBlueprint)x;
            var yObj = (SelectionBlueprint)y;

            // Put earlier blueprints towards the end of the list, so they handle input first
            int i = yObj.HitObject.StartTime.CompareTo(xObj.HitObject.StartTime);
            return i == 0 ? CompareReverseChildID(x, y) : i;
        }
    }
}
