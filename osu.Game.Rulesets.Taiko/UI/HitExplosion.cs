// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// A circle explodes from the hit target to indicate a hitobject has been hit.
    /// </summary>
    internal partial class HitExplosion : HitExplosionBase
    {
        private readonly HitResult result;

        /// <summary>
        /// This constructor only exists to meet the <c>new()</c> type constraint of <see cref="DrawablePool{T}"/>.
        /// </summary>
        public HitExplosion()
            : this(HitResult.Great)
        {
        }

        public HitExplosion(HitResult result)
        {
            this.result = result;
            Size = new Vector2(TaikoHitObject.DEFAULT_SIZE);
            RelativePositionAxes = Axes.Both;
        }

        protected override SkinnableDrawable OnLoadSkinnableCreate() =>
            new SkinnableDrawable(new TaikoSkinComponentLookup(getComponentName(result)), _ => new DefaultHitExplosion(result));

        private static TaikoSkinComponents getComponentName(HitResult result)
        {
            switch (result)
            {
                case HitResult.Miss:
                    return TaikoSkinComponents.TaikoExplosionMiss;

                case HitResult.Ok:
                    return TaikoSkinComponents.TaikoExplosionOk;

                case HitResult.Great:
                    return TaikoSkinComponents.TaikoExplosionGreat;
            }

            throw new ArgumentOutOfRangeException(nameof(result), $"Invalid result type: {result}");
        }
    }
}
