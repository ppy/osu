using System;
using osu.Framework.Platform;
using osu.Game.Database;
using SQLite.Net;

namespace osu.Game.Input
{
    public class BindingStore : DatabaseBackedStore
    {
        public BindingStore(SQLiteConnection connection, Storage storage = null)
            : base(connection, storage)
        {
        }

        protected override void Prepare(bool reset = false)
        {
            Connection.CreateTable<Binding>();
        }

        protected override Type[] ValidTypes => new[]
        {
            typeof(Binding)
        };

    }
}