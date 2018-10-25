using osu.Core.Wiki.Sections;

namespace osu.Core.Wiki.Included.Symcol.Sections
{
    public sealed class SymcolChangelog : WikiChangelogSection
    {
        public const string SYMCOL_VERSION = "0.8.6";

        public override string Title => "Changelog";

        protected override string Version => SYMCOL_VERSION;

        protected override string StoragePath => "symcol\\changelogs";

        protected override string FileExtention => ".symcol";

        protected override string VersionChangelog => "-Updated to lazer version 2018.928.0\n\n" +
            "Features:\n\n" +
            "\n\n" +
            "Tweaks / Changes:\n\n" +
            "\n\n" +
            "Fixes:\n\n" +
            "\n\n" +
            "Dev Notes:\n\n" +
            "";
    }
}
