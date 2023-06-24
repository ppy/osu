// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Screens;

namespace osu.Game.Screens
{
    /// <summary>
    /// A screen which manages a nested stack of screens within itself.
    /// </summary>
    public interface IHasSubScreenStack
    {
        ScreenStack SubScreenStack { get; }
    }
}
