// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.IO;
using osu.Game.Rulesets;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace osu.Game.Database
{
    public class OsuDbContext : DbContext
    {
        public DbSet<BeatmapInfo> BeatmapInfo { get; set; }
        public DbSet<BeatmapDifficulty> BeatmapDifficulty { get; set; }
        public DbSet<BeatmapMetadata> BeatmapMetadata { get; set; }
        public DbSet<BeatmapSetInfo> BeatmapSetInfo { get; set; }
        public DbSet<DatabasedKeyBinding> DatabasedKeyBinding { get; set; }
        public DbSet<FileInfo> FileInfo { get; set; }
        public DbSet<RulesetInfo> RulesetInfo { get; set; }
        private readonly string connectionString;

        private static readonly Lazy<OsuDbLoggerFactory> logger = new Lazy<OsuDbLoggerFactory>(() => new OsuDbLoggerFactory());

        static OsuDbContext()
        {
            // required to initialise native SQLite libraries on some platforms.
            SQLitePCL.Batteries_V2.Init();
        }

        /// <summary>
        /// Create a new in-memory OsuDbContext instance.
        /// </summary>
        public OsuDbContext()
            : this("DataSource=:memory:")
        {
            // required for tooling (see https://wildermuth.com/2017/07/06/Program-cs-in-ASP-NET-Core-2-0).
        }

        /// <summary>
        /// Create a new OsuDbContext instance.
        /// </summary>
        /// <param name="connectionString">A valid SQLite connection string.</param>
        public OsuDbContext(string connectionString)
        {
            this.connectionString = connectionString;

            Database.SetCommandTimeout(new TimeSpan(TimeSpan.TicksPerSecond * 10));

            var connection = Database.GetDbConnection();
            connection.Open();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA journal_mode=WAL;";
                cmd.ExecuteNonQuery();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite(connectionString);
            optionsBuilder.UseLoggerFactory(logger.Value);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BeatmapInfo>().HasIndex(b => b.MD5Hash);
            modelBuilder.Entity<BeatmapInfo>().HasIndex(b => b.Hash);

            modelBuilder.Entity<BeatmapSetInfo>().HasIndex(b => b.DeletePending);
            modelBuilder.Entity<BeatmapSetInfo>().HasIndex(b => b.Hash);

            modelBuilder.Entity<DatabasedKeyBinding>().HasIndex(b => b.Variant);
            modelBuilder.Entity<DatabasedKeyBinding>().HasIndex(b => b.IntAction);

            modelBuilder.Entity<FileInfo>().HasIndex(b => b.Hash).IsUnique();
            modelBuilder.Entity<FileInfo>().HasIndex(b => b.ReferenceCount);

            modelBuilder.Entity<RulesetInfo>().HasIndex(b => b.Available);

            modelBuilder.Entity<BeatmapInfo>().HasOne(b => b.BaseDifficulty);
        }

        private class OsuDbLoggerFactory : ILoggerFactory
        {
            #region Disposal

            public void Dispose()
            {
            }

            #endregion

            public ILogger CreateLogger(string categoryName) => new OsuDbLogger();

            public void AddProvider(ILoggerProvider provider)
            {
                // no-op. called by tooling.
            }

            private class OsuDbLoggerProvider : ILoggerProvider
            {
                #region Disposal

                public void Dispose()
                {
                }

                #endregion

                public ILogger CreateLogger(string categoryName) => new OsuDbLogger();
            }

            private class OsuDbLogger : ILogger
            {
                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                {
                    if (logLevel < LogLevel.Information)
                        return;

                    Framework.Logging.LogLevel frameworkLogLevel;

                    switch (logLevel)
                    {
                        default:
                            frameworkLogLevel = Framework.Logging.LogLevel.Debug;
                            break;
                        case LogLevel.Warning:
                            frameworkLogLevel = Framework.Logging.LogLevel.Important;
                            break;
                        case LogLevel.Error:
                        case LogLevel.Critical:
                            frameworkLogLevel = Framework.Logging.LogLevel.Error;
                            break;
                    }

                    Logger.Log(formatter(state, exception), LoggingTarget.Database, frameworkLogLevel);
                }

                public bool IsEnabled(LogLevel logLevel)
                {
#if DEBUG
                    return logLevel > LogLevel.Debug;
#else
                    return logLevel > LogLevel.Information;
#endif
                }

                public IDisposable BeginScope<TState>(TState state) => null;
            }
        }

        public void Migrate()
        {
            migrateFromSqliteNet();
            Database.Migrate();
        }

        private void migrateFromSqliteNet()
        {
            try
            {
                // will fail if EF hasn't touched the database yet.
                Database.ExecuteSqlCommand("SELECT * FROM __EFMigrationsHistory LIMIT 1");
            }
            catch
            {
                try
                {
                    // will fail (intentionally) if we don't have sqlite-net data present.
                    Database.ExecuteSqlCommand("SELECT OnlineBeatmapSetId FROM BeatmapMetadata LIMIT 1");

                    try
                    {
                        // we are good to perform messy migration of data!.
                        Database.ExecuteSqlCommand("ALTER TABLE BeatmapDifficulty RENAME TO BeatmapDifficulty_Old");
                        Database.ExecuteSqlCommand("ALTER TABLE BeatmapMetadata RENAME TO BeatmapMetadata_Old");
                        Database.ExecuteSqlCommand("ALTER TABLE FileInfo RENAME TO FileInfo_Old");
                        Database.ExecuteSqlCommand("ALTER TABLE KeyBinding RENAME TO KeyBinding_Old");
                        Database.ExecuteSqlCommand("ALTER TABLE BeatmapSetInfo RENAME TO BeatmapSetInfo_Old");
                        Database.ExecuteSqlCommand("ALTER TABLE BeatmapInfo RENAME TO BeatmapInfo_Old");
                        Database.ExecuteSqlCommand("ALTER TABLE BeatmapSetFileInfo RENAME TO BeatmapSetFileInfo_Old");
                        Database.ExecuteSqlCommand("ALTER TABLE RulesetInfo RENAME TO RulesetInfo_Old");

                        Database.ExecuteSqlCommand("DROP TABLE StoreVersion");

                        // perform EF migrations to create sane table structure.
                        Database.Migrate();

                        // copy data table by table to new structure, dropping old tables as we go.
                        Database.ExecuteSqlCommand("INSERT INTO FileInfo SELECT * FROM FileInfo_Old");
                        Database.ExecuteSqlCommand("DROP TABLE FileInfo_Old");

                        Database.ExecuteSqlCommand("INSERT INTO KeyBinding SELECT ID, [Action], Keys, RulesetID, Variant FROM KeyBinding_Old");
                        Database.ExecuteSqlCommand("DROP TABLE KeyBinding_Old");

                        Database.ExecuteSqlCommand(
                            "INSERT INTO BeatmapMetadata SELECT ID, Artist, ArtistUnicode, AudioFile, Author, BackgroundFile, PreviewTime, Source, Tags, Title, TitleUnicode FROM BeatmapMetadata_Old");
                        Database.ExecuteSqlCommand("DROP TABLE BeatmapMetadata_Old");

                        Database.ExecuteSqlCommand(
                            "INSERT INTO BeatmapDifficulty SELECT  `ID`, `ApproachRate`, `CircleSize`, `DrainRate`, `OverallDifficulty`, `SliderMultiplier`, `SliderTickRate` FROM BeatmapDifficulty_Old");
                        Database.ExecuteSqlCommand("DROP TABLE BeatmapDifficulty_Old");

                        Database.ExecuteSqlCommand("INSERT INTO BeatmapSetInfo SELECT ID, DeletePending, Hash, BeatmapMetadataID, OnlineBeatmapSetID, Protected FROM BeatmapSetInfo_Old");
                        Database.ExecuteSqlCommand("DROP TABLE BeatmapSetInfo_Old");

                        Database.ExecuteSqlCommand("INSERT INTO BeatmapSetFileInfo SELECT ID, BeatmapSetInfoID, FileInfoID, Filename FROM BeatmapSetFileInfo_Old");
                        Database.ExecuteSqlCommand("DROP TABLE BeatmapSetFileInfo_Old");

                        Database.ExecuteSqlCommand("INSERT INTO RulesetInfo SELECT ID, Available, InstantiationInfo, Name FROM RulesetInfo_Old");
                        Database.ExecuteSqlCommand("DROP TABLE RulesetInfo_Old");

                        Database.ExecuteSqlCommand(
                            "INSERT INTO BeatmapInfo SELECT ID, AudioLeadIn, BaseDifficultyID, BeatDivisor, BeatmapSetInfoID, Countdown, DistanceSpacing, GridSize, Hash, Hidden, LetterboxInBreaks, MD5Hash, NULLIF(BeatmapMetadataID, 0), OnlineBeatmapID, Path, RulesetID, SpecialStyle, StackLeniency, StarDifficulty, StoredBookmarks, TimelineZoom, Version, WidescreenStoryboard FROM BeatmapInfo_Old");
                        Database.ExecuteSqlCommand("DROP TABLE BeatmapInfo_Old");
                    }
                    catch
                    {
                        // if anything went wrong during migration just nuke the database.
                        throw new MigrationFailedException();
                    }
                }
                catch (MigrationFailedException e)
                {
                    throw;
                }
                catch
                {
                }
            }
        }
    }

    public class MigrationFailedException : Exception
    {
    }
}
