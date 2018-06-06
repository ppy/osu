// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
