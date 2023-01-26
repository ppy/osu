// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    public interface IHasStreamPath : IHasPath
    {
        /// <summary>
        /// The stream path.
        /// </summary>
        StreamPath StreamPath { get; }
    }
}
