// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CaughtObjectPool : CompositeDrawable
    {
        private DrawablePool<CaughtFruit> caughtFruitPool;
        private DrawablePool<CaughtBanana> caughtBananaPool;
        private DrawablePool<CaughtDroplet> caughtDropletPool;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                caughtFruitPool = new DrawablePool<CaughtFruit>(50),
                caughtBananaPool = new DrawablePool<CaughtBanana>(100),
                // less capacity is needed compared to fruit because droplet is not stacked
                caughtDropletPool = new DrawablePool<CaughtDroplet>(25),
            };
        }

        [NotNull]
        public CaughtObject Get(PalpableCatchHitObject source)
        {
            switch (source)
            {
                case Fruit _:
                    return caughtFruitPool.Get();

                case Banana _:
                    return caughtBananaPool.Get();

                case Droplet _:
                    return caughtDropletPool.Get();

                default:
                    throw new InvalidOperationException($"Unexpected {nameof(PalpableCatchHitObject)}: {source.GetType().ReadableName()}.");
            }
        }
    }
}
