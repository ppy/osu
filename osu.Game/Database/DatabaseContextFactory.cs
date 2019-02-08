﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore.Storage;
using osu.Framework.Platform;

namespace osu.Game.Database
{
    public class DatabaseContextFactory : IDatabaseContextFactory
    {
        private readonly Storage storage;

        private const string database_name = @"client";

        private ThreadLocal<OsuDbContext> threadContexts;

        private readonly object writeLock = new object();

        private bool currentWriteDidWrite;
        private bool currentWriteDidError;

        private int currentWriteUsages;

        private IDbContextTransaction currentWriteTransaction;

        public DatabaseContextFactory(Storage storage)
        {
            this.storage = storage;
            recycleThreadContexts();
        }

        /// <summary>
        /// Get a context for the current thread for read-only usage.
        /// If a <see cref="DatabaseWriteUsage"/> is in progress, the existing write-safe context will be returned.
        /// </summary>
        public OsuDbContext Get() => threadContexts.Value;

        /// <summary>
        /// Request a context for write usage. Can be consumed in a nested fashion (and will return the same underlying context).
        /// This method may block if a write is already active on a different thread.
        /// </summary>
        /// <param name="withTransaction">Whether to start a transaction for this write.</param>
        /// <returns>A usage containing a usable context.</returns>
        public DatabaseWriteUsage GetForWrite(bool withTransaction = true)
        {
            Monitor.Enter(writeLock);
            OsuDbContext context;

            try
            {
                if (currentWriteTransaction == null && withTransaction)
                {
                    // this mitigates the fact that changes on tracked entities will not be rolled back with the transaction by ensuring write operations are always executed in isolated contexts.
                    // if this results in sub-optimal efficiency, we may need to look into removing Database-level transactions in favour of running SaveChanges where we currently commit the transaction.
                    if (threadContexts.IsValueCreated)
                        recycleThreadContexts();

                    context = threadContexts.Value;
                    currentWriteTransaction = context.Database.BeginTransaction();
                }
                else
                {
                    // we want to try-catch the retrieval of the context because it could throw an error (in CreateContext).
                    context = threadContexts.Value;
                }
            }
            catch (Exception e)
            {
                // retrieval of a context could trigger a fatal error.
                Monitor.Exit(writeLock);
                throw;
            }

            Interlocked.Increment(ref currentWriteUsages);

            return new DatabaseWriteUsage(context, usageCompleted) { IsTransactionLeader = currentWriteTransaction != null && currentWriteUsages == 1 };
        }

        private void usageCompleted(DatabaseWriteUsage usage)
        {
            int usages = Interlocked.Decrement(ref currentWriteUsages);

            try
            {
                currentWriteDidWrite |= usage.PerformedWrite;
                currentWriteDidError |= usage.Errors.Any();

                if (usages == 0)
                {
                    if (currentWriteDidError)
                        currentWriteTransaction?.Rollback();
                    else
                        currentWriteTransaction?.Commit();

                    if (currentWriteDidWrite || currentWriteDidError)
                    {
                        // explicitly dispose to ensure any outstanding flushes happen as soon as possible (and underlying resources are purged).
                        usage.Context.Dispose();

                        // once all writes are complete, we want to refresh thread-specific contexts to make sure they don't have stale local caches.
                        recycleThreadContexts();
                    }

                    currentWriteTransaction = null;
                    currentWriteDidWrite = false;
                    currentWriteDidError = false;
                }
            }
            finally
            {
                Monitor.Exit(writeLock);
            }
        }

        private void recycleThreadContexts()
        {
            // Contexts for other threads are not disposed as they may be in use elsewhere. Instead, fresh contexts are exposed
            // for other threads to use, and we rely on the finalizer inside OsuDbContext to handle their previous contexts
            threadContexts?.Value.Dispose();
            threadContexts = new ThreadLocal<OsuDbContext>(CreateContext, true);
        }

        protected virtual OsuDbContext CreateContext() => new OsuDbContext(storage.GetDatabaseConnectionString(database_name))
        {
            Database = { AutoTransactionsEnabled = false }
        };

        public void ResetDatabase()
        {
            lock (writeLock)
            {
                recycleThreadContexts();
                storage.DeleteDatabase(database_name);
            }
        }
    }
}
