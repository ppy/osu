namespace osu.Game.Beatmaps
{
    public interface ICanImportArchives
    {
        void Import(params string[] paths);

        string[] HandledExtensions { get; }
    }
}
