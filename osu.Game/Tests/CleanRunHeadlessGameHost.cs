// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        }

        protected override void SetupForRun()
        {
            base.SetupForRun();
            Storage.DeleteDirectory(string.Empty);
        }
    }
}
