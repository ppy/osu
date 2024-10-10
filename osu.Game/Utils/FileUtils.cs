// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;

namespace osu.Game.Utils
{
    public static class FileUtils
    {
        /// <summary>
        /// Attempt an IO operation multiple times and only throw if none of the attempts succeed.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="state">The provided state.</param>
        /// <param name="attempts">The number of attempts (250ms wait between each).</param>
        /// <param name="throwOnFailure">Whether to throw an exception on failure. If <c>false</c>, will silently fail.</param>
        public static bool AttemptOperation<T>(Action<T> action, T state, int attempts = 10, bool throwOnFailure = true)
        {
            while (true)
            {
                try
                {
                    action(state);
                    return true;
                }
                catch (Exception)
                {
                    if (attempts-- == 0)
                    {
                        if (throwOnFailure)
                            throw;

                        return false;
                    }
                }

                Thread.Sleep(250);
            }
        }

        /// <summary>
        /// Attempt an IO operation multiple times and only throw if none of the attempts succeed.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="attempts">The number of attempts (250ms wait between each).</param>
        /// <param name="throwOnFailure">Whether to throw an exception on failure. If <c>false</c>, will silently fail.</param>
        public static bool AttemptOperation(Action action, int attempts = 10, bool throwOnFailure = true)
        {
            while (true)
            {
                try
                {
                    action();
                    return true;
                }
                catch (Exception)
                {
                    if (attempts-- == 0)
                    {
                        if (throwOnFailure)
                            throw;

                        return false;
                    }
                }

                Thread.Sleep(250);
            }
        }
    }
}
