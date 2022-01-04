// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Taiko.UI
{
    internal class DrumRollHitContainer : ScrollingHitObjectContainer
    {
        // TODO: this usage is buggy.
        // Because `LifetimeStart` is set based on scrolling, lifetime is not same as the time when the object is created.
        // If the `Update` override is removed, it breaks in an obscure way.
        protected override bool RemoveRewoundEntry => true;

        protected override void Update()
        {
            base.Update();

            // Remove any auxiliary hit notes that were spawned during a drum roll but subsequently rewound.
            for (int i = AliveInternalChildren.Count - 1; i >= 0; i--)
            {
                var flyingHit = (DrawableFlyingHit)AliveInternalChildren[i];
                if (Time.Current <= flyingHit.HitObject.StartTime)
                    Remove(flyingHit);
            }
        }
    }
}
