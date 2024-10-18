// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Changes
{
    /// <summary>
    /// Adds a range of <see cref="PathControlPoint"/>s to the provided <see cref="BindableList{T}"/>.
    /// </summary>
    public class AddRangePathControlPointChange : CompositeChange
    {
        private readonly BindableList<PathControlPoint> controlPoints;
        private readonly IEnumerable<PathControlPoint> points;

        public AddRangePathControlPointChange(BindableList<PathControlPoint> controlPoints, IEnumerable<PathControlPoint> points)
        {
            this.controlPoints = controlPoints;
            this.points = points;
        }

        protected override void SubmitChanges()
        {
            foreach (var point in points)
                Submit(new InsertPathControlPointChange(controlPoints, controlPoints.Count, point));
        }
    }
}
