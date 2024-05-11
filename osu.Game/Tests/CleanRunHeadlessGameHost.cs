// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.CompilerServices;
using osu.Framework;
using osu.Framework.Testing;

namespace osu.Game.Tests
{
    /// <summary>
    /// A headless host which cleans up before running (removing any remnants from a previous execution).
    /// </summary>
    public class CleanRunHeadlessGameHost : TestRunHeadlessGameHost
    {
        private readonly bool bypassCleanupOnSetup;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="bindIPC">Whether to bind IPC channels.</param>
        /// <param name="realtime">Whether the host should be forced to run in realtime, rather than accelerated test time.</param>
        /// <param name="bypassCleanupOnSetup">Whether to bypass directory cleanup on <see cref="SetupForRun"/>.</param>
        /// <param name="bypassCleanupOnDispose">Whether to bypass directory cleanup on host disposal. Should be used only if a subsequent test relies on the files still existing.</param>
        /// <param name="callingMethodName">The name of the calling method, used for test file isolation and clean-up.</param>
        public CleanRunHeadlessGameHost(bool bindIPC = false, bool realtime = true, bool bypassCleanupOnSetup = false, bool bypassCleanupOnDispose = false,
                                        [CallerMemberName] string callingMethodName = @"")
            : base($"{callingMethodName}-{Guid.NewGuid()}", new HostOptions
            {
                IPCPort = bindIPC ? OsuGame.IPC_PORT : null,
            }, bypassCleanup: bypassCleanupOnDispose, realtime: realtime)
        {
            this.bypassCleanupOnSetup = bypassCleanupOnSetup;
        }

        protected override void SetupForRun()
        {
            if (!bypassCleanupOnSetup)
            {
                try
                {
                    Storage.DeleteDirectory(string.Empty);
                }
                catch
                {
                    // May fail if a logging target has already been set via OsuStorage.ChangeTargetStorage.
                }
            }

            // base call needs to be run *after* storage is emptied, as it updates the (static) logger's storage and may start writing
            // log entries from another source if a unit test host is shared over multiple tests, causing a file access denied exception.
            base.SetupForRun();
        }
    }
}
