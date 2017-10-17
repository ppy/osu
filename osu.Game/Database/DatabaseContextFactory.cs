// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Platform;

namespace osu.Game.Database
{
    public class DatabaseContextFactory
    {
        private readonly GameHost host;

        public DatabaseContextFactory(GameHost host)
        {
            this.host = host;
        }

        public OsuDbContext GetContext() => new OsuDbContext(host.Storage.GetDatabaseConnectionString(@"client"));
    }
}
