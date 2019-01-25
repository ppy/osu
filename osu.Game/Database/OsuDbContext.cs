﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using DatabasedKeyBinding = osu.Game.Input.Bindings.DatabasedKeyBinding;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using osu.Game.Skinning;

namespace osu.Game.Database
{
    public class OsuDbContext : DbContext
    {
        public DbSet<BeatmapInfo> BeatmapInfo { get; set; }
        public DbSet<BeatmapDifficulty> BeatmapDifficulty { get; set; }
        public DbSet<BeatmapMetadata> BeatmapMetadata { get; set; }
        public DbSet<BeatmapSetInfo> BeatmapSetInfo { get; set; }
        public DbSet<DatabasedKeyBinding> DatabasedKeyBinding { get; set; }
        public DbSet<DatabasedSetting> DatabasedSetting { get; set; }
        public DbSet<FileInfo> FileInfo { get; set; }
        public DbSet<RulesetInfo> RulesetInfo { get; set; }
        public DbSet<SkinInfo> SkinInfo { get; set; }
        public DbSet<ScoreInfo> ScoreInfo { get; set; }

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

            Migrate();
        }

        /// <summary>
        /// Create a new OsuDbContext instance.
        /// </summary>
        /// <param name="connectionString">A valid SQLite connection string.</param>
        public OsuDbContext(string connectionString)
        {
            this.connectionString = connectionString;

            var connection = Database.GetDbConnection();
            try
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA journal_mode=WAL;";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                connection.Close();
                throw;
            }
        }

        ~OsuDbContext()
        {
            // DbContext does not contain a finalizer (https://github.com/aspnet/EntityFrameworkCore/issues/8872)
            // This is used to clean up previous contexts when fresh contexts are exposed via DatabaseContextFactory
            Dispose();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder
                // this is required for the time being due to the way we are querying in places like BeatmapStore.
                // if we ever move to having consumers file their own .Includes, or get eager loading support, this could be re-enabled.
                .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.IncludeIgnoredWarning))
                .UseSqlite(connectionString, sqliteOptions => sqliteOptions.CommandTimeout(10))
                .UseLoggerFactory(logger.Value);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BeatmapInfo>().HasIndex(b => b.OnlineBeatmapID).IsUnique();
            modelBuilder.Entity<BeatmapInfo>().HasIndex(b => b.MD5Hash);
            modelBuilder.Entity<BeatmapInfo>().HasIndex(b => b.Hash);

            modelBuilder.Entity<BeatmapSetInfo>().HasIndex(b => b.OnlineBeatmapSetID).IsUnique();
            modelBuilder.Entity<BeatmapSetInfo>().HasIndex(b => b.DeletePending);
            modelBuilder.Entity<BeatmapSetInfo>().HasIndex(b => b.Hash).IsUnique();

            modelBuilder.Entity<SkinInfo>().HasIndex(b => b.Hash).IsUnique();
            modelBuilder.Entity<SkinInfo>().HasIndex(b => b.DeletePending);

            modelBuilder.Entity<DatabasedKeyBinding>().HasIndex(b => new { b.RulesetID, b.Variant });
            modelBuilder.Entity<DatabasedKeyBinding>().HasIndex(b => b.IntAction);

            modelBuilder.Entity<DatabasedSetting>().HasIndex(b => new { b.RulesetID, b.Variant });

            modelBuilder.Entity<FileInfo>().HasIndex(b => b.Hash).IsUnique();
            modelBuilder.Entity<FileInfo>().HasIndex(b => b.ReferenceCount);

            modelBuilder.Entity<RulesetInfo>().HasIndex(b => b.Available);
            modelBuilder.Entity<RulesetInfo>().HasIndex(b => b.ShortName).IsUnique();

            modelBuilder.Entity<BeatmapInfo>().HasOne(b => b.BaseDifficulty);

            modelBuilder.Entity<ScoreInfo>().HasIndex(b => b.OnlineScoreID).IsUnique();
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
#if DEBUG_DATABASE
                    return logLevel > LogLevel.Debug;
#else
                    return logLevel > LogLevel.Information;
#endif
                }

                public IDisposable BeginScope<TState>(TState state) => null;
            }
        }

        public void Migrate() => Database.Migrate();
    }
}
