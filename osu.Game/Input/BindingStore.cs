// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Input.Bindings;
using SQLite.Net;

namespace osu.Game.Input
{
    public class BindingStore : DatabaseBackedStore
    {
        public BindingStore(SQLiteConnection connection, Storage storage = null)
            : base(connection, storage)
        {
        }

        protected override int StoreVersion => 2;

        protected override void PerformMigration(int currentVersion, int targetVersion)
        {
            base.PerformMigration(currentVersion, targetVersion);

            while (currentVersion++ < targetVersion)
            {
                switch (currentVersion)
                {
                    case 1:
                        // cannot migrate; breaking underlying changes.
                        Reset();
                        break;
                }
            }
        }

        protected override void Prepare(bool reset = false)
        {
            Connection.CreateTable<DatabasedKeyBinding>();
        }

        protected override Type[] ValidTypes => new[]
        {
            typeof(DatabasedKeyBinding)
        };

    }
}