// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Platform;

namespace osu.Game.Tests
{
    /// <summary>
    /// A headless host which cleans up before running (removing any remnants from a previous execution).
    /// </summary>
    public class CleanRunHeadlessGameHost : HeadlessGameHost
    {
        public CleanRunHeadlessGameHost(string gameName = @"", bool bindIPC = false, bool realtime = true)
            : base(gameName, bindIPC, realtime)
        {
            Storage.DeleteDirectory(string.Empty);
        }
    }
}
