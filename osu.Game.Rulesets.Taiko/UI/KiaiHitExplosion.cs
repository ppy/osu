// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// An explosion from the hit target in Kiai mode to indicate a hitobject has been hit.
    /// </summary>
    internal partial class KiaiHitExplosion : HitExplosionBase
    {
        private readonly HitType hitType;

        /// <summary>
        /// This constructor only exists to meet the <c>new()</c> type constraint of <see cref="DrawablePool{T}"/>.
        /// </summary>
        public KiaiHitExplosion() : this(HitType.Centre) { }

        public KiaiHitExplosion(HitType hitType)
        {
            this.hitType = hitType;
            Size = new Vector2(TaikoHitObject.DEFAULT_SIZE, 1);
        }

        public KiaiHitExplosion(DrawableHitObject judgedObject, HitType hitType) : this(hitType)
        {
            Apply(judgedObject);
        }

        protected override SkinnableDrawable OnLoadSkinnableCreate() =>
            new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.TaikoExplosionKiai), _ => new DefaultKiaiHitExplosion(hitType));
    }
}
