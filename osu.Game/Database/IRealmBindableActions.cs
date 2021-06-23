// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Database
{
    public interface IRealmBindableActions
    {
        /// <summary>
        /// Re-run bind actions on the current context.
        /// Should only be called after a context switch occurs.
        /// </summary>
        void RunBindActions();
    }
}
