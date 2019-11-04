// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that allows user control over it's properties.
    /// </summary>
    public interface IModHasSettings : IApplicableMod
    {
        Drawable[] CreateControls();
    }
}
