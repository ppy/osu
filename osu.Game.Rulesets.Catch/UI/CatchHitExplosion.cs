// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Judgements;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// The base for hit explosion types used with <see cref="PoolableHitExplosion"/>.
    /// </summary>
    public abstract class CatchHitExplosion : CompositeDrawable, ICatchHitExplosion
    {
        public Color4 ObjectColour { get; set; }
        public PalpableCatchHitObject HitObject { get; set; }
        public JudgementResult JudgementResult { get; set; }
        public float CatcherMargin { get; set; }
        public float CatcherWidth { get; set; }
        public float CatchPosition { get; set; }
        public abstract void Animate();
    }

    /// <summary>
    /// Provides properties hit explosions can use.
    /// </summary>
    public interface ICatchHitExplosion
    {
        /// <summary>
        /// Color of the object the hit explosion is attached to.
        /// </summary>
        public Color4 ObjectColour
        {
            set;
        }

        /// <summary>
        /// Catch hitobject the explosion was created from.
        /// </summary>
        public PalpableCatchHitObject HitObject
        {
            set;
        }

        /// <summary>
        /// Judgement attached to the hit explosion.
        /// </summary>
        public JudgementResult JudgementResult
        {
            set;
        }

        /// <inheritdoc cref="Catcher.ALLOWED_CATCH_RANGE"/>
        public float CatcherMargin
        {
            set;
        }

        /// <inheritdoc cref="Catcher.catchWidth"/>
        public float CatcherWidth
        {
            set;
        }

        /// <summary>
        /// Position on the catcher where the hitobject lands.
        /// </summary>
        public float CatchPosition
        {
            set;
        }

        public void Animate();
    }
}
