// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Graphics;

namespace osu.Game.Updating
{
    /// <summary>
    /// Represents an updater component for checking and performing updates to newer game releases.
    /// </summary>
    public abstract class Updater : Component
    {
        /// <summary>
        /// Checks for newer releases and prepares to update if found.
        /// </summary>
        /// <returns>Whether a newer release has been detected and has done preparing to update.</returns>
        public abstract Task<bool> CheckAndPrepareAsync();
    }
}
