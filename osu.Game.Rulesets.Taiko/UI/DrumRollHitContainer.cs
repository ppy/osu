// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Performance;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Taiko.UI
{
    internal class DrumRollHitContainer : ScrollingHitObjectContainer
    {
        protected override void Update()
        {
            base.Update();

            // Remove any auxiliary hit notes that were spawned during a drum roll but subsequently rewound.
            for (var i = AliveInternalChildren.Count - 1; i >= 0; i--)
            {
                var flyingHit = (DrawableFlyingHit)AliveInternalChildren[i];
                if (Time.Current <= flyingHit.HitObject.StartTime)
                    Remove(flyingHit);
            }
        }

        protected override void OnChildLifetimeBoundaryCrossed(LifetimeBoundaryCrossedEvent e)
        {
            base.OnChildLifetimeBoundaryCrossed(e);

            // ensure all old hits are removed on becoming alive (may miss being in the AliveInternalChildren list above).
            if (e.Kind == LifetimeBoundaryKind.Start && e.Direction == LifetimeBoundaryCrossingDirection.Backward)
                Remove((DrawableHitObject)e.Child);
        }
    }
}
