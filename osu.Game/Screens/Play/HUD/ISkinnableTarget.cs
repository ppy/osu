// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// A container which supports skinnable components being added to it.
    /// </summary>
    public interface ISkinnableTarget
    {
        public SkinnableTarget Target { get; }
    }
}
