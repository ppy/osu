using Microsoft.EntityFrameworkCore;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.IO;
using osu.Game.Rulesets;

namespace osu.Game.Database
{
    public class OsuDbContext : DbContext
    {
        private readonly string connectionString;

        public OsuDbContext()
        {
            connectionString = "DataSource=:memory:";
        }

        public OsuDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DbSet<BeatmapMetadata> BeatmapMetadata { get; set; }
        public DbSet<BeatmapDifficulty> BeatmapDifficulty { get; set; }
        public DbSet<BeatmapInfo> BeatmapInfo { get; set; }
        public DbSet<BeatmapSetInfo> BeatmapSetInfo { get; set; }
        public DbSet<BeatmapSetFileInfo> BeatmapSetFileInfo { get; set; }
        public DbSet<DatabasedKeyBinding> DatabasedKeyBinding { get; set; }
        public DbSet<FileInfo> FileInfo { get; set; }
        public DbSet<RulesetInfo> RulesetInfo { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<BeatmapInfo>().HasIndex(b => b.MD5Hash);
            modelBuilder.Entity<BeatmapSetInfo>().HasIndex(b => b.DeletePending);
            modelBuilder.Entity<DatabasedKeyBinding>().HasIndex(b => b.Variant);
            modelBuilder.Entity<DatabasedKeyBinding>().HasIndex(b => b.IntAction);
            modelBuilder.Entity<FileInfo>().HasIndex(b => b.Hash).IsUnique();
            modelBuilder.Entity<FileInfo>().HasIndex(b => b.ReferenceCount);
            modelBuilder.Entity<RulesetInfo>().HasIndex(b => b.Name).IsUnique();
            modelBuilder.Entity<RulesetInfo>().HasIndex(b => b.InstantiationInfo).IsUnique();
            modelBuilder.Entity<RulesetInfo>().HasIndex(b => b.Available);
            //modelBuilder.Entity<BeatmapMetadata>().HasOne(b => b.BeatmapSetOnlineInfo).WithOne(bs => bs.BeatmapMetadata).OnDelete(DeleteBehavior.SetNull);
            //modelBuilder.Entity<BeatmapSetInfo>().HasOne(b => b.BeatmapSetOnlineInfo).WithOne(bs => bs.BeatmapSetInfo).OnDelete(DeleteBehavior.SetNull);
            //modelBuilder.Entity<BeatmapInfo>().HasOne(b => b.BeatmapSetOnlineInfo).WithMany(bs => bs.Beatmaps).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
