// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Database
{
    public class SingletonContextFactory : IDatabaseContextFactory
    {
        private readonly OsuDbContext context;

        public SingletonContextFactory(OsuDbContext context)
        {
            this.context = context;
        }

        public OsuDbContext Get() => context;

        public DatabaseWriteUsage GetForWrite(bool withTransaction = true) => new DatabaseWriteUsage(context, null);
    }
}
