// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Realms;

namespace osu.Game.Database
{
    public class RealmWriteUsage : IDisposable
    {
        public readonly Realm Context;
        private readonly Action<RealmWriteUsage> usageCompleted;

        public bool RollbackRequired { get; private set; }

        public RealmWriteUsage(Realm context, Action<RealmWriteUsage> onCompleted)
        {
            Context = context;
            usageCompleted = onCompleted;
        }

        /// <summary>
        /// Whether this write usage will commit a transaction on completion.
        /// If false, there is a parent usage responsible for transaction commit.
        /// </summary>
        public bool IsTransactionLeader;

        private bool isDisposed;

        protected void Dispose(bool disposing)
        {
            if (isDisposed) return;

            isDisposed = true;

            usageCompleted?.Invoke(this);
        }

        public void Rollback(Exception error = null)
        {
            RollbackRequired = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RealmWriteUsage()
        {
            Dispose(false);
        }
    }
}
