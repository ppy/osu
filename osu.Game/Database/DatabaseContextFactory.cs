// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Platform;

namespace osu.Game.Database
{
    public class DatabaseContextFactory
    {
        private readonly GameHost host;

        private const string database_name = @"client";

        public DatabaseContextFactory(GameHost host)
        {
            this.host = host;
        }

        public OsuDbContext GetContext() => new OsuDbContext(host.Storage.GetDatabaseConnectionString(database_name));

        public void ResetDatabase()
        {
            // todo: we probably want to make sure there are no active contexts before performing this operation.
            host.Storage.DeleteDatabase(database_name);
        }
    }
}
