// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace osu.Game.Database
{
    public class DatabaseWriteUsage : IDisposable
    {
        public readonly OsuDbContext Context;
        private readonly IDbContextTransaction transaction;
        private readonly Action<DatabaseWriteUsage> usageCompleted;

        public DatabaseWriteUsage(OsuDbContext context, Action<DatabaseWriteUsage> onCompleted)
        {
            Context = context;
            transaction = Context.BeginTransaction();
            usageCompleted = onCompleted;
        }

        public bool PerformedWrite { get; private set; }

        private bool isDisposed;

        protected void Dispose(bool disposing)
        {
            if (isDisposed) return;
            isDisposed = true;

            PerformedWrite |= Context.SaveChanges(transaction) > 0;
            usageCompleted?.Invoke(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DatabaseWriteUsage()
        {
            Dispose(false);
        }
    }
}
