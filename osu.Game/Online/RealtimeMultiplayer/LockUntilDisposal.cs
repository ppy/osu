// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;

namespace osu.Game.Online.RealtimeMultiplayer
{
    public readonly struct LockUntilDisposal : IDisposable
    {
        private readonly object lockTarget;

        public LockUntilDisposal(object lockTarget)
        {
            this.lockTarget = lockTarget;
            Monitor.Enter(lockTarget);
        }

        public void Dispose()
        {
            Monitor.Exit(lockTarget);
        }
    }
}
