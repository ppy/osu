// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints
{
    /// <summary>
    /// A piece of a blueprint which responds to changes in the state of a <see cref="OsuHitObject"/>.
    /// </summary>
    public abstract class HitObjectPiece : CompositeDrawable
    {
        protected readonly OsuHitObject HitObject;

        protected HitObjectPiece(OsuHitObject hitObject)
        {
            HitObject = hitObject;
        }
    }
}
