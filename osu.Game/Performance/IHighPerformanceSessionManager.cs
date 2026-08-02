// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Performance
{
    /// <summary>
    /// Allows creating a temporary "high performance" session, with the goal of optimising runtime
    /// performance for gameplay purposes.
    ///
    /// On desktop platforms, this will set a low latency GC mode which collects more frequently to avoid
    /// GC spikes.
    /// </summary>
    public interface IHighPerformanceSessionManager
    {
        /// <summary>
        /// Whether a high performance session is currently active.
        /// </summary>
        bool IsSessionActive { get; }

        /// <summary>
        /// Start a new high performance session.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> which will end the session when disposed.</returns>
        IDisposable BeginSession();
    }
}
