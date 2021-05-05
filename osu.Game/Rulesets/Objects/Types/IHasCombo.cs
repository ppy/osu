// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that is part of a combo.
    /// </summary>
    public interface IHasCombo
    {
        /// <summary>
        /// Whether the HitObject starts a new combo.
        /// </summary>
        bool NewCombo { get; }
    }
}
