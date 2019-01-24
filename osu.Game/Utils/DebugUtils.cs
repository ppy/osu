// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Utils
{
    public static class DebugUtils
    {
        public static bool IsDebug
        {
            get
            {
                // ReSharper disable once RedundantAssignment
                bool isDebug = false;
                // Debug.Assert conditions are only evaluated in debug mode
                System.Diagnostics.Debug.Assert(isDebug = true);
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                return isDebug;
            }
        }
    }
}
