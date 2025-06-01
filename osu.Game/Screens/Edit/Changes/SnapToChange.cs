// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Changes
{
    /// <summary>
    /// Snaps the provided <see cref="HitObject"/>'s duration using the <see cref="IDistanceSnapProvider"/>.
    /// </summary>
    public class SnapToChange<THitObject> : CompositeChange where THitObject : HitObject, IHasPath, IHasSliderVelocity
    {
        private readonly THitObject hitObject;
        private readonly IDistanceSnapProvider? snapProvider;

        public SnapToChange(THitObject hitObject, IDistanceSnapProvider? snapProvider)
        {
            this.hitObject = hitObject;
            this.snapProvider = snapProvider;
        }

        protected override void SubmitChanges()
        {
            double newDistance = snapProvider?.FindSnappedDistance((float)hitObject.Path.CalculatedDistance, hitObject.StartTime, hitObject) ?? hitObject.Path.CalculatedDistance;
            Submit(new ExpectedDistanceChange(hitObject.Path, newDistance));
        }
    }
}
