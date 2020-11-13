// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// A <see cref="CompositeDrawable"/> that pools <see cref="DrawableHitObject"/>s and allows children to retrieve them via <see cref="GetPooledDrawableRepresentation"/>.
    /// </summary>
    [Cached(typeof(HitObjectPoolProvider))]
    public class HitObjectPoolProvider : CompositeDrawable
    {
        [Resolved]
        private DrawableRuleset drawableRuleset { get; set; }

        [Resolved]
        private IReadOnlyList<Mod> mods { get; set; }

        [Resolved(CanBeNull = true)]
        private HitObjectPoolProvider parentProvider { get; set; }

        private readonly Dictionary<Type, IDrawablePool> pools = new Dictionary<Type, IDrawablePool>();

        /// <summary>
        /// Registers a default <see cref="DrawableHitObject"/> pool with this <see cref="DrawableRuleset"/> which is to be used whenever
        /// <see cref="DrawableHitObject"/> representations are requested for the given <typeparamref name="TObject"/> type (via <see cref="GetPooledDrawableRepresentation"/>).
        /// </summary>
        /// <param name="initialSize">The number of <see cref="DrawableHitObject"/>s to be initially stored in the pool.</param>
        /// <param name="maximumSize">
        /// The maximum number of <see cref="DrawableHitObject"/>s that can be stored in the pool.
        /// If this limit is exceeded, every subsequent <see cref="DrawableHitObject"/> will be created anew instead of being retrieved from the pool,
        /// until some of the existing <see cref="DrawableHitObject"/>s are returned to the pool.
        /// </param>
        /// <typeparam name="TObject">The <see cref="HitObject"/> type.</typeparam>
        /// <typeparam name="TDrawable">The <see cref="DrawableHitObject"/> receiver for <typeparamref name="TObject"/>s.</typeparam>
        protected void RegisterPool<TObject, TDrawable>(int initialSize, int? maximumSize = null)
            where TObject : HitObject
            where TDrawable : DrawableHitObject, new()
            => RegisterPool<TObject, TDrawable>(new DrawablePool<TDrawable>(initialSize, maximumSize));

        /// <summary>
        /// Registers a custom <see cref="DrawableHitObject"/> pool with this <see cref="DrawableRuleset"/> which is to be used whenever
        /// <see cref="DrawableHitObject"/> representations are requested for the given <typeparamref name="TObject"/> type (via <see cref="GetPooledDrawableRepresentation"/>).
        /// </summary>
        /// <param name="pool">The <see cref="DrawablePool{T}"/> to register.</param>
        /// <typeparam name="TObject">The <see cref="HitObject"/> type.</typeparam>
        /// <typeparam name="TDrawable">The <see cref="DrawableHitObject"/> receiver for <typeparamref name="TObject"/>s.</typeparam>
        protected void RegisterPool<TObject, TDrawable>([NotNull] DrawablePool<TDrawable> pool)
            where TObject : HitObject
            where TDrawable : DrawableHitObject, new()
        {
            pools[typeof(TObject)] = pool;
            AddInternal(pool);
        }

        /// <summary>
        /// Attempts to retrieve the poolable <see cref="DrawableHitObject"/> representation of a <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to retrieve the <see cref="DrawableHitObject"/> representation of.</param>
        /// <returns>The <see cref="DrawableHitObject"/> representing <see cref="HitObject"/>, or <c>null</c> if no poolable representation exists.</returns>
        [CanBeNull]
        public DrawableHitObject GetPooledDrawableRepresentation([NotNull] HitObject hitObject)
        {
            var lookupType = hitObject.GetType();

            IDrawablePool pool;

            // Tests may add derived hitobject instances for which pools don't exist. Try to find any applicable pool and dynamically assign the type if the pool exists.
            if (!pools.TryGetValue(lookupType, out pool))
            {
                foreach (var (t, p) in pools)
                {
                    if (!t.IsInstanceOfType(hitObject))
                        continue;

                    pools[lookupType] = pool = p;
                    break;
                }
            }

            if (pool == null)
                return parentProvider?.GetPooledDrawableRepresentation(hitObject);

            return (DrawableHitObject)pool.Get(d =>
            {
                var dho = (DrawableHitObject)d;

                // If this is the first time this DHO is being used (not loaded), then apply the DHO mods.
                // This is done before Apply() so that the state is updated once when the hitobject is applied.
                if (!dho.IsLoaded)
                {
                    foreach (var m in mods.OfType<IApplicableToDrawableHitObjects>())
                        m.ApplyToDrawableHitObjects(dho.Yield());
                }

                dho.Apply(hitObject, drawableRuleset.GetLifetimeEntry(hitObject));
            });
        }
    }
}
