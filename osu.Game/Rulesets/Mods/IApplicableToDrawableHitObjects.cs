// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mods
{
    [Obsolete(@"Use the singular version IApplicableToDrawableHitObject instead.")] // Can be removed 20211216
    public interface IApplicableToDrawableHitObjects : IApplicableToDrawableHitObject
    {
        void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables);

        void IApplicableToDrawableHitObject.ApplyToDrawableHitObject(DrawableHitObject drawable) => ApplyToDrawableHitObjects(drawable.Yield());
    }
}
