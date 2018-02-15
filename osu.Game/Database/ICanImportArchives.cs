namespace osu.Game.Database
{
    public interface ICanImportArchives
    {
        void Import(params string[] paths);

        string[] HandledExtensions { get; }
    }
}
