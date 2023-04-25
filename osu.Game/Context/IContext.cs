// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Context;

public interface IContext
{
    /// <summary>
    /// Makes a deep copy of this context.
    /// </summary>
    /// <returns>The deep copy of this context.</returns>
    public IContext Copy();
}
