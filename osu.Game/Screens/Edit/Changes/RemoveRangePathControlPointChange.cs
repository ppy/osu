// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Changes
{
    /// <summary>
    /// Removes a range of <see cref="PathControlPoint"/>s from the provided <see cref="BindableList{T}"/>.
    /// </summary>
    public class RemoveRangePathControlPointChange : CompositeChange
    {
        private readonly BindableList<PathControlPoint> controlPoints;
        private readonly int startIndex;
        private readonly int count;

        public RemoveRangePathControlPointChange(BindableList<PathControlPoint> controlPoints, int startIndex, int count)
        {
            this.controlPoints = controlPoints;
            this.startIndex = startIndex;
            this.count = count;
        }

        protected override void SubmitChanges()
        {
            for (int i = 0; i < count; i++)
                Submit(new RemovePathControlPointChange(controlPoints, startIndex));
        }
    }
}
