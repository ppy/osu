// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Pooling;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class HitMarker : PoolableDrawableWithLifetime<AimPointEntry>
    {
        public HitMarker()
        {
            Origin = Anchor.Centre;
        }

        protected override void OnApply(AimPointEntry entry)
        {
            Position = entry.Position;

            using (BeginAbsoluteSequence(LifetimeStart))
                Show();

            using (BeginAbsoluteSequence(LifetimeEnd - 200))
                this.FadeOut(200);
        }
    }
}
