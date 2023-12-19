// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.UI
{
    /// <summary>
    /// An action that an <see cref="IHitPolicy"/> recommends be taken in response to a click
    /// on a <see cref="DrawableOsuHitObject"/>.
    /// </summary>
    public enum ClickAction
    {
        Ignore,
        Shake,
        Hit
    }
}
