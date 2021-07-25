// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Rulesets.Objects.Pooling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.UI
{
    public class HitExplosion : PoolableDrawableWithLifetime<HitExplosionEntry>
    {
        [Cached]
        private Bindable<HitExplosionEntry> bindableEntry { get; set; } = new Bindable<HitExplosionEntry>();

        public HitExplosion()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new SkinnableDrawable(new CatchSkinComponent(CatchSkinComponents.HitExplosion), _ => new DefaultHitExplosion())
            {
                CentreComponent = false,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre
            };
        }

        protected override void OnApply(HitExplosionEntry entry)
        {
            base.OnApply(entry);

            bindableEntry.Value = entry;
        }
    }
}
